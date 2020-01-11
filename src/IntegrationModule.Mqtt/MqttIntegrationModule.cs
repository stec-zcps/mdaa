// <copyright file="MqttIntegrationModule.cs" company="Fraunhofer Institute for Manufacturing Engineering and Automation IPA">
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

namespace Fraunhofer.IPA.DataAggregator.Modules.IntegrationModules.Mqtt
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Mdaa.Communication;
    using Mdaa.Communication.Messages;
    using Mdaa.Communication.Messages.Configuration;
    using Mdaa.Communication.Messages.Instructions;
    using Mdaa.Model.Modules.IntegrationModules.Mqtt;
    using Mdaa.Modules;
    using MQTTnet;
    using MQTTnet.Client;
    using Newtonsoft.Json;
    using Serilog;

    public class MqttIntegrationModule : BaseModule
    {
        private MessagePublisher messagePublisher;
        private Dictionary<string, MessageSubscriber<InformationMessage>> informationSubscribers = new Dictionary<string, MessageSubscriber<InformationMessage>>();
        private Dictionary<string, MqttInformation> MqttInfoPublications = new Dictionary<string, MqttInformation>();

        private IMqttClient MqttClient;

        public MqttIntegrationModule(string moduleId, string moduleIp, string managerHost, uint managerRequestPort, uint managerPublishPort, string dataRouterHost, uint dataRouterPublishPort)
             : base(moduleId, moduleIp, managerHost, managerRequestPort, managerPublishPort, dataRouterHost, dataRouterPublishPort)
        {
            Log.Information("MqttIntegrationModule started");
        }

        public void OnNewInformationMessageReceived(InformationMessage info)
        {
            foreach (var i in this.MqttInfoPublications)
            {
                if (i.Value.Source == info.Key)
                {
                    string payload = string.Empty;

                    switch (i.Value.MqttMode)
                    {
                        case "Plain":
                            {
                                payload = info.Value.ToString();
                                break;
                            }

                        case "JSON":
                            {
                                payload = JsonConvert.SerializeObject(info.Value);
                                break;
                            }

                        default:
                            {
                                break;
                            }
                    }

                    this.MqttClient.PublishAsync(i.Value.MqttTopic, payload);
                }
            }
        }

        protected override void NewConfigurationMessageReceived(string configurationMessageString)
        {
            var newConfigMessage = JsonConvert.DeserializeObject<MqttIntegrationModuleConfigurationMessage>(configurationMessageString);

            Log.Information("Config received");
            this.messagePublisher = new MessagePublisher(newConfigMessage.PublishingPort);

            // Create a new MQTT client.
            var factory = new MqttFactory();
            this.MqttClient = factory.CreateMqttClient();

            // Use TCP connection.
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(newConfigMessage.MqttBrokerAddress, (int)newConfigMessage.MqttBrokerPort) // Port is optional
                .Build();

            this.MqttClient.Connected += (s, e) =>
            {
                Log.Information("### CONNECTED WITH SERVER ###");
            };
            this.MqttClient.ConnectAsync(options);
        }

        protected override void NewInstructionsMessageReceived(string instructionsMessageString)
        {
            var newInstructionsMessage = JsonConvert.DeserializeObject<MqttIntegrationModuleInstructionsMessage>(instructionsMessageString);

            // TODO: Add timeout
            while (!this.MqttClient.IsConnected)
            {
            }

            foreach (var t in newInstructionsMessage.MqttInfoSubscriptions)
            {
                Log.Information("Making MQTT-Subscription to " + t.Value.MqttTopic);
                this.MqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(t.Value.MqttTopic).Build());
            }

            foreach (var t in newInstructionsMessage.MqttInfoPublications)
            {
                Log.Information("Making Databroker-Subscription to " + t.Value.Source);
                MessageSubscriber<InformationMessage> msgSub = new MessageSubscriber<InformationMessage>(this.DataRouterHost, this.DataRouterPublishPort, t.Value.Source);
                msgSub.NewMessageReceived += this.OnNewInformationMessageReceived;
                this.informationSubscribers.Add(t.Key, msgSub);
            }

            this.MqttInfoPublications = newInstructionsMessage.MqttInfoPublications;

            this.MqttClient.ApplicationMessageReceived += (s, e) =>
            {
                Log.Information("### RECEIVED APPLICATION MESSAGE ###");
                Log.Information($"+ Topic = {e.ApplicationMessage.Topic}");
                Log.Information($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                Log.Information($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                Log.Information($"+ Retain = {e.ApplicationMessage.Retain}");
                Log.Information(string.Empty);

                foreach (var t in newInstructionsMessage.MqttInfoSubscriptions)
                {
                    if (t.Value.MqttTopic == e.ApplicationMessage.Topic)
                    {
                        string valueAsString = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                        object value;
                        if (long.TryParse(valueAsString, out long longValue))
                        {
                            value = longValue;
                        }
                        else if (double.TryParse(valueAsString, out double doubleValue))
                        {
                            value = doubleValue;
                        }
                        else if (bool.TryParse(valueAsString, out bool booleanValue))
                        {
                            value = booleanValue;
                        }
                        else
                        {
                            value = valueAsString;
                        }

                        this.messagePublisher.PublishMessage(new InformationMessage(t.Key, value, DateTime.Now), "NewInformation");
                        break;
                    }
                }
            };
        }
    }
}
