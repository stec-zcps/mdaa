using Newtonsoft.Json;

namespace Fraunhofer.IPA.DataAggregator.Modules.IntegrationModules.Mqtt
{
    public class MqttInformation
    {
        #region Attributes
        public string MqttTopic;
        public string MqttMode;
        public string Source;
        #endregion Attributes

        #region Constructors
        [JsonConstructor]
        public MqttInformation(string topic, string mode)
        {
            this.MqttTopic = topic;
            this.MqttMode = mode;
        }

        public MqttInformation(string topic, string mode, string source)
        {
            this.MqttTopic = topic;
            this.MqttMode = mode;
            this.Source = source;
        }
        #endregion Constructors
    }
}
