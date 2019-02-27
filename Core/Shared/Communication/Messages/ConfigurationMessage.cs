namespace Fraunhofer.IPA.DataAggregator.Communication.Messages
{
    public class ConfigurationMessage : Message
    {
        #region Attributes
        public string ModuleId { get; set; } = "";
        public uint PublishingPort { get; set; } = 0;
        #endregion Attributes

        #region Constructors
        public ConfigurationMessage(string moduleId, uint publishingPort) : base("Configuration")
        {
            this.ModuleId = moduleId;
            this.PublishingPort = publishingPort;
        }
        #endregion Constructors
    }
}
