using Fraunhofer.IPA.DataAggregator.Communication;
using Fraunhofer.IPA.DataAggregator.Communication.Messages;
using Serilog;

namespace Fraunhofer.IPA.DataAggregator.Modules.OperationModules.Math
{
    class MathOperationModule : OperationModule
    {
        #region Attributes
        private MessagePublisher messagePublisher;
        #endregion Attributes

        #region Constructors
        public MathOperationModule(string moduleId, string moduleIp, string managerHost, uint managerRequestPort, uint managerPublishPort, string dataRouterHost, uint dataRouterPublishPort)
            : base(moduleId, moduleIp, managerHost, managerRequestPort, managerPublishPort, dataRouterHost, dataRouterPublishPort)
        {
            Log.Information("MathOperationModule started");

            MessageSubscriber<MathOperationModuleConfigurationMessage> configMessageSubscriber = new MessageSubscriber<MathOperationModuleConfigurationMessage>(ManagerHost, ManagerPublishPort, "Configuration");
            configMessageSubscriber.OnNewMessageReceived += OnNewConfigurationMessageReceived;

            Register();
        }
        #endregion Constructors

        #region Event Handling - MessageSubscriber
        public void OnNewConfigurationMessageReceived(MathOperationModuleConfigurationMessage newConfigMessage)
        {
            if (newConfigMessage.ModuleId == ModuleId)
            {
                Log.Information("Config received");
                messagePublisher = new MessagePublisher(newConfigMessage.PublishingPort);
                foreach (var mathOperatorConfig in newConfigMessage.MathOperatorConfigs)
                {
                    MathOperator newMathOperator = new MathOperator(DataRouterHost, DataRouterPublishPort, mathOperatorConfig);
                    newMathOperator.OnNewResultCalculated += OnNewResultCalculated;
                    newMathOperator.Start();
                }
            }
        }
        #endregion Event Handling - MessageSubscriber

        #region Event Handling - MathOperator
        public void OnNewResultCalculated(Information newResultInformation)
        {
            Log.Debug("New result calculated: " + newResultInformation);
            messagePublisher.PublishMessage(newResultInformation, "NewInformation");
        }
        #endregion Event Handling - MathOperator
    }
}
