using Fraunhofer.IPA.DataAggregator.Communication.Messages;
using org.mariuszgromada.math.mxparser;
using System;

namespace Fraunhofer.IPA.DataAggregator.Modules.OperationModules.Math
{
    public class MathOperator : Operator
    {
        #region Constructors
        public MathOperator(string dataRouterHost, uint dataRouterPublishPort, MathOperatorConfig mathOperatorConfig) : 
            base(dataRouterHost, dataRouterPublishPort, mathOperatorConfig.ResultName, mathOperatorConfig.Description, mathOperatorConfig.Variables)
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
        #endregion Methods

        #region Event Handling
        public event NewResultCalculatedHandler OnNewResultCalculated = delegate { };
        public delegate void NewResultCalculatedHandler(Information newResultInformation);
        #endregion Event Handling
    }
}
