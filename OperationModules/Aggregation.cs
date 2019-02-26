using de.fhg.ipa.data.aggregator.datarouter;
using de.fhg.ipa.data.aggregator.shared.model;
using de.fhg.ipa.data.aggregator.shared.operation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggegration
{
    class Aggregation : Operator
    {
        public Aggregation(string dataRouterHost, uint dataRouterPublishPort, AggregationConfig aggregationConfig)
        {
            this.DataRouterHost = dataRouterHost;
            this.DataRouterPublishPort = dataRouterPublishPort;
            this.ResultName = aggregationConfig.ResultName;
            this.Description = aggregationConfig.Description;
        }

        override public void Execute()
        {
            if (NeededInformation.Count == InformationValues.Count)
            {
                string expressionToCalculate = Description;
                foreach (var variable in Variables)
                {
                    expressionToCalculate = expressionToCalculate.Replace("${" + variable.Key + "}", variable.Value.ToString());
                }
                foreach (var informationValue in InformationValues)
                {
                    expressionToCalculate = expressionToCalculate.Replace("${" + informationValue.Key + "}", informationValue.Value.Value.ToString());
                }
                expressionToCalculate = expressionToCalculate.Replace(",", ".");
                Expression mxParserExpression = new Expression(expressionToCalculate);
                double result = mxParserExpression.calculate();
                //new Information(ResultName, result);
                OnNewResultCalculated?.Invoke(new Information(ResultName, result, DateTime.Now));
                Console.WriteLine($"Result: {result}");
            }
            else
            {
                Console.WriteLine("Some information are missing for calculation");
            }
        }

    }
}
