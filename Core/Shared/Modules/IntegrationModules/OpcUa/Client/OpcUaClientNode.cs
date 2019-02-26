namespace Fraunhofer.IPA.DataAggregator.Modules.IntegrationModules.OpcUa.Client
{
    public class OpcUaClientNode
    {
        #region Attributes
        public string NodeId { get; private set; }
        public int SamplingInterval { get; set; } = 1000;
        #endregion Attributes

        #region Constructors
        public OpcUaClientNode(string nodeId)
        {
            this.NodeId = nodeId;
        }
        #endregion Constructors
    }
}
