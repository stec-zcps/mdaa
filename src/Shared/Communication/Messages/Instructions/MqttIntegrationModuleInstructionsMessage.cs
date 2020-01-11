// <copyright file="MqttIntegrationModuleInstructionsMessage.cs" company="Fraunhofer Institute for Manufacturing Engineering and Automation IPA">
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

namespace Mdaa.Communication.Messages.Instructions
{
    using System.Collections.Generic;
    using Mdaa.Communication.Messages;
    using Mdaa.Model.Modules.IntegrationModules.Mqtt;
    using Newtonsoft.Json;

    public class MqttIntegrationModuleInstructionsMessage : InstructionsMessage
    {
        public Dictionary<string, MqttInformation> MqttInfoSubscriptions { get; set; } = new Dictionary<string, MqttInformation>();

        public Dictionary<string, MqttInformation> MqttInfoPublications { get; set; } = new Dictionary<string, MqttInformation>();

        [JsonConstructor]
        public MqttIntegrationModuleInstructionsMessage(string moduleId)
            : base(moduleId)
        {
        }

        public MqttIntegrationModuleInstructionsMessage(string moduleId, Dictionary<string, MqttInformation> infoSubscriptions, Dictionary<string, MqttInformation> infoPublications)
            : base(moduleId)
        {
            this.MqttInfoSubscriptions = infoSubscriptions;
            this.MqttInfoPublications = infoPublications;
        }
    }
}
