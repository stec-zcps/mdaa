// <copyright file="InstructionalModel.cs" company="Fraunhofer Institute for Manufacturing Engineering and Automation IPA">
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

namespace Fraunhofer.IPA.DataAggregator.Manager
{
    using System.Collections.Generic;
    using global::Manager.Model.InstructionalModel;

    public class InstructionalModel
    {
        public string Id { get; set; }

        public List<Module> Modules { get; set; } = new List<Module>();

        public List<Information> Information { get; set; } = new List<Information>();

        public List<Operation> Operations { get; set; } = new List<Operation>();

        public Module GetModuleById(string moduleId)
        {
            return this.Modules.Find(m => m.Id == moduleId);
        }

        public Information GetInformationById(string informationId)
        {
            return this.Information.Find(i => i.Id == informationId);
        }

        public Operation GetOperationById(string operationId)
        {
            return this.Operations.Find(o => o.Id == operationId);
        }
    }

    // public void GetInstructionalMessageForModule(string moduleId)
    // {
    // foreach (var o in Model.Modules)
    // {
    //    switch (o.Value.Type)
    //    {
    //        case "BeckhoffAds":
    //            {
    //                string amsNetId = o.Value.Configuration["AmsNetId"].ToString();
    //                string amsIpV4 = o.Value.Configuration["AmsIpV4"].ToString();
    //                BeckhoffAdsIntegrationModuleConfigurationMessage adsConfigMsg = new BeckhoffAdsIntegrationModuleConfigurationMessage(o.Key, GetNextFreePublishingPort(), amsNetId, amsIpV4);
    //                IntegrationModuleConfigurations.Add(adsConfigMsg.ModuleId, adsConfigMsg);

    // BeckhoffAdsIntegrationModuleInstructionsMessage adsInstructMsg = new BeckhoffAdsIntegrationModuleInstructionsMessage(o.Key);
    //                IntegrationModuleInstructions.Add(o.Key, adsInstructMsg);
    //                break;
    //            }
    //        case "OpcUaClient":
    //            {
    //                string adr = o.Value.Configuration["OpcUaServerAddress"].ToString();
    //                OpcUaClientModuleConfigurationMessage opcUaConfigMsg = new OpcUaClientModuleConfigurationMessage(o.Key, GetNextFreePublishingPort(), adr);
    //                IntegrationModuleConfigurations.Add(opcUaConfigMsg.ModuleId, opcUaConfigMsg);

    // OpcUaClientModuleInstructionsMessage opcUaInstructMsg = new OpcUaClientModuleInstructionsMessage(o.Key);
    //                IntegrationModuleInstructions.Add(o.Key, opcUaInstructMsg);
    //                break;
    //            }
    //        case "OpcUaServer":
    //            {
    //                uint port = uint.Parse(o.Value.Configuration["PublishingPort"].ToString());
    //                OpcUaServerModuleConfiugrationMessage opcUaServerConfigMsg = new OpcUaServerModuleConfiugrationMessage(o.Key, GetNextFreePublishingPort(), port);
    //                IntegrationModuleConfigurations.Add(opcUaServerConfigMsg.ModuleId, opcUaServerConfigMsg);

    // OpcUaServerModuleInstructionsMessage opcUaServerModuleInstructionsMessage = new OpcUaServerModuleInstructionsMessage(o.Key);
    //                IntegrationModuleInstructions.Add(o.Key, opcUaServerModuleInstructionsMessage);
    //                break;
    //            }
    //        case "MqttClient":
    //            {
    //                string broker = o.Value.Configuration["Broker"].ToString();
    //                int port = int.Parse(o.Value.Configuration["Port"].ToString());
    //                MqttIntegrationModuleConfigurationMessage mqttConfigMsg = new MqttIntegrationModuleConfigurationMessage(o.Key, GetNextFreePublishingPort(), broker, port);
    //                IntegrationModuleConfigurations.Add(mqttConfigMsg.ModuleId, mqttConfigMsg);

    // MqttIntegrationModuleInstructionsMessage mqttInstructMsg = new MqttIntegrationModuleInstructionsMessage(o.Key);
    //                IntegrationModuleInstructions.Add(mqttInstructMsg.ModuleId, mqttInstructMsg);
    //                break;
    //            }
    //        case "MathOperator":
    //            {
    //                MathOperationModuleConfigurationMessage mathModuleConfigMessage = new MathOperationModuleConfigurationMessage(o.Key, GetNextFreePublishingPort(), new List<MathOperatorConfig> { });
    //                IntegrationModuleConfigurations.Add(mathModuleConfigMessage.ModuleId, mathModuleConfigMessage);
    //                break;
    //            }
    //        case "Aggregation":
    //            {
    //                AggregationModuleConfigurationMessage aggregationModuleConfigMessage = new AggregationModuleConfigurationMessage(o.Key, GetNextFreePublishingPort(), new List<AggregationConfig> { });
    //                IntegrationModuleConfigurations.Add(aggregationModuleConfigMessage.ModuleId, aggregationModuleConfigMessage);
    //                break;
    //            }
    //        default:
    //            {
    //                break;
    //            }
    //    }
    // }

    // foreach (var o in Model.Operations)
    // {
    //    try
    //    {
    //        string type = Model.Modules[o.Value.Operator].Type;

    // switch (type)
    //        {
    //            case "MathOperator":
    //                {
    //                    MathOperationModuleConfigurationMessage m = (MathOperationModuleConfigurationMessage)IntegrationModuleConfigurations[o.Value.Operator];
    //                    Dictionary<string, string> variables = o.Value.Variables;
    //                    MathOperatorConfig mathOperator = new MathOperatorConfig(o.Value.Result, o.Value.Description, variables);
    //                    m.MathOperatorConfigs.Add(mathOperator);
    //                    break;
    //                }
    //            case "Aggregation":
    //                {
    //                    AggregationModuleConfigurationMessage m = (AggregationModuleConfigurationMessage)IntegrationModuleConfigurations[o.Value.Operator];
    //                    Dictionary<string, string> variables = o.Value.Variables;
    //                    AggregationConfig aggregationConfig = new AggregationConfig(o.Value.Result, o.Value.Description, variables);
    //                    m.AggregationConfig.Add(aggregationConfig);
    //                    break;
    //                }
    //            default:
    //                {
    //                    break;
    //                }
    //        }
    //    }
    //    catch (System.Exception e)
    //    {

    // }
    // }

    // foreach (var i in Model.InformationsToGet)
    // {
    //    string type = Model.Modules[i.Value.Module].Type;

    // switch (type)
    //    {
    //        case "BeckhoffAds":
    //            {
    //                BeckhoffAdsIntegrationModuleInstructionsMessage adsInstructMsg = (BeckhoffAdsIntegrationModuleInstructionsMessage)IntegrationModuleInstructions[i.Value.Module];
    //                adsInstructMsg.AdsSymbolsFromTarget.Add(i.Key, new BeckhoffAdsSymbol(i.Value.Access["Symbolname"].ToString(), i.Value.Access["Datatype"].ToString(), (bool)i.Value.Access["Array"]));
    //                break;
    //            }
    //        case "OpcUaClient":
    //            {
    //                OpcUaClientModuleInstructionsMessage opcUaInstructMsg = (OpcUaClientModuleInstructionsMessage)IntegrationModuleInstructions[i.Value.Module];
    //                opcUaInstructMsg.AddInformation(i.Key, new OpcUaClientNode(i.Value.Access["NodeId"].ToString()));
    //                break;
    //            }
    //        case "MqttClient":
    //            {
    //                MqttIntegrationModuleInstructionsMessage configurationMessage = (MqttIntegrationModuleInstructionsMessage)IntegrationModuleInstructions[i.Value.Module];
    //                configurationMessage.MqttInfoSubscriptions.Add(i.Key, new MqttInformation(i.Value.Access["Topic"].ToString(), i.Value.Access["Mode"].ToString()));
    //                break;
    //            }
    //        default:
    //            {
    //                break;
    //            }
    //    }
    // }

    // foreach (var i in Model.InformationsToProvide)
    // {
    //    string type = Model.Modules[i.Value.Module].Type;

    // switch (type)
    //    {
    //        case "BeckhoffAds":
    //            {
    //                BeckhoffAdsIntegrationModuleInstructionsMessage adsInstructMsg = (BeckhoffAdsIntegrationModuleInstructionsMessage)IntegrationModuleInstructions[i.Value.Module];
    //                adsInstructMsg.AdsSymbolsToTarget.Add(i.Key, new BeckhoffAdsSymbol(i.Value.Access["Symbolname"].ToString(), i.Value.Access["Datatype"].ToString(), (bool)i.Value.Access["Array"], i.Value.Source));
    //                break;
    //            }
    //        case "OpcUaServer":
    //            {
    //                OpcUaServerModuleInstructionsMessage opcUaServerModuleInstructionsMessage = (OpcUaServerModuleInstructionsMessage)IntegrationModuleInstructions[i.Value.Module];
    //                opcUaServerModuleInstructionsMessage.AddInformation(i.Key, new OpcUaServerNode(i.Value.Access["NodeId"].ToString(), i.Value.Source));
    //                break;
    //            }
    //        case "MqttClient":
    //            {
    //                MqttIntegrationModuleInstructionsMessage configurationMessage = (MqttIntegrationModuleInstructionsMessage)IntegrationModuleInstructions[i.Value.Module];
    //                configurationMessage.MqttInfoPublications.Add(i.Key, new MqttInformation(i.Value.Access["Topic"].ToString(), i.Value.Access["Mode"].ToString(), i.Value.Source));
    //                //configurationMessage.MqttInfoPublications.Add(i.Key, new MqttIntegrationModuleInstructionsMessage.MqttInformation(i.Value.Access["Topic"].ToString(), i.Value.Access["Mode"].ToString()));
    //                break;
    //            }
    //        default:
    //            {
    //                break;
    //            }
    //    }
    // }
    // }
}