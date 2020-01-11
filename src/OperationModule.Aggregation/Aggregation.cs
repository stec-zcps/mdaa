// <copyright file="Aggregation.cs" company="Fraunhofer Institute for Manufacturing Engineering and Automation IPA">
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

namespace Fraunhofer.IPA.DataAggregator.Modules.OperationModules.Aggregation
{
    using System;
    using Mdaa.Communication.Messages;
    using Mdaa.Model.Modules.OperationModules;
    using Mdaa.Model.Modules.OperationModules.Aggregation;
    using Serilog;

    public class Aggregation : Operator
    {
        public Aggregation(string dataRouterHost, uint dataRouterPublishPort, AggregationConfig aggregationConfig)
            : base(dataRouterHost, dataRouterPublishPort, aggregationConfig.ResultName, aggregationConfig.Description, aggregationConfig.Variables)
        {
        }

        public override void Execute()
        {
            if (this.NeededInformation.Count == this.InformationValues.Count)
            {
                string expressionToCalculate = this.Description;
                foreach (var variable in this.Variables)
                {
                    expressionToCalculate = expressionToCalculate.Replace("${" + variable.Key + "}", variable.Value.ToString());
                }

                foreach (var informationValue in this.InformationValues)
                {
                    expressionToCalculate = expressionToCalculate.Replace("${" + informationValue.Key + "}", informationValue.Value.Value.ToString());
                }

                object result = Newtonsoft.Json.JsonConvert.DeserializeObject(expressionToCalculate);

                this.OnNewResultCalculated?.Invoke(new InformationMessage(this.ResultName, result, DateTime.Now));
                Log.Information($"Result: {result}");
            }
            else
            {
                Log.Information("Some information are missing for Aggregation");
            }
        }

        public delegate void NewResultCalculatedHandler(InformationMessage newResultInformation);

        public event NewResultCalculatedHandler OnNewResultCalculated = newResultInformation => { };
    }
}
