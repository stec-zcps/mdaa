using System.Collections.Generic;
using Fraunhofer.IPA.DataAggregator.Communication.Messages;
using Newtonsoft.Json;

namespace Fraunhofer.IPA.DataAggregator.Modules.IntegrationModules.BeckhoffAds
{
    public class BeckhoffAdsIntegrationModuleInstructionsMessage : InstructionsMessage
    {
        #region Attributes
        public Dictionary<string, BeckhoffAdsSymbol> AdsSymbolsFromTarget = new Dictionary<string, BeckhoffAdsSymbol>();
        public Dictionary<string, BeckhoffAdsSymbol> AdsSymbolsToTarget = new Dictionary<string, BeckhoffAdsSymbol>();
        #endregion Attributes

        #region Constructors
        [JsonConstructor]
        public BeckhoffAdsIntegrationModuleInstructionsMessage(string moduleId) : base(moduleId)
        {

        }

        public BeckhoffAdsIntegrationModuleInstructionsMessage(string moduleId, Dictionary<string, BeckhoffAdsSymbol> AdsSymbolsFromTarget, Dictionary<string, BeckhoffAdsSymbol> AdsSymbolsToTarget) : base(moduleId)
        {
            this.AdsSymbolsFromTarget = AdsSymbolsFromTarget;
            this.AdsSymbolsToTarget = AdsSymbolsToTarget;
        }
        #endregion Constructors
    }
}