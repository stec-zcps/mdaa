using Fraunhofer.IPA.DataAggregator.Communication.Messages;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using Serilog;
using System.Collections.Concurrent;
using System.Threading;

namespace Fraunhofer.IPA.DataAggregator.Communication
{
    public class MessagePublisher
    {
        #region Attributes
        public uint Port { get; private set; }
        private PublisherSocket publisherSocket;

        private ConcurrentQueue<Message> MessagesToPublish = new ConcurrentQueue<Message>();
        private Thread publisherThread;
        #endregion Attributes

        #region Constructors
        public MessagePublisher(uint port)
        {
            this.Port = port;
            publisherThread = new Thread(CheckPublishQueue);
            publisherThread.Start();
        }
        #endregion Constructors

        #region Methods
        public void PublishMessage(Message messageToPublish, string topic)
        {
            messageToPublish.Topic = topic;
            PublishMessage(messageToPublish);
        }

        public void PublishMessage(Message messageToPublish)
        {
            MessagesToPublish.Enqueue(messageToPublish);
        }

        private void CheckPublishQueue()
        {
            using (publisherSocket = new PublisherSocket())
            {
                Log.Information($"Publisher socket binding on port '{Port}'");
                publisherSocket.Options.SendHighWatermark = 1000;
                publisherSocket.Bind($"tcp://*:{Port}");
                while (true)
                {
                    if (MessagesToPublish.Count > 0)
                    {
                        Message dequeuedMessage;
                        MessagesToPublish.TryDequeue(out dequeuedMessage);
                        string msg = JsonConvert.SerializeObject(dequeuedMessage);
                        Log.Information($"Sending message '{msg}' on topic '{dequeuedMessage.Topic}'");
                        publisherSocket.SendMoreFrame(dequeuedMessage.Topic).SendFrame(msg);
                    }
                    Thread.Sleep(100);
                }
            }
        }
        #endregion Methods
    }
}