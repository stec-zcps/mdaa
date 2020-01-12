namespace Mdaa.Manager.Controller.Docker
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::Docker.DotNet.Models;
    using Mdaa.Model;
    using Serilog;

    public class ManagerDockerContainerController : DockerContainerController
    {
        public async Task InitModel(InstructionalModel instructionalModel)
        {
            var networkName = $"{ContainerPrefix}{instructionalModel.Id}";

            await this.CreateNetworkIfNotExist(networkName).ConfigureAwait(true);

            var endpointSettings = new EndpointSettings()
            {
                Aliases = new List<string>() { Environment.MachineName },
            };

            if (this.IsDotNetRunningInContainer())
            {
                Log.Information($"Manager running in container, joining network '{networkName}'");
                await this.JoinNetwork(networkName, Environment.MachineName, endpointSettings).ConfigureAwait(true);
            }
        }
    }
}
