using Newtonsoft.Json;

namespace Fraunhofer.IPA.DataAggregator.Communication.Messages
{
    public abstract class Message
    {
        #region Attributes
        [JsonIgnore]
        public string Topic { get; set; }
        #endregion Attributes

        #region Constructors
        protected Message(string topic)
        {
            this.Topic = topic;
        }
        #endregion Constructors
    }
}
