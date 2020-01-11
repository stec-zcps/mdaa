// <copyright file="DataRouterDockerContainerController.cs" company="Fraunhofer Institute for Manufacturing Engineering and Automation IPA">
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
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::Docker.DotNet.Models;

    public class DataRouterDockerContainerController : DockerContainerController
    {
        public const string DockerImageRepository = "mdaastec/datarouter";
        public const string DockerImageTag = "1.0";

        public DataRouterDockerContainerController(string modelId)
        {
            this.ModelId = modelId;
        }

        public string ModelId { get; private set; }

        public async Task StartDataRouter(bool removeExistingInstance)
        {
            if (removeExistingInstance)
            {
                await this.RemoveDataRouter().ConfigureAwait(true);
            }

            await this.CreateNetworkIfNotExist($"{ContainerPrefix}{this.ModelId}").ConfigureAwait(true);

            var environmentVariables = new List<string>()
            {
                $"ManagerHost=172.17.0.1",

                // $"ManagerHost={Environment.MachineName}",
            };
            var hostConfig = new HostConfig()
            {
                NetworkMode = $"{ContainerPrefix}{this.ModelId}",
            };

            await this.PullImage(DockerImageRepository, DockerImageTag).ConfigureAwait(true);

            await this.CreateAndStartContainer(
                $"{ContainerPrefix}{this.ModelId}_DataRouter",
                $"{DockerImageRepository}:{DockerImageTag}",
                environmentVariables,
                hostConfig).ConfigureAwait(true);
        }

        public async Task RemoveDataRouter()
        {
            await this.StopAndRemoveContainer($"{ContainerPrefix}{this.ModelId}_DataRouter").ConfigureAwait(true);
        }
    }
}
