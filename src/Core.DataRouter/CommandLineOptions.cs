// <copyright file="CommandLineOptions.cs" company="Fraunhofer Institute for Manufacturing Engineering and Automation IPA">
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
    using CommandLine;

    public class CommandLineOptions
    {
        [Option(
            HelpText = "Publishing port of the DataRouter.",
            Default = 40020u)]
        public uint PublishingPort { get; private set; }

        [Option(
            HelpText = "Host of the Manager.",
            Default = "localhost")]
        public string ManagerHost { get; private set; }

        [Option(
            HelpText = "Port where the Manager publishes messages.",
            Default = 40011u)]
        public uint ManagerPort { get; private set; }
    }
}
