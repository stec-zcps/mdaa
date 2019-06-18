using Fraunhofer.IPA.DataAggregator.Communication.Messages;

namespace Fraunhofer.IPA.DataAggregator.Modules.IntegrationModules.BeckhoffAds
{
    public class BeckhoffAdsIntegrationModuleConfigurationMessage : ConfigurationMessage
    {
        #region Attributes
        public string AmsNetId { get; private set; }
        public string AmsIpV4 { get; private set; }
        #endregion Attributes

        #region Constructors
        public BeckhoffAdsIntegrationModuleConfigurationMessage(string moduleId, uint publishingPort, string AmsNetId, string AmsIpV4) : base(moduleId, publishingPort)
        {
            this.AmsNetId = AmsNetId;
            this.AmsIpV4 = AmsIpV4;
        }
        #endregion Constructors
    }
}