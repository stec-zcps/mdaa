using Fraunhofer.IPA.DataAggregator.Communication.Messages;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Threading;

namespace Fraunhofer.IPA.DataAggregator.DataRouter
{
    class InformationSubscriber //obsolet?
    {
        private Thread listenerThread;

        public event NewInformationReceivedHandler OnNewInformationReceived;
        public delegate void NewInformationReceivedHandler(Information newInformation);

        private string Host;
        private uint Port;

        public readonly string Topic_NewInformation = "NewInformation";

        public InformationSubscriber(string host, uint port)
        {
            this.Host = host;
            this.Port = port;
            listenerThread = new Thread(ListenForNewInformation);
            listenerThread.Start();
        }

        private void ListenForNewInformation()
        {
            using (var subSocket = new SubscriberSocket())
            {
                subSocket.Options.ReceiveHighWatermark = 1000;
                string zeroMqAddress = "tcp://" + Host + ":" + Port;
                subSocket.Connect(zeroMqAddress);
                subSocket.Subscribe(Topic_NewInformation);
                Console.WriteLine($"Subscribing to '{Topic_NewInformation}' on '{zeroMqAddress}'");
                while (true)
                {
                    string messageTopicReceived = subSocket.ReceiveFrameString();
                    string messageReceived = subSocket.ReceiveFrameString();
                    Information receivedInformation = JsonConvert.DeserializeObject<Information>(messageReceived);
                    OnNewInformationReceived(receivedInformation);
                }
            }
        }
    }
}
