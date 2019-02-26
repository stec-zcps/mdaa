using System.Collections.Generic;

namespace Fraunhofer.IPA.DataAggregator.Modules.OperationModules.Math
{
    public class MathOperatorConfig
    {
        #region Attributes
        public string ResultName;
        public string Description;
        public Dictionary<string, string> Variables;
        #endregion Attributes

        #region Constructors
        public MathOperatorConfig(string resultName, string description, Dictionary<string, string> variables)
        {
            this.ResultName = resultName;
            this.Description = description;
            this.Variables = variables;
        }
        #endregion Constructors
    }
}