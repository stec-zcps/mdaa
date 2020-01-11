// <copyright file="MqttModule.cs" company="Fraunhofer Institute for Manufacturing Engineering and Automation IPA">
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

namespace Manager.Model.InstructionalModel
{
    using Mdaa.Communication.Messages;
    using Mdaa.Communication.Messages.Configuration;
    using Mdaa.Communication.Messages.Instructions;
    using Mdaa.Model.Modules.IntegrationModules.Mqtt;

    public class MqttModule : Module
    {
        public MqttModuleConfiguration Configuration { get; set; }

        public override ConfigurationMessage GetConfigurationMessage()
        {
            MqttIntegrationModuleConfigurationMessage configurationMessage = new MqttIntegrationModuleConfigurationMessage(this.Id, this.GetNextFreePublishingPort(), this.Configuration.BrokerHostname, this.Configuration.Port);

            return configurationMessage;
        }

        public override InstructionsMessage GetInstructionsMessage()
        {
            MqttIntegrationModuleInstructionsMessage instructionsMessage = new MqttIntegrationModuleInstructionsMessage(this.Id);

            foreach (var information in this.InformationToGet)
            {
                instructionsMessage.MqttInfoSubscriptions.Add(
                    information.Key,
                    new MqttInformation(information.Value.Access["Topic"].ToString(), information.Value.Access["Mode"].ToString()));
            }

            foreach (var information in this.InformationToProvide)
            {
                instructionsMessage.MqttInfoPublications.Add(
                    information.Key,
                    new MqttInformation(information.Value.Access["Topic"].ToString(), information.Value.Access["Mode"].ToString(), information.Value.Source));
            }

            return instructionsMessage;
        }
    }
}
