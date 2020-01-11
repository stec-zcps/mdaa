// <copyright file="DataRouter.cs" company="Fraunhofer Institute for Manufacturing Engineering and Automation IPA">
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
    using System.Collections.Generic;
    using Mdaa.Communication;
    using Mdaa.Communication.Messages;
    using Serilog;

    public class DataRouter
    {
        public uint PublishingPort { get; private set; }

        public string ManagerHost { get; private set; }

        public uint ManagerPort { get; private set; }

        public Dictionary<uint, MessageSubscriber<InformationMessage>> RegisteredModules = new Dictionary<uint, MessageSubscriber<InformationMessage>>();

        private Dictionary<string, object> data = new Dictionary<string, object>();

        private MessagePublisher informationPublisher;

        public DataRouter(uint publishingPort, string managerHost, uint managerPort)
        {
            this.PublishingPort = publishingPort;
            this.ManagerHost = managerHost;
            this.ManagerPort = managerPort;
        }

        public void Run()
        {
            MessageSubscriber<RegistrationMessage> registrationSubscriber = new MessageSubscriber<RegistrationMessage>(this.ManagerHost, this.ManagerPort, "NewRegistration");
            registrationSubscriber.NewMessageReceived += this.OnNewRegistrationReceived;

            this.informationPublisher = new MessagePublisher(this.PublishingPort);
        }

        public void OnNewInformationReceived(InformationMessage newInformation)
        {
            Log.Information($"Received message: '{newInformation.Key}' : {newInformation.Value}");
            this.informationPublisher.PublishMessage(newInformation);
        }

        public void OnNewRegistrationReceived(RegistrationMessage newRegistrationMessage)
        {
            Log.Debug($"New registraion received: {newRegistrationMessage}");
            Log.Debug($"New registraion received: {newRegistrationMessage.Ip}");
            Log.Debug($"New registraion received: {newRegistrationMessage.Port}");

            // if (RegisteredModules.ContainsKey(newRegistrationMessage.Port))
            // {
            //    RegisteredModules[newRegistrationMessage.Port].StopListening();
            // }
            MessageSubscriber<InformationMessage> newInformationSubscriber = new MessageSubscriber<InformationMessage>(newRegistrationMessage.Ip, newRegistrationMessage.Port, "NewInformation");
            newInformationSubscriber.NewMessageReceived += this.OnNewInformationReceived;
            this.RegisteredModules[newRegistrationMessage.Port] = newInformationSubscriber;
        }
    }
}
