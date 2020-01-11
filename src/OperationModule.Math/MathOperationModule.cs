// <copyright file="MathOperationModule.cs" company="Fraunhofer Institute for Manufacturing Engineering and Automation IPA">
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

namespace Fraunhofer.IPA.DataAggregator.Modules.OperationModules.Math
{
    using Mdaa.Communication;
    using Mdaa.Communication.Messages;
    using Mdaa.Communication.Messages.Configuration;
    using Mdaa.Communication.Messages.Instructions;
    using Mdaa.Modules;
    using Newtonsoft.Json;
    using Serilog;

    public class MathOperationModule : BaseOperationModule
    {
        private MessagePublisher messagePublisher;

        public MathOperationModule(string moduleId, string moduleIp, string managerHost, uint managerRequestPort, uint managerPublishPort, string dataRouterHost, uint dataRouterPublishPort)
            : base(moduleId, moduleIp, managerHost, managerRequestPort, managerPublishPort, dataRouterHost, dataRouterPublishPort)
        {
            Log.Information("MathOperationModule started");
        }

        public void OnNewResultCalculated(InformationMessage newResultInformation)
        {
            Log.Debug("New result calculated: " + newResultInformation);
            this.messagePublisher.PublishMessage(newResultInformation, "NewInformation");
        }

        protected override void NewConfigurationMessageReceived(string configurationMessageString)
        {
            var newConfigMessage = JsonConvert.DeserializeObject<MathOperationModuleConfigurationMessage>(configurationMessageString);

            Log.Information("Config received");
            this.messagePublisher = new MessagePublisher(newConfigMessage.PublishingPort);
        }

        protected override void NewInstructionsMessageReceived(string instructionsMessageString)
        {
            var newInstructionsMessage = JsonConvert.DeserializeObject<MathOperationModuleInstructionsMessage>(instructionsMessageString);

            foreach (var mathOperatorConfig in newInstructionsMessage.MathOperatorConfigs)
            {
                MathOperator newMathOperator = new MathOperator(this.DataRouterHost, this.DataRouterPublishPort, mathOperatorConfig);
                newMathOperator.OnNewResultCalculated += this.OnNewResultCalculated;
                newMathOperator.Start();
            }
        }
    }
}
