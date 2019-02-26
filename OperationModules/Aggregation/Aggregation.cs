using Fraunhofer.IPA.DataAggregator.Communication.Messages;
using Serilog;
using System;

namespace Fraunhofer.IPA.DataAggregator.Modules.OperationModules.Aggregation
{
    public class Aggregation : Operator    
    {
        #region Constructors
        public Aggregation(string dataRouterHost, uint dataRouterPublishPort, AggregationConfig aggregationConfig) :
            base(dataRouterHost, dataRouterPublishPort, aggregationConfig.ResultName, aggregationConfig.Description, aggregationConfig.Variables)
        {
        }
        #endregion Constructors

        #region Methods
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
                /*expressionToCalculate = expressionToCalculate.Replace(",", ".");
                Expression mxParserExpression = new Expression(expressionToCalculate);
                double result = mxParserExpression.calculate();
                //new Information(ResultName, result);*/

                object result = Newtonsoft.Json.JsonConvert.DeserializeObject(expressionToCalculate);

                OnNewResultCalculated?.Invoke(new Information(ResultName, result, DateTime.Now));
                Log.Information($"Result: {result}");
            }
            else
            {
                Log.Information("Some information are missing for Aggregation");
            }
        }
        #endregion Methods

        #region Event Handling
        public event NewResultCalculatedHandler OnNewResultCalculated = delegate { };
        public delegate void NewResultCalculatedHandler(Information newResultInformation);
        #endregion Event Handling
    }
}
