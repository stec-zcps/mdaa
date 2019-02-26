using CommandLine;

namespace Fraunhofer.IPA.DataAggregator.Modules
{
    public class CommandLineOptions
    {
        [Option(HelpText = "Id of this module.",
                Default = "Module")]
        public string ModuleId { get; private set; }

        [Option(HelpText = "Ip of this module.",
                Default = "localhost")]
        public string ModuleIp { get; private set; }

        [Option(HelpText = "Host of the Manager.",
                Default = "localhost")]
        public string ManagerHost { get; private set; }

        [Option(HelpText = "Port of the Manager to receive requests.",
                Default = 40010u)]
        public uint ManagerRequestPort { get; private set; }

        [Option(HelpText = "Port where the Manager publishes messages.",
                Default = 40011u)]
        public uint ManagerPublishPort { get; private set; }

        [Option(HelpText = "Host of the DataRouter.",
                Default = "localhost")]
        public string DataRouterHost { get; private set; }

        [Option(HelpText = "Port where the DataRouter publishes messages.",
                Default = 40020u)]
        public uint DataRouterPublishPort { get; private set; }
    }
}
