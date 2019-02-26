﻿using Fraunhofer.IPA.DataAggregator.Communication.Messages;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using System.Threading;

namespace Fraunhofer.IPA.DataAggregator.Communication
{
    public class MessageSubscriber<T> where T : Message
    {
        #region Attributes
        private Thread listenerThread;

        public event NewMessageReceivedHandler OnNewMessageReceived;
        public delegate void NewMessageReceivedHandler(T newMessage);

        private string Host;
        private uint Port;
        private string Topic;

        private bool run = true;
        #endregion Attributes

        #region Constructors
        public MessageSubscriber(string host, uint port, string topic)
        {
            this.Host = host;
            this.Port = port;
            this.Topic = topic;
            listenerThread = new Thread(ListenForNewMessage);
            listenerThread.Start();
        }
        #endregion Constructors

        #region Methods
        public void StopListening()
        {
            run = false;
        }

        private void ListenForNewMessage()
        {
            using (var subSocket = new SubscriberSocket())
            {
                subSocket.Options.ReceiveHighWatermark = 1000;
                string zeroMqAddress = "tcp://" + Host + ":" + Port;
                subSocket.Connect(zeroMqAddress);
                subSocket.Subscribe(Topic);
                Log.Information($"Subscribing to '{Topic}' on '{zeroMqAddress}'");
                while (run)
                {
                    string messageTopicReceived = subSocket.ReceiveFrameString();
                    string messageReceived = subSocket.ReceiveFrameString();

                    if (messageTopicReceived.Equals(Topic))
                    { 
                        Log.Debug("NewMessage");
                        //try
                        //{
                        var format = "yyyy-MM-ddTH:mm:ss.fffZ"; // your datetime format
                        var dateTimeConverter = new IsoDateTimeConverter { DateTimeFormat = format };
                        T receivedMessageDeserialized = JsonConvert.DeserializeObject<T>(messageReceived, dateTimeConverter);
                        OnNewMessageReceived(receivedMessageDeserialized);
                        //}
                        //catch(Exception e)
                        //{
                        //    Log.Error($"Unable to deseriliaze: {e}");
                        //}
                    }
                }
                subSocket.Unsubscribe(Topic);
            }
        }
        #endregion Methods
    }
}
