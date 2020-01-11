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

namespace Fraunhofer.IPA.DataAggregator.DataRouter
{
    using System;
    using CommandLine;
    using Newtonsoft.Json;
    using Serilog;

    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
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
            Log.Information("DataRouter started");

            var publishingPort = uint.Parse(Environment.GetEnvironmentVariable("PublishingPort") ?? options.PublishingPort.ToString());
            var managerHost = Environment.GetEnvironmentVariable("ManagerHost") ?? options.ManagerHost;
            var managerPort = uint.Parse(Environment.GetEnvironmentVariable("ManagerPort") ?? options.ManagerPort.ToString());

            Log.Information($"publishingPort: {publishingPort}");
            Log.Information($"managerHost: {managerHost}");
            Log.Information($"managerPort: {managerPort}");

            var dataRouter = new DataRouter(publishingPort, managerHost, managerPort);
            dataRouter.Run();
        }
    }
}
