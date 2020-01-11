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

namespace Mdaa.Model.Modules.OperationModules
{
    using System.Collections.Generic;
    using Mdaa.Communication.Messages;
    using Mdaa.Communication.Messages.Configuration;
    using Mdaa.Communication.Messages.Instructions;
    using Mdaa.Model.Modules.OperationModules.Math;

    public class MathOperationModule : OperationModule
    {
        public override ConfigurationMessage GetConfigurationMessage()
        {
            MathOperationModuleConfigurationMessage configurationMessage = new MathOperationModuleConfigurationMessage(this.Id, this.GetNextFreePublishingPort());

            return configurationMessage;
        }

        public override InstructionsMessage GetInstructionsMessage()
        {
            var mathOperationConfigs = new List<MathOperatorConfig>();
            foreach (var operation in this.Operations.Values)
            {
                var mathOperationConfig = new MathOperatorConfig(operation.Result, operation.Description, operation.Variables);
                mathOperationConfigs.Add(mathOperationConfig);
            }

            var instructionsMessage = new MathOperationModuleInstructionsMessage(this.Id, mathOperationConfigs);
            return instructionsMessage;
        }
    }
}
