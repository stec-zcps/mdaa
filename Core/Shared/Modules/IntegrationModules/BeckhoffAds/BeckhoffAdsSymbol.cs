using Newtonsoft.Json;

namespace Fraunhofer.IPA.DataAggregator.Modules.IntegrationModules.BeckhoffAds
{
    public class BeckhoffAdsSymbol
    {
        #region Attributes
        public string Symbolname;
        public string Datatype;
        public bool Array;
        public string Source;
        #endregion Attributes

        #region Constructors
        [JsonConstructor]
        public BeckhoffAdsSymbol(string Symbolname, string Datatype, bool Array)
        {
            this.Symbolname = Symbolname;
            this.Datatype = Datatype;
            this.Array = Array;
        }

        public BeckhoffAdsSymbol(string Symbolname, string Datatype, bool Array, string Source)
        {
            this.Symbolname = Symbolname;
            this.Datatype = Datatype;
            this.Array = Array;
            this.Source = Source;
        }
        #endregion Constructors
    }
}