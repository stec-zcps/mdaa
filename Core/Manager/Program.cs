using CommandLine;
using Newtonsoft.Json;
using Serilog;
using System.Collections.Generic;

namespace Fraunhofer.IPA.DataAggregator.Manager
{
    class Program
    {
        private static readonly string configFile = @"Config/Model.json";

        public  static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            var cmdOptions = Parser.Default.ParseArguments<CommandLineOptions>(args);
            cmdOptions.WithParsed(
                options =>
                {
                    Main(options);
                });
        }

        private static void Main(CommandLineOptions options)
        {
            Log.Information("Manager started");
            Log.Information($"Parameters: {JsonConvert.SerializeObject(options, Formatting.Indented)}");

            string inp = System.IO.File.ReadAllText(configFile);
            Dictionary<string, InstructionalModel> Model = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, InstructionalModel>>(inp);
            Manager manager = new Manager(Model["Manager2"]);
        }
    }
}
