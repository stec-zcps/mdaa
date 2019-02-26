using System.Collections.Generic;
using Fraunhofer.IPA.DataAggregator.Communication;
using Fraunhofer.IPA.DataAggregator.Communication.Messages;
using Serilog;

namespace Fraunhofer.IPA.DataAggregator.DataRouter
{
    class DataRouter
    {
        #region Attributes
        public uint PublishingPort { get; private set; }
        public string ManagerHost { get; private set; }
        public uint ManagerPort { get; private set; }

        public Dictionary<uint, MessageSubscriber<Information>> RegisteredModules = new Dictionary<uint, MessageSubscriber<Information>>();

        private Dictionary<string, object> Data = new Dictionary<string, object>();

        private MessagePublisher informationPublisher;
        #endregion Attributes

        #region Constructors
        public DataRouter(uint publishingPort, string managerHost, uint managerPort)
        {
            this.PublishingPort = publishingPort;
            this.ManagerHost = managerHost;
            this.ManagerPort = managerPort;
        }
        #endregion Constructors

        #region Methods
        public void run()
        {
            MessageSubscriber<RegistrationMessage> registrationSubscriber = new MessageSubscriber<RegistrationMessage>(ManagerHost, ManagerPort, "NewRegistration");
            registrationSubscriber.OnNewMessageReceived += OnNewRegistrionReceived;

            informationPublisher = new MessagePublisher(PublishingPort);
        }

        public void OnNewInformationReceived(Information newInformation)
        {
            Log.Information($"Received message: '{newInformation.Key}' : {newInformation.Value}");
            informationPublisher.PublishMessage(newInformation);
        }

        public void OnNewRegistrionReceived(RegistrationMessage newRegistrationMessage)
        {
            Log.Debug($"New registraion received: {newRegistrationMessage}");
            Log.Debug($"New registraion received: {newRegistrationMessage.Ip}");
            Log.Debug($"New registraion received: {newRegistrationMessage.Port}");

            if (RegisteredModules.ContainsKey(newRegistrationMessage.Port))
            {
                RegisteredModules[newRegistrationMessage.Port].StopListening();
            }
            MessageSubscriber<Information> newInformationSubscriber = new MessageSubscriber<Information>(newRegistrationMessage.Ip, newRegistrationMessage.Port, "NewInformation");
            newInformationSubscriber.OnNewMessageReceived += OnNewInformationReceived;
            RegisteredModules[newRegistrationMessage.Port] = newInformationSubscriber;
        }
        #endregion Methods
    }
}
