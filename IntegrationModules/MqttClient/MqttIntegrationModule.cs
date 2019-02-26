using Fraunhofer.IPA.DataAggregator.Communication;
using Fraunhofer.IPA.DataAggregator.Communication.Messages;
using MQTTnet;
using MQTTnet.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fraunhofer.IPA.DataAggregator.Modules.IntegrationModules.Mqtt
{
    public class MqttIntegrationModule : Module
    {
        #region Attributes
        private MessagePublisher messagePublisher;
        private Dictionary<string, MessageSubscriber<Information>> informationSubscribers = new Dictionary<string, MessageSubscriber<Information>>();
        private Dictionary<string, MqttInformation> MqttInfoPublications = new Dictionary<string, MqttInformation>();

        private IMqttClient MqttClient;
        #endregion Attributes

        #region Constructors
        public MqttIntegrationModule(string moduleId, string moduleIp, string managerHost, uint managerRequestPort, uint managerPublishPort, string dataRouterHost, uint dataRouterPublishPort)
             : base(moduleId, moduleIp, managerHost, managerRequestPort, managerPublishPort, dataRouterHost, dataRouterPublishPort)
        {
            Log.Information("MqttIntegrationModule started");
            MessageSubscriber<MqttIntegrationModuleConfigurationMessage> configMessageSubscriber = new MessageSubscriber<MqttIntegrationModuleConfigurationMessage>(ManagerHost, ManagerPublishPort, "Configuration");
            configMessageSubscriber.OnNewMessageReceived += OnNewConfigurationMessageReceived;

            MessageSubscriber<MqttIntegrationModuleInstructionsMessage> instructionsMessageSubscriber = new MessageSubscriber<MqttIntegrationModuleInstructionsMessage>(ManagerHost, ManagerPublishPort, "Instructions");
            instructionsMessageSubscriber.OnNewMessageReceived += OnNewInstructionsMessageReceived;
            
            Register();
        }
        #endregion Constructors

        #region Event Handling - MessageSubscriber
        public void OnNewInformationMessageReceived(Information info)
        {
            foreach(var i in MqttInfoPublications)
            {
                if(i.Value.Source == info.Key)
                {
                    string payload = "";

                    switch (i.Value.MqttMode)
                    {
                        case "Plain":
                            {
                                payload = info.Value.ToString();
                                break;
                            }
                        case "JSON":
                            {
                                payload = Newtonsoft.Json.JsonConvert.SerializeObject(info.Value); //?????? was serialisieren
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }

                    MqttClient.PublishAsync(i.Value.MqttTopic, payload);
                }
            }
        }

        public void OnNewConfigurationMessageReceived(MqttIntegrationModuleConfigurationMessage newConfigMessage)
        {
            if (newConfigMessage.ModuleId == ModuleId)
            {
                Log.Information("Config received");
                messagePublisher = new MessagePublisher(newConfigMessage.PublishingPort);

                // Create a new MQTT client.
                var factory = new MqttFactory();
                MqttClient = factory.CreateMqttClient();

                // Use TCP connection.
                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer(newConfigMessage.MqttBrokerAddress, newConfigMessage.MqttBrokerPort) // Port is optional
                    .Build();

                MqttClient.Connected += async (s, e) =>
                {
                    Log.Information("### CONNECTED WITH SERVER ###");
                };
                MqttClient.ConnectAsync(options);
            }
        }

        public void OnNewInstructionsMessageReceived(MqttIntegrationModuleInstructionsMessage newInstructionsMessage)
        {
            //MqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(newInstructionsMessage.MqttTopic).Build());
            //TODO: Add timeout
            while (!MqttClient.IsConnected)
            {

            }

            foreach(var t in newInstructionsMessage.MqttInfoSubscriptions)
            {
                Log.Information("Making MQTT-Subscription to " + t.Value.MqttTopic);
                MqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(t.Value.MqttTopic).Build());
            }
            
            foreach(var t in newInstructionsMessage.MqttInfoPublications)
            {
                Log.Information("Making Databroker-Subscription to " + t.Value.Source);
                MessageSubscriber<Information> msgSub = new MessageSubscriber<Information>(DataRouterHost, DataRouterPublishPort, t.Value.Source);
                msgSub.OnNewMessageReceived += OnNewInformationMessageReceived;
                informationSubscribers.Add(t.Key, msgSub);
            }

            MqttInfoPublications = newInstructionsMessage.MqttInfoPublications;
            
            MqttClient.ApplicationMessageReceived += (s, e) =>
            {
                Log.Information("### RECEIVED APPLICATION MESSAGE ###");
                Log.Information($"+ Topic = {e.ApplicationMessage.Topic}");
                Log.Information($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                Log.Information($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                Log.Information($"+ Retain = {e.ApplicationMessage.Retain}");
                Log.Information("");

                foreach (var t in newInstructionsMessage.MqttInfoSubscriptions)
                {
                    if(t.Value.MqttTopic == e.ApplicationMessage.Topic)
                    {
                        string valueAsString = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                        Object value;
                        if (Int64.TryParse(valueAsString, out long longValue))
                        {
                            value = longValue;
                        }
                        else if (Double.TryParse(valueAsString, out double doubleValue))
                        {
                            value = doubleValue;
                        }
                        else if (Boolean.TryParse(valueAsString, out bool booleanValue))
                        {
                            value = booleanValue;
                        }
                        else
                        {
                            value = valueAsString;
                        }
                        messagePublisher.PublishMessage(new Information(t.Key, value, DateTime.Now), "NewInformation");
                        break;
                    }
                }
            };
        }
        #endregion Event Handling - MessageSubscriber
    }
}
