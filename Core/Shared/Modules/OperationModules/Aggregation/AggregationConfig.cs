using System.Collections.Generic;

namespace Fraunhofer.IPA.DataAggregator.Modules.OperationModules.Aggregation
{
    public class AggregationConfig
    {
        #region Attributes
        public string ResultName;
        public string Description;
        public Dictionary<string, string> Variables;
        #endregion Attributes

        #region Constructors
        public AggregationConfig(string resultName, string description, Dictionary<string, string> variables)
        {
            this.ResultName = resultName;
            this.Description = description;
            this.Variables = variables;
        }
        #endregion Constructors
    }
}
