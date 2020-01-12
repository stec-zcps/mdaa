// <copyright file="Module.cs" company="Fraunhofer Institute for Manufacturing Engineering and Automation IPA">
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

namespace Mdaa.Model.Modules
{
    using System.Collections.Generic;
    using JsonSubTypes;
    using Mdaa.Communication.Messages;
    using Mdaa.Model.Informations;
    using Mdaa.Model.Modules.IntegrationModules.Mqtt;
    using Mdaa.Model.Modules.IntegrationModules.OpcUaClient;
    using Mdaa.Model.Modules.IntegrationModules.OpcUaServer;
    using Mdaa.Model.Modules.OperationModules.Aggregation;
    using Mdaa.Model.Modules.OperationModules.Math;
    using Newtonsoft.Json;

    [JsonConverter(typeof(JsonSubtypes), "Type")]
    [JsonSubtypes.KnownSubType(typeof(MqttIntegrationModule), ModuleType.MqttIntegrationModule)]
    [JsonSubtypes.KnownSubType(typeof(OpcUaClientIntegrationModule), ModuleType.OpcUaClientIntegrationModule)]
    [JsonSubtypes.KnownSubType(typeof(OpcUaServerIntegrationModule), ModuleType.OpcUaServerIntegrationModule)]
    [JsonSubtypes.KnownSubType(typeof(MathOperationModule), ModuleType.MathOperationModule)]
    [JsonSubtypes.KnownSubType(typeof(AggregationOperationModule), ModuleType.AggregationOperationModule)]
    public class Module
    {
        protected static uint NextFreePublishingPort = 40100;

        public string Id { get; set; }

        public ModuleType Type { get; set; }

        public bool Managed { get; set; }

        [JsonIgnore]
        public Dictionary<string, InformationToGet> InformationToGet { get; set; } = new Dictionary<string, InformationToGet>();

        [JsonIgnore]
        public Dictionary<string, InformationToProvide> InformationToProvide { get; set; } = new Dictionary<string, InformationToProvide>();

        public virtual ConfigurationMessage GetConfigurationMessage()
        {
            var configurationMessage = new ConfigurationMessage(this.Id, this.GetNextFreePublishingPort());
            return configurationMessage;
        }

        public void AddInformationToGet(InformationToGet newGatheredInformation)
        {
            this.InformationToGet.Add(newGatheredInformation.Id, newGatheredInformation);
        }

        public void AddInformationToProvide(InformationToProvide newProvidedInformation)
        {
            this.InformationToProvide.Add(newProvidedInformation.Id, newProvidedInformation);
        }

        public virtual InstructionsMessage GetInstructionsMessage()
        {
            var instructionsMessage = new InstructionsMessage(this.Id);
            return instructionsMessage;
        }

        protected uint GetNextFreePublishingPort()
        {
            uint port = NextFreePublishingPort;
            NextFreePublishingPort++;
            return port;
        }
    }
}
