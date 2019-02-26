using Fraunhofer.IPA.DataAggregator.Communication.Messages;
using System.Collections.Generic;

namespace Fraunhofer.IPA.DataAggregator.Modules.IntegrationModules.OpcUa.Client
{
    public class OpcUaClientModuleInstructionsMessage : InstructionsMessage
    {
        #region Attributes
        public Dictionary<string, OpcUaClientNode> OpcUaNodes { get; set; } = new Dictionary<string, OpcUaClientNode>();
        #endregion Attributes

        #region Constructors
        public OpcUaClientModuleInstructionsMessage(string moduleId) : base(moduleId)
        {

        }
        #endregion Constructors

        #region Methods
        public void AddInformation(string informationKey, OpcUaClientNode opcUaNode)
        {
            this.OpcUaNodes.Add(informationKey, opcUaNode);
        }
        #endregion Methods
    }
}
