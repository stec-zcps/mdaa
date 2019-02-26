using Fraunhofer.IPA.DataAggregator.Communication.Messages;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using Serilog;

namespace Fraunhofer.IPA.DataAggregator.Modules
{
    public class Module
    {
        #region Attributes
        public string ModuleId { get; protected set; }
        public string ModuleIp { get; protected set; }
        public string ManagerHost { get; protected set; }
        public uint ManagerRequestPort { get; protected set; }
        public uint ManagerPublishPort { get; protected set; }
        public string DataRouterHost { get; protected set; }
        public uint DataRouterPublishPort { get; protected set; }
        #endregion Attributes

        #region Constructors
        protected Module(string moduleId, string moduleIp, string managerHost, uint managerRequestPort, uint managerPublishPort, string dataRouterHost, uint dataRouterPublishPort)
        {            
            this.ModuleId = moduleId;
            this.ModuleIp = moduleIp;
            this.ManagerHost = managerHost;
            this.ManagerRequestPort = managerRequestPort;
            this.ManagerPublishPort = managerPublishPort;
            this.DataRouterHost = dataRouterHost;
            this.DataRouterPublishPort = dataRouterPublishPort;
        }
        #endregion Constructors

        #region Methods
        protected void Register()
        {
            using (var requestSocket = new RequestSocket($"tcp://{ManagerHost}:{ManagerRequestPort}"))
            {
                RegistrationMessage regMsg = new RegistrationMessage(ModuleId);
                regMsg.Ip = ModuleIp;
                requestSocket.SendFrame(JsonConvert.SerializeObject(regMsg));
                string message = requestSocket.ReceiveFrameString();
                Log.Information("requestSocket : Received '{0}'", message);
            }
        }
        #endregion Methods
    }
}
