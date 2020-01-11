// <copyright file="MathOperator.cs" company="Fraunhofer Institute for Manufacturing Engineering and Automation IPA">
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

namespace Fraunhofer.IPA.DataAggregator.Modules.OperationModules.Math
{
    using System;
    using Mdaa.Communication.Messages;
    using Mdaa.Model.Modules.OperationModules;
    using Mdaa.Model.Modules.OperationModules.Math;
    using org.mariuszgromada.math.mxparser;

    public class MathOperator : Operator
    {
        public MathOperator(string dataRouterHost, uint dataRouterPublishPort, MathOperatorConfig mathOperatorConfig)
            : base(dataRouterHost, dataRouterPublishPort, mathOperatorConfig.ResultName, mathOperatorConfig.Description, mathOperatorConfig.Variables)
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

                expressionToCalculate = expressionToCalculate.Replace(",", ".");
                Expression mxParserExpression = new Expression(expressionToCalculate);
                double result = mxParserExpression.calculate();

                // new Information(ResultName, result);
                this.OnNewResultCalculated?.Invoke(new InformationMessage(this.ResultName, result, DateTime.Now));
                Console.WriteLine($"Result: {result}");
            }
            else
            {
                Console.WriteLine("Some information are missing for calculation");
            }
        }

        public delegate void NewResultCalculatedHandler(InformationMessage newResultInformation);

        public event NewResultCalculatedHandler OnNewResultCalculated = newResultInformation => { };
    }
}
