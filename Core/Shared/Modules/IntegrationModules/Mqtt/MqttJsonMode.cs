namespace Fraunhofer.IPA.DataAggregator.Modules.IntegrationModules.Mqtt
{
    public class MqttJsonMode : MqttMode
    {
        #region Attributes
        public string Property { get; private set; }
        #endregion Attributes

        #region Constructors
        public MqttJsonMode(string property)
        {
            this.Property = property;
        }
        #endregion Constructors
    }
}
