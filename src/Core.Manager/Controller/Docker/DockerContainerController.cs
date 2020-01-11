// <copyright file="DockerContainerController.cs" company="Fraunhofer Institute for Manufacturing Engineering and Automation IPA">
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
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using global::Docker.DotNet;
    using global::Docker.DotNet.Models;
    using Serilog;

    public class DockerContainerController
    {
        public const string ContainerPrefix = "mdaa_";

        private DockerClient dockerClient;

        protected DockerClient DockerClient
        {
            get
            {
                if (this.dockerClient == null)
                {
                    Uri dockerHostUri = null;
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        dockerHostUri = new Uri("unix:///var/run/docker.sock");
                    }
                    else
                    {
                        dockerHostUri = new Uri("npipe://./pipe/docker_engine");
                    }

                    this.dockerClient = new DockerClientConfiguration(dockerHostUri).CreateClient();
                }

                return this.dockerClient;
            }
        }

        public async Task<bool> PullImage(string imageRepository, string imageTag)
        {
            try
            {
                AuthConfig authConfigForPull = new AuthConfig();
                Progress<JSONMessage> pullProgress = new Progress<JSONMessage>();
                await this.DockerClient.Images.CreateImageAsync(
                    new ImagesCreateParameters
                    {
                        FromImage = imageRepository,
                        Tag = imageTag,
                    },
                    authConfigForPull,
                    new Progress<JSONMessage>())
                    .ConfigureAwait(true);

                return true;
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                return false;
            }
        }

        public async Task<string> CreateAndStartContainer(string name, string image, List<string> environmentVariables, HostConfig hostConfig)
        {
            // Create Docker container
            var createContainerResult = await this.DockerClient.Containers.CreateContainerAsync(
                new CreateContainerParameters()
                {
                    Name = name,
                    Image = image,
                    Env = environmentVariables,
                    HostConfig = hostConfig,
                }).ConfigureAwait(true);

            // Start Docker container
            var startContainerResult = await this.DockerClient.Containers.StartContainerAsync(
                   createContainerResult.ID,
                   new ContainerStartParameters())
                   .ConfigureAwait(true);

            Log.Information($"Docker Container for image '{image}' succesfully created with id '{createContainerResult.ID}'");
            return createContainerResult.ID;
        }

        public ContainerListResponse GetContainerByName(string containerName)
        {
            var containerListParameters = new ContainersListParameters()
            {
                All = true,
            };
            var listContainerResult = this.DockerClient.Containers.ListContainersAsync(containerListParameters).Result;
            foreach (var container in listContainerResult)
            {
                foreach (var name in container.Names)
                {
                    if (name.Contains(containerName))
                    {
                        return container;
                    }
                }
            }

            return null;
        }

        public async Task StopAndRemoveContainer(string containerName)
        {
            var dataRouterContainer = this.GetContainerByName(containerName);
            if (dataRouterContainer != null)
            {
                if (dataRouterContainer.State != "stopped")
                {
                    var containerStopParameters = new ContainerStopParameters()
                    {
                    };
                    await this.DockerClient.Containers.StopContainerAsync(dataRouterContainer.ID, containerStopParameters).ConfigureAwait(true);
                }

                var containerRemoveParameters = new ContainerRemoveParameters()
                {
                };
                await this.DockerClient.Containers.RemoveContainerAsync(dataRouterContainer.ID, containerRemoveParameters).ConfigureAwait(true);
            }
        }

        public async Task CreateNetworkIfNotExist(string networkName)
        {
            var networksCreateParameters = new NetworksCreateParameters()
            {
                Name = networkName,
                Attachable = true,
            };

            var networkListResult = this.DockerClient.Networks.ListNetworksAsync().Result;
            var foundNetwork = networkListResult.Where(n => n.Name == networkName).FirstOrDefault();
            if (foundNetwork == default)
            {
                var networkCreateResult = await this.DockerClient.Networks.CreateNetworkAsync(networksCreateParameters).ConfigureAwait(true);
            }
        }
    }
}
