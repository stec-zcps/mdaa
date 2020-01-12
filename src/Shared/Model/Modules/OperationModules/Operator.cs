// <copyright file="Operator.cs" company="Fraunhofer Institute for Manufacturing Engineering and Automation IPA">
// Copyright 2019 Fraunhofer Institute for Manufacturing Engineering and Automation IPA
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

namespace Mdaa.Model.Modules.OperationModules
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Mdaa.Communication;
    using Mdaa.Communication.Messages;
    using Mdaa.Model.Informations;
    using Serilog;

    public class Operator
    {
        private MessageSubscriber<InformationMessage> messageSubscriber;

        protected Operator(string dataRouterHost, uint dataRouterPublishPort, string resultName, string description, Dictionary<string, object> variables)
        {
            this.DataRouterHost = dataRouterHost;
            this.DataRouterPublishPort = dataRouterPublishPort;
            this.ResultName = resultName;
            this.Description = description;
            this.Variables = variables;
        }

        public string DataRouterHost { get; set; }

        public uint DataRouterPublishPort { get; set; }

        public string ResultName { get; set; }

        public string Description { get; set; }

        public List<string> NeededInformation { get; private set; } = new List<string>();

        public Dictionary<string, InformationMessage> InformationValues { get; set; } = new Dictionary<string, InformationMessage>();

        public Dictionary<string, object> Variables { get; set; } = new Dictionary<string, object>();

        public void Start()
        {
            this.ParsedNeededInformationFromDescription();
            foreach (var neededInformation in this.NeededInformation)
            {
                Log.Information($"Subscribing for needed information '{neededInformation}' on DataRouter '{this.DataRouterHost}:{this.DataRouterPublishPort}'");
                this.messageSubscriber = new MessageSubscriber<InformationMessage>(this.DataRouterHost, this.DataRouterPublishPort, neededInformation);
                this.messageSubscriber.NewMessageReceived += this.OnNewInformationReceived;
            }
        }

        public virtual void Execute()
        {
        }

        public void OnNewInformationReceived(InformationMessage receivedInformation)
        {
            this.InformationValues[receivedInformation.Key] = receivedInformation;
            this.Execute();
        }

        private void ParsedNeededInformationFromDescription()
        {
            string pattern = "\\$\\{([a-zA-Z0-9]*)\\}";
            MatchCollection matches = Regex.Matches(this.Description, pattern);

            List<string> references = new List<string>();
            foreach (Match match in matches)
            {
                string matchValue = match.Groups[0].Value;
                matchValue = matchValue.Substring(2, matchValue.Length - 2 - 1);
                if (!this.Variables.ContainsKey(matchValue) && !this.NeededInformation.Contains(matchValue))
                {
                    this.NeededInformation.Add(matchValue);
                }
            }
        }
    }
}
