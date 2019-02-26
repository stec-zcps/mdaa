using Fraunhofer.IPA.DataAggregator.Communication.Messages;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using Serilog;
using System.Threading;

namespace Fraunhofer.IPA.DataAggregator.Manager
{
    class RegistrationResponder
    {
        #region Attributes
        private bool run = false;
        private Thread runThread;
        private uint ResponsePort;
        #endregion Attributes

        #region Event Handling
        public event NewRegistrationMessageReceivedHandler OnNewRegistrationMessageReceived;
        public delegate void NewRegistrationMessageReceivedHandler(RegistrationMessage newRegistrationMessage);
        #endregion Event Handling

        #region Constructors
        public RegistrationResponder(uint port)
        {
            this.ResponsePort = port;            
        }
        #endregion Constructors

        #region Methods
        public void Start()
        {
            Log.Information($"Started listening on port '{ResponsePort}'");
            runThread = new Thread(ListenForNewRequests);
            run = true;
            runThread.Start();
        }

        public void Stop()
        {
            Log.Information("Stopped");
            run = false;
        }

        private void ListenForNewRequests()
        {
            using (ResponseSocket responseSocket = new ResponseSocket())
            {
                responseSocket.Bind($"tcp://*:{ResponsePort}");
                while (run)
                {
                    string messageReceived = responseSocket.ReceiveFrameString();
                    Log.Debug($"Received message: {messageReceived}");
                    RegistrationMessage receivedRegistrationMessage = JsonConvert.DeserializeObject<RegistrationMessage>(messageReceived);
                    responseSocket.SendFrame("OK");
                    OnNewRegistrationMessageReceived(receivedRegistrationMessage);
                }
            }
        }
        #endregion Methods
    }
}