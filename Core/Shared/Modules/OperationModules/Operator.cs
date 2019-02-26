using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Fraunhofer.IPA.DataAggregator.Communication;
using Fraunhofer.IPA.DataAggregator.Communication.Messages;

namespace Fraunhofer.IPA.DataAggregator.Modules.OperationModules
{
    public class Operator
    {
        #region Attributes
        public string DataRouterHost { get; set; }
        public uint DataRouterPublishPort { get; set; }

        public string ResultName { get; set; }
        public string Description { get; set; }

        public List<string> NeededInformation { get; private set; } = new List<string>();

        private MessageSubscriber<Information> messageSubscriber;
        public Dictionary<string, Information> InformationValues { get; set; } = new Dictionary<string, Information>();
        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();
        #endregion Attributes

        #region Constructors
        protected Operator (String dataRouterHost, uint dataRouterPublishPort, string resultName, string description, Dictionary<string, string> variables)
        {
            this.DataRouterHost = dataRouterHost;
            this.DataRouterPublishPort = dataRouterPublishPort;
            this.ResultName = resultName;
            this.Description = description;
            this.Variables = variables;
        }
        #endregion Constructors

        #region Methods
        public void Start()
        {
            ParsedNeededInformationFromDescription();
            foreach (var neededInformation in NeededInformation)
            {
                messageSubscriber = new MessageSubscriber<Information>(DataRouterHost, DataRouterPublishPort, neededInformation);
                messageSubscriber.OnNewMessageReceived += OnNewInformationReceived;
            }
        }

        private void ParsedNeededInformationFromDescription()
        {
            string pattern = "\\$\\{([a-zA-Z0-9]*)\\}";
            MatchCollection matches = Regex.Matches(Description, pattern);

            List<string> references = new List<string>();
            foreach (Match match in matches)
            {
                string matchValue = match.Groups[0].Value;
                matchValue = matchValue.Substring(2, matchValue.Length - 2 - 1);
                if (!Variables.ContainsKey(matchValue) && !NeededInformation.Contains(matchValue))
                {
                    NeededInformation.Add(matchValue);
                }
            }
        }

        virtual public void Execute()
        {
        }
        #endregion Methods

        #region Event Handling - MessageSubscriber
        public void OnNewInformationReceived(Information receivedInformation)
        {
            InformationValues[receivedInformation.Key] = receivedInformation;
            Execute();
        }
        #endregion Event Handling - MessageSubscriber
    }
}
