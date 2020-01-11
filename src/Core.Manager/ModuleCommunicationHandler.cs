// <copyright file="ModuleCommunicationHandler.cs" company="Fraunhofer Institute for Manufacturing Engineering and Automation IPA">
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

namespace Fraunhofer.IPA.DataAggregator.Manager
{
    using System.Collections.Concurrent;
    using System.Text;
    using System.Threading;
    using Mdaa.Communication.Messages;
    using NetMQ;
    using NetMQ.Sockets;
    using Newtonsoft.Json;
    using Serilog;

    public class ModuleCommunicationHandler
    {
        private bool run = false;
        private Thread runThread;
        private uint ResponsePort;

        public event NewRegistrationMessageReceivedHandler OnNewRegistrationMessageReceived;

        public delegate void NewRegistrationMessageReceivedHandler(RegistrationMessage newRegistrationMessage);

        private ConcurrentQueue<NetMQMessage> messagesToSend = new ConcurrentQueue<NetMQMessage>();

        public ModuleCommunicationHandler(uint port)
        {
            this.ResponsePort = port;
        }

        public void Start()
        {
            Log.Information($"Started listening on port '{this.ResponsePort}'");
            this.runThread = new Thread(this.ListenForNewRequests);
            this.run = true;
            this.runThread.Start();
        }

        public void Stop()
        {
            Log.Information("Stopped");
            this.run = false;
        }

        public void SendConfigurationToModule(string moduleId, ConfigurationMessage configurationMessage)
        {
            var configurationNetMQMessage = new NetMQMessage();
            configurationNetMQMessage.Append(Encoding.Unicode.GetBytes(moduleId));
            configurationNetMQMessage.AppendEmptyFrame();
            configurationNetMQMessage.Append("CONFIGURATION");
            configurationNetMQMessage.Append(JsonConvert.SerializeObject(configurationMessage));

            this.messagesToSend.Enqueue(configurationNetMQMessage);
        }

        public void SendInstructionsToModule(string moduleId, InstructionsMessage instructionsMessage)
        {
            var instructionsNetMQMessage = new NetMQMessage();
            instructionsNetMQMessage.Append(Encoding.Unicode.GetBytes(moduleId));
            instructionsNetMQMessage.AppendEmptyFrame();
            instructionsNetMQMessage.Append("INSTRUCTIONS");
            instructionsNetMQMessage.Append(JsonConvert.SerializeObject(instructionsMessage));

            this.messagesToSend.Enqueue(instructionsNetMQMessage);
        }

        public void SendRegistrationSuccessMessageToModule(string moduleId)
        {
            var instructionsNetMQMessage = new NetMQMessage();
            instructionsNetMQMessage.Append(Encoding.Unicode.GetBytes(moduleId));
            instructionsNetMQMessage.AppendEmptyFrame();
            instructionsNetMQMessage.Append("REGISTERED");
            instructionsNetMQMessage.AppendEmptyFrame();

            this.messagesToSend.Enqueue(instructionsNetMQMessage);
        }

        public void SendRegistrationFailedMessageToModule(string moduleId)
        {
            var instructionsNetMQMessage = new NetMQMessage();
            instructionsNetMQMessage.Append(Encoding.Unicode.GetBytes(moduleId));
            instructionsNetMQMessage.AppendEmptyFrame();
            instructionsNetMQMessage.Append("REGISTRATION_ERROR");
            instructionsNetMQMessage.AppendEmptyFrame();

            this.messagesToSend.Enqueue(instructionsNetMQMessage);
        }

        private void ListenForNewRequests()
        {
            using (var server = new RouterSocket($"@tcp://*:{this.ResponsePort}"))
            {
                while (this.run)
                {
                    if (server.HasIn)
                    {
                        var receivedMultipartMessage = server.ReceiveMultipartMessage();
                        if (receivedMultipartMessage.FrameCount == 4)
                        {
                            var clientAddress = receivedMultipartMessage[0].ConvertToString();
                            var command = receivedMultipartMessage[2].ConvertToString();
                            var message = receivedMultipartMessage[3].ConvertToString();
                            switch (command)
                            {
                                case "CONNECT":
                                    var connectedMessage = new NetMQMessage();
                                    connectedMessage.Append(clientAddress);
                                    connectedMessage.AppendEmptyFrame();
                                    connectedMessage.Append("CONNECTED");
                                    connectedMessage.AppendEmptyFrame();
                                    server.SendMultipartMessage(connectedMessage);
                                    break;

                                case "REGISTER":
                                    var registrationMessage = JsonConvert.DeserializeObject<RegistrationMessage>(message);
                                    this.OnNewRegistrationMessageReceived?.Invoke(registrationMessage);
                                    break;

                                case "PING":
                                    var pongMessage = new NetMQMessage();
                                    pongMessage.Append(clientAddress);
                                    pongMessage.AppendEmptyFrame();
                                    pongMessage.Append("PONG");
                                    pongMessage.AppendEmptyFrame();
                                    server.SendMultipartMessage(pongMessage);
                                    break;

                                default:
                                    Log.Warning($"Unkown command '{command}' and message '{message}'");
                                    var messageToClient = new NetMQMessage();
                                    messageToClient.Append(clientAddress);
                                    messageToClient.AppendEmptyFrame();
                                    messageToClient.Append("MESSAGE");
                                    messageToClient.Append($"Unkown command '{command}'");
                                    server.SendMultipartMessage(messageToClient);
                                    break;
                            }
                        }
                    }

                    if (this.messagesToSend.Count > 0)
                    {
                        NetMQMessage messageToSend = new NetMQMessage();
                        if (this.messagesToSend.TryDequeue(out messageToSend))
                        {
                            Log.Information($"Sending message to client '{messageToSend[0].ConvertToString()}': {messageToSend[2].ConvertToString()}");
                            server.SendMultipartMessage(messageToSend);
                        }
                    }
                }
            }
        }
    }
}