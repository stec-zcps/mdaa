using Fraunhofer.IPA.DataAggregator.Communication.Messages;

namespace Fraunhofer.IPA.DataAggregator.Modules.IntegrationModules.OpcUa.Server
{
    public class OpcUaServerModuleConfiugrationMessage : ConfigurationMessage
    {
        public uint OpcUaServerPort;

        public OpcUaServerModuleConfiugrationMessage(string moduleId, uint publishingPort, uint opcUaServerPort) : base(moduleId, publishingPort)
        {
            this.OpcUaServerPort = opcUaServerPort;
        }
    }
}
