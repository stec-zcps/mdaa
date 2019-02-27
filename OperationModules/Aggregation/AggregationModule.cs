using Fraunhofer.IPA.DataAggregator.Communication;
using Fraunhofer.IPA.DataAggregator.Communication.Messages;
using Newtonsoft.Json;
using Serilog;

namespace Fraunhofer.IPA.DataAggregator.Modules.OperationModules.Aggregation
{
    public class AggregationModule : OperationModule
    {
        #region Attributes
        private MessagePublisher messagePublisher;
        #endregion Attributes

        #region Constructors
        public AggregationModule(string moduleId, string moduleIp, string managerHost, uint managerRequestPort, uint managerPublishPort, string dataRouterHost, uint dataRouterPublishPort)
            : base(moduleId, moduleIp, managerHost, managerRequestPort, managerPublishPort, dataRouterHost, dataRouterPublishPort)
        {
            Log.Information("AggregationModule started");

            MessageSubscriber<AggregationModuleConfigurationMessage> configMessageSubscriber = new MessageSubscriber<AggregationModuleConfigurationMessage>(ManagerHost, ManagerPublishPort, "Configuration");
            configMessageSubscriber.OnNewMessageReceived += OnNewConfigurationMessageReceived;

            Register();
        }
        #endregion Constructors

        #region Event Handling - MessageSubscriber
        public void OnNewConfigurationMessageReceived(AggregationModuleConfigurationMessage newConfigMessage)
        {
            Log.Information($"Config received: {JsonConvert.SerializeObject(newConfigMessage, Formatting.Indented)}");

            if (newConfigMessage.ModuleId == ModuleId)
            {
                if (newConfigMessage.PublishingPort == 0)
                {
                    Log.Warning($"Invalid config received: PublishingPort is '{newConfigMessage.PublishingPort}'");
                }
                else
                {
                    messagePublisher = new MessagePublisher(newConfigMessage.PublishingPort);
                    foreach (var aggregationConfig in newConfigMessage.AggregationConfig)
                    {
                        Aggregation newAggregation = new Aggregation(DataRouterHost, DataRouterPublishPort, aggregationConfig);
                        newAggregation.OnNewResultCalculated += OnNewResultCalculated;
                        newAggregation.Start();
                    }
                }
            }
        }
        #endregion Event Handling - MessageSubscriber

        #region Event Handling - Aggregation
        public void OnNewResultCalculated(Information newResultInformation)
        {
            Log.Debug("New result aggregated: " + newResultInformation);
            messagePublisher.PublishMessage(newResultInformation, "NewInformation");
        }
        #endregion Event Handling - Aggregation
    }
}
