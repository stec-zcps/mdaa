namespace Fraunhofer.IPA.DataAggregator.Modules.IntegrationModules.OpcUa.Server
{
    public class OpcUaServerNode
    {
        public string NodeId { get; private set; }
        public string Source { get; private set; }

        public OpcUaServerNode(string nodeId, string source)
        {
            this.NodeId = nodeId;
            this.Source = source;
        }
    }
}
