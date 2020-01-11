// <copyright file="ModuleDockerContainerController.cs" company="Fraunhofer Institute for Manufacturing Engineering and Automation IPA">
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

namespace Mdaa.Manager.Controller.Docker
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fraunhofer.IPA.DataAggregator.Manager;
    using global::Docker.DotNet.Models;
    using global::Manager.Model.InstructionalModel;

    public class ModuleDockerContainerController : DockerContainerController
    {
        public readonly Dictionary<Type, string> ModuleDockerImageRepositories = new Dictionary<Type, string>()
        {
            { typeof(MqttModule), "mdaastec/integration-module-mqtt" },
            { typeof(OpcUaClientModule), "mdaastec/integration-module-opcua-client" },
            { typeof(OpcUaServerModule), "mdaastec/integration-module-opcua-server" },
            { typeof(MathOperationModule), "mdaastec/operation-module-math" },
            { typeof(AggregationModule), "mdaastec/operation-module-aggregation" },
        };

        public readonly Dictionary<Type, string> ModuleDockerImageTags = new Dictionary<Type, string>()
        {
            { typeof(MqttModule), "1.0" },
            { typeof(OpcUaClientModule), "1.0" },
            { typeof(OpcUaServerModule), "1.0" },
            { typeof(MathOperationModule), "1.0" },
            { typeof(AggregationModule), "1.0" },
        };

        public ModuleDockerContainerController(InstructionalModel instructionalModel)
        {
            this.InstructionalModel = instructionalModel;
        }

        public InstructionalModel InstructionalModel { get; private set; }

        public async Task Init()
        {
            await this.CreateNetworkIfNotExist($"{ContainerPrefix}{this.InstructionalModel.Id}").ConfigureAwait(true);

            foreach (var module in this.InstructionalModel.Modules)
            {
                if (module.Managed)
                {
                    await this.RemoveModule(module).ConfigureAwait(true);
                    await this.StartModule(module).ConfigureAwait(true);
                }
            }
        }

        public async Task StartModule(Module module)
        {
            var moduleDockerImageRepository = this.ModuleDockerImageRepositories[module.GetType()];
            var moduleDockerImageTag = this.ModuleDockerImageTags[module.GetType()];
            var environmentVariables = new List<string>()
            {
                $"ModuleId={this.InstructionalModel.Id}$$${module.Id}",
                $"ManagerHost={Environment.MachineName}",
                $"DataRouterHost={Environment.MachineName}",
            };
            var hostConfig = new HostConfig()
            {
                NetworkMode = $"{ContainerPrefix}{this.InstructionalModel.Id}",
            };

            await this.PullImage(moduleDockerImageRepository, moduleDockerImageTag).ConfigureAwait(true);

            await this.CreateAndStartContainer(
                $"{ContainerPrefix}{this.InstructionalModel.Id}_{module.Id}",
                $"{moduleDockerImageRepository}:{moduleDockerImageTag}",
                environmentVariables,
                hostConfig).ConfigureAwait(true);
        }

        public async Task RemoveModule(Module module)
        {
            await this.StopAndRemoveContainer($"{ContainerPrefix}{this.InstructionalModel.Id}_{module.Id}").ConfigureAwait(true);
        }
    }
}