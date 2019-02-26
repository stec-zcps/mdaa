using Fraunhofer.IPA.DataAggregator.Communication.Messages;
using System.Collections.Generic;

namespace Fraunhofer.IPA.DataAggregator.Modules.IntegrationModules.OpcUa.Server
{
    public class OpcUaServerModuleInstructionsMessage : InstructionsMessage
    {
        public Dictionary<string, OpcUaServerNode> OpcUaNodes { get; set; } = new Dictionary<string, OpcUaServerNode>();

        public OpcUaServerModuleInstructionsMessage(string moduleId) : base(moduleId)
        {

        }

        public void AddInformation(string informationKey, OpcUaServerNode opcUaNode)
        {
            this.OpcUaNodes.Add(informationKey, opcUaNode);
        }
    }
}
