﻿// <copyright file="OpcUaServerModuleInstructionsMessage.cs" company="Fraunhofer Institute for Manufacturing Engineering and Automation IPA">
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
    using Mdaa.Model.Modules.IntegrationModules.OpcUa.Server;

    public class OpcUaServerModuleInstructionsMessage : InstructionsMessage
    {
        public Dictionary<string, OpcUaServerNode> OpcUaNodes { get; set; } = new Dictionary<string, OpcUaServerNode>();

        public OpcUaServerModuleInstructionsMessage(string moduleId)
            : base(moduleId)
        {
        }

        public void AddInformation(string informationKey, OpcUaServerNode opcUaNode)
        {
            this.OpcUaNodes.Add(informationKey, opcUaNode);
        }
    }
}
