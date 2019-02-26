using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fraunhofer.IPA.DataAggregator.Manager
{
    class CommandLineOptions
    {
        [Option(HelpText = "Port where Manager publishes messages",
        Default = 40011u)]
        public uint MessagePublisherPort { get; set; }

        [Option(HelpText = "Port where Manger listens for registration requests.",
        Default = 40010u)]
        public uint RegistrationResponderPort { get; set; }

        [Option(HelpText = "Starting port for the port assignment of new registered modules.",
        Default = 40100u)]
        public uint NextFreePublishingPort { get; set; }
    }
}
