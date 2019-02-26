using System.Collections.Generic;
using Fraunhofer.IPA.DataAggregator.Communication.Messages;
using Newtonsoft.Json;

namespace Fraunhofer.IPA.DataAggregator.Modules.IntegrationModules.Mqtt
{
    public class MqttIntegrationModuleInstructionsMessage : InstructionsMessage
    {
        #region Attributes
        public Dictionary<string, MqttInformation> MqttInfoSubscriptions = new Dictionary<string, MqttInformation>();
        public Dictionary<string, MqttInformation> MqttInfoPublications = new Dictionary<string, MqttInformation>();
        #endregion Attributes

        #region Constructors
        [JsonConstructor]
        public MqttIntegrationModuleInstructionsMessage(string moduleId) : base(moduleId)
        {

        }

        public MqttIntegrationModuleInstructionsMessage(string moduleId, Dictionary<string, MqttInformation> infoSubscriptions, Dictionary<string, MqttInformation> infoPublications) : base(moduleId)
        {
            this.MqttInfoSubscriptions = infoSubscriptions;
            this.MqttInfoPublications = infoPublications;
        }
        #endregion Constructors
    }
}
