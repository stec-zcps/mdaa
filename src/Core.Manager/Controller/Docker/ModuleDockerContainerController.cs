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
    using global::Docker.DotNet.Models;
    using Mdaa.Model;
    using Mdaa.Model.Modules;
    using Mdaa.Model.Modules.IntegrationModules.Mqtt;
    using Mdaa.Model.Modules.IntegrationModules.OpcUaClient;
    using Mdaa.Model.Modules.IntegrationModules.OpcUaServer;
    using Mdaa.Model.Modules.OperationModules.Aggregation;
    using Mdaa.Model.Modules.OperationModules.Math;

    public class ModuleDockerContainerController : DockerContainerController
    {
        public readonly Dictionary<Type, string> ModuleDockerImageRepositories = new Dictionary<Type, string>()
        {
            { typeof(MqttIntegrationModule), "mdaastec/integration-module-mqtt" },
            { typeof(OpcUaClientIntegrationModule), "mdaastec/integration-module-opcua-client" },
            { typeof(OpcUaServerIntegrationModule), "mdaastec/integration-module-opcua-server" },
            { typeof(MathOperationModule), "mdaastec/operation-module-math" },
            { typeof(AggregationOperationModule), "mdaastec/operation-module-aggregation" },
        };

        public readonly Dictionary<Type, string> ModuleDockerImageTags = new Dictionary<Type, string>()
        {
            { typeof(MqttIntegrationModule), "1.0" },
            { typeof(OpcUaClientIntegrationModule), "1.0" },
            { typeof(OpcUaServerIntegrationModule), "1.0" },
            { typeof(MathOperationModule), "1.0" },
            { typeof(AggregationOperationModule), "1.0" },
        };

        public ModuleDockerContainerController(InstructionalModel instructionalModel)
        {
            this.InstructionalModel = instructionalModel;
        }

        public InstructionalModel InstructionalModel { get; private set; }

        public async Task Init()
        {
            var networkName = $"{ContainerPrefix}{this.InstructionalModel.Id}";
            await this.CreateNetworkIfNotExist(networkName).ConfigureAwait(true);

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
            var networkName = $"{ContainerPrefix}{this.InstructionalModel.Id}";
            var hostConfig = new HostConfig()
            {
                NetworkMode = networkName,
            };
            var endpointSettings = new EndpointSettings()
            {
                Aliases = new List<string>() { module.Id },
            };
            var networkConfig = new NetworkingConfig()
            {
                EndpointsConfig = new Dictionary<string, EndpointSettings>()
                {
                    { networkName, endpointSettings },
                },
            };

            await this.PullImage(moduleDockerImageRepository, moduleDockerImageTag).ConfigureAwait(true);

            await this.CreateAndStartContainer(
                $"{ContainerPrefix}{this.InstructionalModel.Id}_{module.Id}",
                $"{moduleDockerImageRepository}:{moduleDockerImageTag}",
                environmentVariables,
                hostConfig,
                networkConfig).ConfigureAwait(true);
        }

        public async Task RemoveModule(Module module)
        {
            await this.StopAndRemoveContainer($"{ContainerPrefix}{this.InstructionalModel.Id}_{module.Id}").ConfigureAwait(true);
        }
    }
}