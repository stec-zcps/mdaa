using CommandLine;
using Newtonsoft.Json;
using Serilog;

namespace Fraunhofer.IPA.DataAggregator.Modules.OperationModules.Aggregation
{
    class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            var cmdOptions = Parser.Default.ParseArguments<CommandLineOptions>(args);
            cmdOptions.WithParsed(
                options => {
                    Main(options);
                });
        }

        private static void Main(CommandLineOptions options)
        {
            Log.Information($"Parameters: {JsonConvert.SerializeObject(options, Formatting.Indented)}");

            AggregationModule aggregationModule = new AggregationModule(
                options.ModuleId,
                options.ModuleIp,
                options.ManagerHost,
                options.ManagerRequestPort,
                options.ManagerPublishPort,
                options.DataRouterHost,
                options.DataRouterPublishPort);
        }
    }
}
