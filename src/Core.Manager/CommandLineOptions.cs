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

namespace Fraunhofer.IPA.DataAggregator.Manager
{
    using CommandLine;

    public class CommandLineOptions
    {
        [Option(
            HelpText = "Port where Manager publishes messages",
            Default = 40011u)]
        public uint MessagePublisherPort { get; set; }

        [Option(
            HelpText = "Port where Manger listens for registration requests.",
            Default = 40010u)]
        public uint RegistrationResponderPort { get; set; }

        [Option(
            HelpText = "Starting port for the port assignment of new registered modules.",
            Default = 40100u)]
        public uint NextFreePublishingPort { get; set; }
    }
}
