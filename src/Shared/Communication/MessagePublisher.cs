// <copyright file="MessagePublisher.cs" company="Fraunhofer Institute for Manufacturing Engineering and Automation IPA">
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
    using System.Collections.Concurrent;
    using System.Threading;
    using Mdaa.Communication.Messages;
    using NetMQ;
    using NetMQ.Sockets;
    using Newtonsoft.Json;
    using Serilog;

    public class MessagePublisher
    {
        private readonly NetMQActor actor;

        private XPublisherSocket publisherSocket;

        private NetMQTimer heartbeatTimer;

        private NetMQPoller poller;

        public MessagePublisher(uint port)
        {
            this.Port = port;

            this.actor = NetMQActor.Create(this.Run);
        }

        public uint Port { get; private set; }

        public TimeSpan HeartbeatInterval { get; set; } = TimeSpan.FromSeconds(2);

        private void Run(PairSocket shim)
        {
            var address = $"tcp://*:{this.Port}";

            using (this.publisherSocket = new XPublisherSocket())
            {
                this.publisherSocket.SetWelcomeMessage(MessageCommand.CONNECTED);
                this.publisherSocket.Bind(address);

                this.publisherSocket.ReceiveReady += this.DropPublisherSubscriptions;

                this.heartbeatTimer = new NetMQTimer(this.HeartbeatInterval);
                this.heartbeatTimer.Elapsed += this.OnHeartbeatTimerElapsed;

                shim.ReceiveReady += this.OnShimMessage;

                // Signal the actor that the shim is ready to work
                shim.SignalOK();

                this.poller = new NetMQPoller { this.publisherSocket, shim, this.heartbeatTimer };

                // Polling until poller is cancelled
                this.poller.Run();
            }
        }

        private void OnHeartbeatTimerElapsed(object sender, NetMQTimerEventArgs e)
        {
            // Heartbeat timer elapsed, let's send another heartbeat
            this.publisherSocket.SendFrame(MessageCommand.HEARTBEAT);
        }

        private void OnShimMessage(object sender, NetMQSocketEventArgs e)
        {
            string command = e.Socket.ReceiveFrameString();

            if (command == MessageCommand.PUBLISH)
            {
                // just forward the message to the publisher
                NetMQMessage message = e.Socket.ReceiveMultipartMessage();
                this.publisherSocket.SendMultipartMessage(message);
                Log.Debug($"Message published: {message}");
            }
            else if (command == NetMQActor.EndShimMessage)
            {
                // we got dispose command, we just stop the poller
                this.poller.Stop();
            }
        }

        private void DropPublisherSubscriptions(object sender, NetMQSocketEventArgs e)
        {
            // just drop the subscription messages, we have to do that to Welcome message to work
            this.publisherSocket.SkipMultipartMessage();
        }

        public void PublishMessage(Message message)
        {
            // we can use actor like NetMQSocket
            var netMQMessage = new NetMQMessage();
            netMQMessage.Append(message.Topic);
            netMQMessage.Append(JsonConvert.SerializeObject(message));
            this.actor.SendMoreFrame(MessageCommand.PUBLISH).SendMultipartMessage(netMQMessage);
        }

        public void PublishMessage(Message messageToPublish, string topic)
        {
            messageToPublish.Topic = topic;
            this.PublishMessage(messageToPublish);
        }
    }
}