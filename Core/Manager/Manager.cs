using Fraunhofer.IPA.DataAggregator.Communication;
using Fraunhofer.IPA.DataAggregator.Communication.Messages;
using Fraunhofer.IPA.DataAggregator.Modules.IntegrationModules.BeckhoffAds;
using Fraunhofer.IPA.DataAggregator.Modules.IntegrationModules.Mqtt;
using Fraunhofer.IPA.DataAggregator.Modules.IntegrationModules.OpcUa.Client;
using Fraunhofer.IPA.DataAggregator.Modules.IntegrationModules.OpcUa.Server;
using Fraunhofer.IPA.DataAggregator.Modules.OperationModules.Aggregation;
using Fraunhofer.IPA.DataAggregator.Modules.OperationModules.Math;
using Serilog;
using System.Collections.Generic;

namespace Fraunhofer.IPA.DataAggregator.Manager
{
    class Manager
    {
        #region Attributes
        public List<string> RegisteredModules { get; private set; } = new List<string>();
        public uint NextFreePublishingPort = 40100;

        public readonly uint RegistrationResponderPort = 40010;
        private RegistrationResponder registrationResponder;

        public readonly uint MessagePublisherPort = 40011;
        private MessagePublisher MessagePublisher;

        Dictionary<string, ConfigurationMessage> IntegrationModuleConfigurations = new Dictionary<string, ConfigurationMessage>();
        Dictionary<string, InstructionsMessage> IntegrationModuleInstructions = new Dictionary<string, InstructionsMessage>();
        #endregion Attributes

        #region Constructors

        public Manager(InstructionalModel Model)
        {
            foreach (var o in Model.Modules)
            {
                switch (o.Value.Type)
                {
                    case "BeckhoffAds":
                        {
                            string amsNetId = o.Value.Configuration["AmsNetId"].ToString();
                            string amsIpV4 = o.Value.Configuration["AmsIpV4"].ToString();
                            BeckhoffAdsIntegrationModuleConfigurationMessage adsConfigMsg = new BeckhoffAdsIntegrationModuleConfigurationMessage(o.Key, GetNextFreePublishingPort(), amsNetId, amsIpV4);
                            IntegrationModuleConfigurations.Add(adsConfigMsg.ModuleId, adsConfigMsg);

                            BeckhoffAdsIntegrationModuleInstructionsMessage adsInstructMsg = new BeckhoffAdsIntegrationModuleInstructionsMessage(o.Key);
                            IntegrationModuleInstructions.Add(o.Key, adsInstructMsg);
                            break;
                        }
                    case "OpcUaClient":
                        {
                            string adr = o.Value.Configuration["OpcUaServerAddress"].ToString();
                            OpcUaClientModuleConfigurationMessage opcUaConfigMsg = new OpcUaClientModuleConfigurationMessage(o.Key, GetNextFreePublishingPort(), adr);
                            IntegrationModuleConfigurations.Add(opcUaConfigMsg.ModuleId, opcUaConfigMsg);

                            OpcUaClientModuleInstructionsMessage opcUaInstructMsg = new OpcUaClientModuleInstructionsMessage(o.Key);
                            IntegrationModuleInstructions.Add(o.Key, opcUaInstructMsg);
                            break;
                        }
                    case "OpcUaServer":
                        {
                            uint port = uint.Parse(o.Value.Configuration["PublishingPort"].ToString());
                            OpcUaServerModuleConfiugrationMessage opcUaServerConfigMsg = new OpcUaServerModuleConfiugrationMessage(o.Key, GetNextFreePublishingPort(), port);
                            IntegrationModuleConfigurations.Add(opcUaServerConfigMsg.ModuleId, opcUaServerConfigMsg);

                            OpcUaServerModuleInstructionsMessage opcUaServerModuleInstructionsMessage = new OpcUaServerModuleInstructionsMessage(o.Key);
                            IntegrationModuleInstructions.Add(o.Key, opcUaServerModuleInstructionsMessage);
                            break;
                        }
                    case "MqttClient":
                        {
                            string broker = o.Value.Configuration["Broker"].ToString();
                            int port = int.Parse(o.Value.Configuration["Port"].ToString());
                            MqttIntegrationModuleConfigurationMessage mqttConfigMsg = new MqttIntegrationModuleConfigurationMessage(o.Key, GetNextFreePublishingPort(), broker, port);
                            IntegrationModuleConfigurations.Add(mqttConfigMsg.ModuleId, mqttConfigMsg);

                            MqttIntegrationModuleInstructionsMessage mqttInstructMsg = new MqttIntegrationModuleInstructionsMessage(o.Key);
                            IntegrationModuleInstructions.Add(mqttInstructMsg.ModuleId, mqttInstructMsg);
                            break;
                        }
                    case "MathOperator":
                        {
                            MathOperationModuleConfigurationMessage mathModuleConfigMessage = new MathOperationModuleConfigurationMessage(o.Key, GetNextFreePublishingPort(), new List<MathOperatorConfig> { });
                            IntegrationModuleConfigurations.Add(mathModuleConfigMessage.ModuleId, mathModuleConfigMessage);
                            break;
                        }
                    case "Aggregation":
                        {
                            AggregationModuleConfigurationMessage aggregationModuleConfigMessage = new AggregationModuleConfigurationMessage(o.Key, GetNextFreePublishingPort(), new List<AggregationConfig> { });
                            IntegrationModuleConfigurations.Add(aggregationModuleConfigMessage.ModuleId, aggregationModuleConfigMessage);
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

            foreach (var o in Model.Operations)
            {
                try
                {
                    string type = Model.Modules[o.Value.Operator].Type;

                    switch (type)
                    {
                        case "MathOperator":
                            {
                                MathOperationModuleConfigurationMessage m = (MathOperationModuleConfigurationMessage)IntegrationModuleConfigurations[o.Value.Operator];
                                Dictionary<string, string> variables = o.Value.Variables;
                                MathOperatorConfig mathOperator = new MathOperatorConfig(o.Value.Result, o.Value.Description, variables);
                                m.MathOperatorConfigs.Add(mathOperator);
                                break;
                            }
                        case "Aggregation":
                            {
                                AggregationModuleConfigurationMessage m = (AggregationModuleConfigurationMessage)IntegrationModuleConfigurations[o.Value.Operator];
                                Dictionary<string, string> variables = o.Value.Variables;
                                AggregationConfig aggregationConfig = new AggregationConfig(o.Value.Result, o.Value.Description, variables);
                                m.AggregationConfig.Add(aggregationConfig);
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
                catch (System.Exception e)
                {

                }
            }

            foreach (var i in Model.InformationsToGet)
            {
                string type = Model.Modules[i.Value.Module].Type;

                switch (type)
                {
                    case "BeckhoffAds":
                        {
                            BeckhoffAdsIntegrationModuleInstructionsMessage adsInstructMsg = (BeckhoffAdsIntegrationModuleInstructionsMessage)IntegrationModuleInstructions[i.Value.Module];
                            adsInstructMsg.AdsSymbolsFromTarget.Add(i.Key, new BeckhoffAdsSymbol(i.Value.Access["Symbolname"].ToString(), i.Value.Access["Datatype"].ToString(), (bool)i.Value.Access["Array"]));
                            break;
                        }
                    case "OpcUaClient":
                        {
                            OpcUaClientModuleInstructionsMessage opcUaInstructMsg = (OpcUaClientModuleInstructionsMessage)IntegrationModuleInstructions[i.Value.Module];
                            opcUaInstructMsg.AddInformation(i.Key, new OpcUaClientNode(i.Value.Access["NodeId"].ToString()));
                            break;
                        }
                    case "MqttClient":
                        {
                            MqttIntegrationModuleInstructionsMessage configurationMessage = (MqttIntegrationModuleInstructionsMessage)IntegrationModuleInstructions[i.Value.Module];
                            configurationMessage.MqttInfoSubscriptions.Add(i.Key, new MqttInformation(i.Value.Access["Topic"].ToString(), i.Value.Access["Mode"].ToString()));
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

            foreach (var i in Model.InformationsToProvide)
            {
                string type = Model.Modules[i.Value.Module].Type;

                switch (type)
                {
                    case "BeckhoffAds":
                        {
                            BeckhoffAdsIntegrationModuleInstructionsMessage adsInstructMsg = (BeckhoffAdsIntegrationModuleInstructionsMessage)IntegrationModuleInstructions[i.Value.Module];
                            adsInstructMsg.AdsSymbolsToTarget.Add(i.Key, new BeckhoffAdsSymbol(i.Value.Access["Symbolname"].ToString(), i.Value.Access["Datatype"].ToString(), (bool)i.Value.Access["Array"], i.Value.Source));
                            break;
                        }
                    case "OpcUaServer":
                        {
                            OpcUaServerModuleInstructionsMessage opcUaServerModuleInstructionsMessage = (OpcUaServerModuleInstructionsMessage)IntegrationModuleInstructions[i.Value.Module];
                            opcUaServerModuleInstructionsMessage.AddInformation(i.Key, new OpcUaServerNode(i.Value.Access["NodeId"].ToString(), i.Value.Source));
                            break;
                        }
                    case "MqttClient":
                        {
                            MqttIntegrationModuleInstructionsMessage configurationMessage = (MqttIntegrationModuleInstructionsMessage)IntegrationModuleInstructions[i.Value.Module];
                            configurationMessage.MqttInfoPublications.Add(i.Key, new MqttInformation(i.Value.Access["Topic"].ToString(), i.Value.Access["Mode"].ToString(), i.Value.Source));
                            //configurationMessage.MqttInfoPublications.Add(i.Key, new MqttIntegrationModuleInstructionsMessage.MqttInformation(i.Value.Access["Topic"].ToString(), i.Value.Access["Mode"].ToString()));
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

            this.MessagePublisher = new MessagePublisher(MessagePublisherPort);
            registrationResponder = new RegistrationResponder(RegistrationResponderPort);
            registrationResponder.OnNewRegistrationMessageReceived += OnNewRegistrationMessageReceived;
            registrationResponder.Start();
        }

        public Manager()
        {
            // Sample Config
            Dictionary<string, string> variables = new Dictionary<string, string>();
            variables.Add("multiplicator", "10");
            MathOperatorConfig mathOperator = new MathOperatorConfig("CalculatedInfo", "${Inf2}*${Inf3}*${multiplicator}", variables);
            MathOperationModuleConfigurationMessage mathModuleConfigMessage = new MathOperationModuleConfigurationMessage("MathOperationModule1", GetNextFreePublishingPort(), new List<MathOperatorConfig> { mathOperator });
            IntegrationModuleConfigurations.Add(mathModuleConfigMessage.ModuleId, mathModuleConfigMessage);

            OpcUaClientModuleConfigurationMessage opcUaConfigMsg = new OpcUaClientModuleConfigurationMessage("integrationModul1", GetNextFreePublishingPort(), "opc.tcp://opcuaserver.com:48010");
            IntegrationModuleConfigurations.Add(opcUaConfigMsg.ModuleId, opcUaConfigMsg);

            OpcUaClientModuleInstructionsMessage opcUaInstructMsg = new OpcUaClientModuleInstructionsMessage("integrationModul1");
            opcUaInstructMsg.AddInformation("Inf1", new OpcUaClientNode("ns=2;s=Demo.Dynamic.Scalar.Boolean"));
            opcUaInstructMsg.AddInformation("Inf2", new OpcUaClientNode("ns=2;s=Demo.Dynamic.Scalar.Double"));
            opcUaInstructMsg.AddInformation("Inf3", new OpcUaClientNode("ns=2;s=Demo.Dynamic.Scalar.Int16"));
            opcUaInstructMsg.AddInformation("Inf4", new OpcUaClientNode("ns=2;s=Demo.Dynamic.Scalar.String"));
            IntegrationModuleInstructions.Add(opcUaInstructMsg.ModuleId, opcUaInstructMsg);

            ConfigurationMessage integrationModuleSimulatorMessage = new ConfigurationMessage("IntegrationModuleSimulator", GetNextFreePublishingPort());
            IntegrationModuleConfigurations.Add(integrationModuleSimulatorMessage.ModuleId, integrationModuleSimulatorMessage);

            MqttIntegrationModuleConfigurationMessage mqttConfigMsg = new MqttIntegrationModuleConfigurationMessage("MqttIntegrationModule1", GetNextFreePublishingPort(), "localhost", 1883);
            IntegrationModuleConfigurations.Add(mqttConfigMsg.ModuleId, mqttConfigMsg);
            //MqttIntegrationModuleInstructionsMessage mqttInstructMsg = new MqttIntegrationModuleInstructionsMessage("MqttIntegrationModule1", "MqttInfo1", "SensorData", new MqttPlainMode());
            //IntegrationModuleInstructions.Add(mqttInstructMsg.ModuleId, mqttInstructMsg);

            OpcUaServerModuleConfiugrationMessage opcUaServerConfigMsg = new OpcUaServerModuleConfiugrationMessage("OpcUaServerModule", GetNextFreePublishingPort(), 4840);
            IntegrationModuleConfigurations.Add(opcUaServerConfigMsg.ModuleId, opcUaServerConfigMsg);

            OpcUaServerModuleInstructionsMessage opcUaServerModuleInstructionsMessage = new OpcUaServerModuleInstructionsMessage("OpcUaServerModule");
            //opcUaServerModuleInstructionsMessage.AddInformation("Inf1", new OpcUaServerNode("ns=2;s=Inf1"));
            //opcUaServerModuleInstructionsMessage.AddInformation("Inf2", new OpcUaServerNode("ns=2;s=Inf2"));
            //opcUaServerModuleInstructionsMessage.AddInformation("Inf3", new OpcUaServerNode("ns=2;s=Inf3"));

            this.MessagePublisher = new MessagePublisher(MessagePublisherPort);
            registrationResponder = new RegistrationResponder(RegistrationResponderPort);
            registrationResponder.OnNewRegistrationMessageReceived += OnNewRegistrationMessageReceived;
            registrationResponder.Start();
        }
        #endregion Constructors

        #region Methods
        private uint GetNextFreePublishingPort()
        {
            uint port = NextFreePublishingPort;
            NextFreePublishingPort++;
            return port;
        }
        #endregion Methods

        #region Event Handling
        public void OnNewRegistrationMessageReceived(RegistrationMessage newRegistrationMessage)
        {
            Log.Information($"New Module with id '{newRegistrationMessage.ModuleId}' registered");
            if (IntegrationModuleConfigurations.ContainsKey(newRegistrationMessage.ModuleId))
            {
                ConfigurationMessage integrationModuleConfigurationMessage = IntegrationModuleConfigurations[newRegistrationMessage.ModuleId];
                MessagePublisher.PublishMessage(integrationModuleConfigurationMessage);
                newRegistrationMessage.Port = integrationModuleConfigurationMessage.PublishingPort;
                MessagePublisher.PublishMessage(newRegistrationMessage);
                RegisteredModules.Add(newRegistrationMessage.ModuleId);
            }
            if (IntegrationModuleInstructions.ContainsKey(newRegistrationMessage.ModuleId))
            {
                MessagePublisher.PublishMessage(IntegrationModuleInstructions[newRegistrationMessage.ModuleId]);
            }
        }
        #endregion Event Handling
    }
}
