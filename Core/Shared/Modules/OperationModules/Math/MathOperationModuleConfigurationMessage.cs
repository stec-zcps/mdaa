using Fraunhofer.IPA.DataAggregator.Communication.Messages;
using System.Collections.Generic;

namespace Fraunhofer.IPA.DataAggregator.Modules.OperationModules.Math
{
    public class MathOperationModuleConfigurationMessage : ConfigurationMessage
    {
        #region Attributes
        public List<MathOperatorConfig> MathOperatorConfigs { get; set; }
        #endregion Attributes

        #region Constructors
        public MathOperationModuleConfigurationMessage(string moduleId, uint publishingPort, List<MathOperatorConfig> mathOperatorConfig) : base(moduleId, publishingPort)
        {
            this.MathOperatorConfigs = mathOperatorConfig;
        }
        #endregion Constructors
    }
}
