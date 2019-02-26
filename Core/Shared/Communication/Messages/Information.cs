using System;

namespace Fraunhofer.IPA.DataAggregator.Communication.Messages
{
    public class Information : Message
    {
        #region Attributes
        public string Key { get; set; }
        public object Value { get; set; }
        public DateTime Timestamp { get; set; }
        #endregion Attributes

        #region Constructors
        public Information(string key, object value, DateTime timestamp) : base("None")
        {
            this.Topic = key;
            this.Key = key;
            this.Value = value;
            this.Timestamp = timestamp;
        }
        #endregion Constructors
    }
}
