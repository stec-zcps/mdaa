using CommandLine;

namespace Fraunhofer.IPA.DataAggregator.DataRouter
{
    class CommandLineOptions
    {
        [Option(HelpText = "Publishing port of the DataRouter.",
                Default = 40020u)]
        public uint PublishingPort { get; private set; }

        [Option(HelpText = "Host of the Manager.",
                Default = "core")]
        public string ManagerHost { get; private set; }

        [Option(HelpText = "Port where the Manager publishes messages.",
                Default = 40011u)]
        public uint ManagerPort { get; private set; }
    }
}
