// <copyright file="Program.cs" company="Fraunhofer Institute for Manufacturing Engineering and Automation IPA">
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

namespace Fraunhofer.IPA.DataAggregator.Modules.IntegrationModules.Mqtt
{
    using System;
    using CommandLine;
    using Mdaa.Model.Configuration;
    using Newtonsoft.Json;
    using Serilog;

    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            var cmdOptions = Parser.Default.ParseArguments<CommandLineOptions>(args);
            cmdOptions.WithParsed(
                options =>
                {
                    MainWithCommandLineOptions(options);
                });
        }

        private static void MainWithCommandLineOptions(CommandLineOptions options)
        {
            // Override command line options with environment variables
            var moduleId = Environment.GetEnvironmentVariable("ModuleId") ?? options.ModuleId;
            var moduleIp = Environment.GetEnvironmentVariable("ModuleIp") ?? options.ModuleIp;
            var managerHost = Environment.GetEnvironmentVariable("ManagerHost") ?? options.ManagerHost;
            var managerPublishingPort = uint.Parse(Environment.GetEnvironmentVariable("ManagerPublishingPort") ?? options.ManagerPublishPort.ToString());
            var managerRequestPort = uint.Parse(Environment.GetEnvironmentVariable("ManagerRequestPort") ?? options.ManagerRequestPort.ToString());
            var dataRouterHost = Environment.GetEnvironmentVariable("DataRouterHost") ?? options.DataRouterHost;
            var dataRouterPublishingPort = uint.Parse(Environment.GetEnvironmentVariable("DataRouterPublishingPort") ?? options.DataRouterPublishPort.ToString());

            if (options.ModuleIp.Equals(string.Empty))
            {
                moduleIp = Environment.MachineName;
                Log.Information($"ModuleIp is empty, using hostname '{moduleIp}' instead");
            }

            Log.Information($"moduleId: {moduleId}");
            Log.Information($"moduleIp: {moduleIp}");
            Log.Information($"managerHost: {managerHost}");
            Log.Information($"managerPublishingPort: {managerPublishingPort}");
            Log.Information($"managerRequestPort: {managerRequestPort}");
            Log.Information($"dataRouterHost: {dataRouterHost}");
            Log.Information($"dataRouterPublishingPort: {dataRouterPublishingPort}");

            var mqttIntegrationModule = new MqttIntegrationModule(
                moduleId,
                moduleIp,
                managerHost,
                managerRequestPort,
                managerPublishingPort,
                dataRouterHost,
                dataRouterPublishingPort);
            mqttIntegrationModule.Start();
        }
    }
}
