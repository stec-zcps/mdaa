// <copyright file="InformationSubscriber.cs" company="Fraunhofer Institute for Manufacturing Engineering and Automation IPA">
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

namespace Fraunhofer.IPA.DataAggregator.DataRouter
{
    using System;
    using System.Threading;
    using Mdaa.Communication.Messages;
    using NetMQ;
    using NetMQ.Sockets;
    using Newtonsoft.Json;
    using Serilog;

    public class InformationSubscriber
    {
        private Thread listenerThread;

        private string host;

        private uint port;

        public delegate void NewInformationReceivedHandler(InformationMessage newInformation);

        public event NewInformationReceivedHandler OnNewInformationReceived;

        public readonly string Topic_NewInformation = "NewInformation";

        public InformationSubscriber(string host, uint port)
        {
            this.host = host;
            this.port = port;
            this.listenerThread = new Thread(this.ListenForNewInformation);
            this.listenerThread.Start();
        }

        private void ListenForNewInformation()
        {
            using (var subSocket = new SubscriberSocket())
            {
                subSocket.Options.ReceiveHighWatermark = 1000;
                string zeroMqAddress = "tcp://" + this.host + ":" + this.port;
                subSocket.Connect(zeroMqAddress);
                subSocket.Subscribe(this.Topic_NewInformation);
                Console.WriteLine($"Subscribing to '{this.Topic_NewInformation}' on '{zeroMqAddress}'");
                while (true)
                {
                    string messageTopicReceived = subSocket.ReceiveFrameString();
                    string messageReceived = subSocket.ReceiveFrameString();
                    InformationMessage receivedInformation = JsonConvert.DeserializeObject<InformationMessage>(messageReceived);
                    this.OnNewInformationReceived(receivedInformation);
                    Log.Debug("Test");
                }
            }
        }
    }
}
