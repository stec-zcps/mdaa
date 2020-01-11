// <copyright file="MessageSubscriber.cs" company="Fraunhofer Institute for Manufacturing Engineering and Automation IPA">
// Copyright 2019 Fraunhofer Institute for Manufacturing Engineering and Automation IPA
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

namespace Mdaa.Communication
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Mdaa.Communication.Messages;
    using NetMQ;
    using NetMQ.Sockets;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Serilog;

    public class MessageSubscriber<T>
        where T : Message
    {
        private readonly NetMQActor actor;

        private readonly List<string> subscriptions = new List<string>();

        public delegate void NewMessageReceivedHandler(T newMessage);

        public event EventHandler<EventArgs> Connected;

        public event EventHandler<EventArgs> ConnectFailed;

        public event NewMessageReceivedHandler NewMessageReceived;

        private string host;

        private uint port;

        private string topic;

        private NetMQPoller poller;

        private NetMQTimer timeoutTimer;

        private NetMQTimer reconnectTimer;

        private SubscriberSocket subscriberSocket;

        private PairSocket shim;

        public string SubscriberAddress { get; private set; }

        public TimeSpan HeartbeatTimeout { get; set; } = TimeSpan.FromSeconds(5);

        public TimeSpan ReconnectInterval { get; set; } = TimeSpan.FromSeconds(10);

        public MessageSubscriber(string host, uint port, string topic)
        {
            this.host = host;
            this.port = port;
            this.topic = topic;

            this.actor = NetMQActor.Create(this.Run);
        }

        protected void Connect()
        {
            var address = $"tcp://{this.host}:{this.port}";
            this.reconnectTimer.Enable = false;
            this.timeoutTimer.Enable = false;

            var poller = new NetMQPoller();
            SubscriberSocket connectedSocket = null;

            // Event handler to handle message from socket
            EventHandler<NetMQSocketEventArgs> handleMessage = (sender, args) =>
            {
                connectedSocket = (SubscriberSocket)args.Socket;
                poller.Stop();
            };

            // In case of timeout, just cancel the poller without seting the connected socket
            var timeoutTimer = new NetMQTimer(this.HeartbeatTimeout);
            timeoutTimer.Elapsed += (sender, args) => poller.Stop();
            poller.Add(timeoutTimer);

            var socket = new SubscriberSocket();
            socket.ReceiveReady += handleMessage;
            poller.Add(socket);

            // Subscribe to welcome message
            socket.Subscribe(MessageCommand.CONNECTED);
            socket.Connect(address);

            poller.Run();

            // If we have a connected socket the connection attempt succeed
            if (connectedSocket != null)
            {
                // Set the socket
                this.subscriberSocket = connectedSocket;

                // Subscribe to heartbeat
                this.subscriberSocket.Subscribe(MessageCommand.HEARTBEAT);

                // Subscribe to topic of this subscriber
                this.Subscribe(this.topic);

                this.subscriberSocket.ReceiveReady -= handleMessage;
                this.subscriberSocket.ReceiveReady += this.OnSubscriberMessage;
                this.actor.ReceiveReady += this.OnActorMessage;
                this.poller.Add(this.actor);
                this.poller.Add(this.subscriberSocket);
                this.timeoutTimer.EnableAndReset();
                Log.Debug($"Connection attempt to '{address}' successfull, waiting for welcome message.");
            }
            else
            {
                // Close  exsiting connection
                socket.Options.Linger = TimeSpan.Zero;
                socket.Dispose();
                this.reconnectTimer.EnableAndReset();
                Log.Debug($"Connection attempt to '{address}' failed");
                this.ConnectFailed?.Invoke(this, new EventArgs());
            }
        }

        protected void Subscribe(string topic)
        {
            this.actor.SendMoreFrame(MessageCommand.SUBSCRIBE).SendFrame(topic);
        }

        private void Run(PairSocket shim)
        {
            this.shim = shim;
            this.shim.ReceiveReady += this.OnShimMessage;

            this.timeoutTimer = new NetMQTimer(this.HeartbeatTimeout);
            this.timeoutTimer.Elapsed += this.OnTimeoutTimer;

            this.reconnectTimer = new NetMQTimer(this.ReconnectInterval);
            this.reconnectTimer.Elapsed += this.OnReconnectTimer;

            this.poller = new NetMQPoller { shim, this.timeoutTimer, this.reconnectTimer };

            this.shim.SignalOK();

            this.Connect();
            this.poller.Run();
            this.subscriberSocket?.Dispose();
        }

        private void OnReconnectTimer(object sender, NetMQTimerEventArgs e)
        {
            Log.Debug("Trying to reconnect...");
            this.Connect();
        }

        private void OnTimeoutTimer(object sender, NetMQTimerEventArgs e)
        {
            Log.Debug("Connection timed out");

            // Dispose the current subscriber socket and try to connect
            this.actor.ReceiveReady -= this.OnActorMessage;
            this.poller.Remove(this.actor);
            this.poller.Remove(this.subscriberSocket);

            this.subscriberSocket.Options.Linger = TimeSpan.Zero;
            this.subscriberSocket.Dispose();
            this.subscriberSocket = null;

            this.Connect();
        }

        private void OnShimMessage(object sender, NetMQSocketEventArgs e)
        {
            string command = e.Socket.ReceiveFrameString();

            if (command == NetMQActor.EndShimMessage)
            {
                this.poller.Stop();
            }
            else if (command == MessageCommand.SUBSCRIBE)
            {
                string topic = e.Socket.ReceiveFrameString();
                Log.Debug($"Subscribing to topic '{topic}'");
                this.subscriptions.Add(topic);
                this.subscriberSocket?.Subscribe(topic);
            }
        }

        private void OnActorMessage(object sender, NetMQActorEventArgs e)
        {
            NetMQMessage message = null;
            while (e.Actor.TryReceiveMultipartMessage(ref message))
            {
                Log.Debug(message.ToString());
                try
                {
                    var receivedTopic = message[0].ConvertToString();
                    var receivedContent = message[1].ConvertToString();
                    Log.Verbose($"Message received on topic (unchecked) '{receivedTopic}': {receivedContent}");
                    if (receivedTopic.Equals(this.topic))
                    {
                        Log.Debug($"Message received on topic '{receivedTopic}': {receivedContent}");
                        try
                        {
                            var format = "yyyy-MM-ddTH:mm:ss.fffZ";
                            var dateTimeConverter = new IsoDateTimeConverter { DateTimeFormat = format };
                            T receivedMessageDeserialized = JsonConvert.DeserializeObject<T>(receivedContent, dateTimeConverter);
                            this.NewMessageReceived(receivedMessageDeserialized);
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"Unable to deserialize message: {ex}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Error during received of message: {message[0].ConvertToString()} -------- {ex}");
                }
            }
        }

        private void OnSubscriberMessage(object sender, NetMQSocketEventArgs e)
        {
            var message = this.subscriberSocket.ReceiveMultipartMessage();
            var topic = message[0].ConvertToString();

            if (topic == MessageCommand.CONNECTED)
            {
                this.SubscriberAddress = e.Socket.Options.LastEndpoint;
                Log.Debug($"Connected to '{this.SubscriberAddress}'");
                this.Connected?.Invoke(this, new EventArgs());
            }
            else if (topic == MessageCommand.HEARTBEAT)
            {
                // Heartbeat received, postpone hearbeat timer
                this.timeoutTimer.EnableAndReset();
                Log.Verbose("Heartbeat received: " + topic);
            }
            else
            {
                Log.Debug(message.ToString());
                this.shim.SendMultipartMessage(message);
            }
        }
    }
}
