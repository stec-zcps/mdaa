namespace Fraunhofer.IPA.DataAggregator.Communication.Messages
{
    public class InstructionsMessage : Message
    {
        #region Attributes
        public string ModuleId { get; set; }
        #endregion Attributes

        #region Constructors
        public InstructionsMessage(string moduleId) : base("Instructions")
        {
            this.ModuleId = moduleId;
        }
        #endregion Constructors
    }
}
