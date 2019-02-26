namespace Fraunhofer.IPA.DataAggregator.Modules.OperationModules
{
    public class OperationModule : Module
    {
        #region Constructors
        protected OperationModule(string moduleId, string moduleIp, string managerHost, uint managerRequestPort, uint managerPublishPort, string dataRouterHost, uint dataRouterPublishPort) 
            : base(moduleId, moduleIp, managerHost, managerRequestPort, managerPublishPort, dataRouterHost, dataRouterPublishPort)
        {

        }
        #endregion Constructors
    }
}
