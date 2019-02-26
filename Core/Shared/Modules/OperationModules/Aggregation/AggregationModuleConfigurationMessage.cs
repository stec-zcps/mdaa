using Fraunhofer.IPA.DataAggregator.Communication.Messages;
using System.Collections.Generic;

namespace Fraunhofer.IPA.DataAggregator.Modules.OperationModules.Aggregation
{
    public class AggregationModuleConfigurationMessage : ConfigurationMessage
    {
        #region Attributes
        public List<AggregationConfig> AggregationConfig { get; set; }
        #endregion Attributes

        #region Constructors
        public AggregationModuleConfigurationMessage(string moduleId, uint publishingPort, List<AggregationConfig> aggregationConfig) : base(moduleId, publishingPort)
        {
            this.AggregationConfig = aggregationConfig;
        }
        #endregion Constructors
    }
}
