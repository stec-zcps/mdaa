using Fraunhofer.IPA.DataAggregator.Communication.Messages;

namespace Fraunhofer.IPA.DataAggregator.Modules.IntegrationModules.OpcUa.Client
{
    public class OpcUaClientModuleConfigurationMessage : ConfigurationMessage
    {
        #region Attributes
        public string OpcUaServerAddress { get; set; }
        #endregion Attributes

        #region Constructors
        public OpcUaClientModuleConfigurationMessage(string moduleId, uint publishingPort, string opcUaServerAddress) : base(moduleId, publishingPort)
        {
            this.OpcUaServerAddress = opcUaServerAddress;
        }
        #endregion Constructors
    }
}
