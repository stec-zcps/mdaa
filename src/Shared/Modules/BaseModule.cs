// <copyright file="BaseModule.cs" company="Fraunhofer Institute for Manufacturing Engineering and Automation IPA">
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

namespace Mdaa.Modules
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Timers;
    using JsonSubTypes;
    using Mdaa.Communication.Messages;
    using NetMQ;
    using NetMQ.Sockets;
    using Newtonsoft.Json;
    using Serilog;

    [JsonConverter(typeof(JsonSubtypes), "Type")]
    public abstract class BaseModule
    {
        private bool run = false;

        private Thread runThread;

        private DealerSocket socket;

        private System.Timers.Timer heartbeatCheckTimer;

        protected static uint NextFreePublishingPort = 40100;

        public BaseModule()
        {
        }

        public BaseModule(string moduleId, string moduleIp, string managerHost, uint managerRequestPort, uint managerPublishPort, string dataRouterHost, uint dataRouterPublishPort)
        {
            this.ModuleId = moduleId;
            this.ModuleIp = moduleIp;
            this.ManagerHost = managerHost;
            this.ManagerRequestPort = managerRequestPort;
            this.ManagerPublishPort = managerPublishPort;
            this.DataRouterHost = dataRouterHost;
            this.DataRouterPublishPort = dataRouterPublishPort;
        }

        public string ModuleId { get; protected set; }

        public string ModuleType { get; protected set; }

        public string ModuleState { get; protected set; }

        public string ModuleIp { get; protected set; }

        public uint PublishingPort { get; protected set; }

        public string ManagerHost { get; protected set; }

        public uint ManagerRequestPort { get; protected set; }

        public uint ManagerPublishPort { get; protected set; }

        public string DataRouterHost { get; protected set; }

        public uint DataRouterPublishPort { get; protected set; }

        public double ManagerConnectionTimeoutInSeconds { get; set; } = 5;

        public long HeartbeatIntervalInMilliseconds { get; set; } = 2000;

        public bool AutoReconnect { get; set; } = true;

        public int AutoReconnectInterval { get; set; } = 5000;

        public void Start()
        {
            if (this.Connect())
            {
                this.Register();
                this.runThread = new Thread(this.ListenForMessages);
                this.run = true;
                this.runThread.Start();
            }
            else
            {
                Log.Information($"Connection to manager '{this.ManagerHost}' failed");
                System.Environment.Exit(1);
            }
        }

        public void Stop()
        {
            this.run = false;
            this.socket.Dispose();
        }

        protected abstract void NewConfigurationMessageReceived(string configurationMessageString);

        protected abstract void NewInstructionsMessageReceived(string instructionsMessageString);

        protected bool Connect()
        {
            this.socket = new DealerSocket();
            this.socket.Options.Identity = Encoding.Unicode.GetBytes(this.ModuleId);
            this.socket.Options.ReconnectInterval = TimeSpan.FromMilliseconds(this.AutoReconnectInterval);
            this.socket.Connect($"tcp://{this.ManagerHost}:{this.ManagerRequestPort}");
            Thread.Sleep(10);
            var connectMessage = new NetMQMessage();
            connectMessage.AppendEmptyFrame();
            connectMessage.Append("CONNECT");
            connectMessage.AppendEmptyFrame();
            NetMQMessage receivedData = new NetMQMessage();
            if (this.socket.TrySendMultipartMessage(TimeSpan.FromSeconds(this.ManagerConnectionTimeoutInSeconds), connectMessage) &&
            this.socket.TryReceiveMultipartMessage(TimeSpan.FromSeconds(this.ManagerConnectionTimeoutInSeconds), ref receivedData))
            {
                if (receivedData.FrameCount == 3)
                {
                    var message = receivedData[1].ConvertToString();
                    if (message == "CONNECTED")
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected void Register()
        {
            RegistrationMessage regMsg = new RegistrationMessage(this.ModuleId);
            var messageToServer = new NetMQMessage();
            messageToServer.AppendEmptyFrame();
            messageToServer.Append("REGISTER");
            messageToServer.Append(JsonConvert.SerializeObject(regMsg));
            this.socket.SendMultipartMessage(messageToServer);
        }

        protected void ListenForMessages()
        {
            this.heartbeatCheckTimer = new System.Timers.Timer(2 * this.HeartbeatIntervalInMilliseconds);

            // Hook up the Elapsed event for the timer.
            this.heartbeatCheckTimer.Elapsed += this.OnHeartbeatMissing;
            this.heartbeatCheckTimer.Enabled = true;
            this.heartbeatCheckTimer.Start();

            var pingMessage = new NetMQMessage();
            pingMessage.AppendEmptyFrame();
            pingMessage.Append("PING");
            pingMessage.AppendEmptyFrame();
            this.socket.SendMultipartMessage(pingMessage);

            while (this.run)
            {
                if (this.socket.HasIn)
                {
                    var receivedMultipartMessage = this.socket.ReceiveMultipartMessage();
                    if (receivedMultipartMessage.FrameCount == 3)
                    {
                        var receivedCommand = receivedMultipartMessage[1].ConvertToString();
                        var receivedMessage = receivedMultipartMessage[2].ConvertToString();
                        switch (receivedCommand)
                        {
                            case "CONFIGURATION":
                                Log.Information($"Configuration received: \n{receivedCommand}\n{receivedMessage}");
                                this.NewConfigurationMessageReceived(receivedMessage);
                                break;

                            case "INSTRUCTIONS":
                                Log.Information($"Instructions received: \n{receivedCommand}\n{receivedMessage}");
                                this.NewInstructionsMessageReceived(receivedMessage);
                                break;

                            case "PONG":
                                this.socket.SendMultipartMessage(pingMessage);
                                this.heartbeatCheckTimer.Stop();
                                this.heartbeatCheckTimer.Start();
                                break;

                            default:
                                Log.Information($"Received message: \n{receivedCommand}\n{receivedMessage}");
                                break;
                        }
                    }
                }
            }
        }

        private void OnHeartbeatMissing(object sender, ElapsedEventArgs e)
        {
            // Log.Error("No connection to server");
        }
    }
}
