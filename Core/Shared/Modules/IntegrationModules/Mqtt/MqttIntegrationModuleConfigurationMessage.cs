using Fraunhofer.IPA.DataAggregator.Communication.Messages;

namespace Fraunhofer.IPA.DataAggregator.Modules.IntegrationModules.Mqtt
{
    public class MqttIntegrationModuleConfigurationMessage : ConfigurationMessage
    {
        #region Attributes
        public string MqttBrokerAddress { get; private set; }
        public int MqttBrokerPort { get; private set; }
        #endregion Attributes

        #region Constructors
        public MqttIntegrationModuleConfigurationMessage(string moduleId, uint publishingPort, string mqttBrokerAddress, int mqttBrokerPort) : base(moduleId, publishingPort)
        {
            this.MqttBrokerAddress = mqttBrokerAddress;
            this.MqttBrokerPort = mqttBrokerPort;
        }
        #endregion Constructors
    }
}
