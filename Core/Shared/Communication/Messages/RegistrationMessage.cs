using System;

namespace Fraunhofer.IPA.DataAggregator.Communication.Messages
{
    public class RegistrationMessage : Message
    {
        #region Attributes
        public string ModuleId { get; set; }
        public uint Port { get; set; }
        public string Ip { get; set; }
        #endregion Attributes

        #region Constructors

        public RegistrationMessage(String moduleId) : base("NewRegistration")
        {
            this.ModuleId = moduleId;
        }


        #endregion Constructors
    }
}
