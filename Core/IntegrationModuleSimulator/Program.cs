using System;
using System.Threading;
using Fraunhofer.IPA.DataAggregator.Communication.Messages;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace IntegrationModuleSimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(1000);

            using (var requestSocket = new RequestSocket("tcp://localhost:40010"))
            {
                RegistrationMessage regMsg = new RegistrationMessage("IntegrationModuleSimulator");
                regMsg.Ip = "localhost";
                requestSocket.SendFrame(JsonConvert.SerializeObject(regMsg));

                string message = requestSocket.ReceiveFrameString();
                Console.WriteLine("requestSocket : Received '{0}'", message);
            }

            using (var pubSocket = new PublisherSocket())
            {
                Console.WriteLine("Publisher socket binding...");
                pubSocket.Options.SendHighWatermark = 1000;
                pubSocket.Bind("tcp://*:40102");

                for (var i = 0; i < 100; i++)
                {
                    Information information = new Information("Inf1", i, DateTime.Now);
                    string msg = JsonConvert.SerializeObject(information, Formatting.Indented);
                    Console.WriteLine("Sending message: {0}", msg);
                    pubSocket.SendMoreFrame("NewInformation").SendFrame(msg);
                    Thread.Sleep(500);
                    information = new Information("Inf2", i, DateTime.Now);
                    msg = JsonConvert.SerializeObject(information, Formatting.Indented);
                    Console.WriteLine("Sending message: {0}", msg);
                    pubSocket.SendMoreFrame("NewInformation").SendFrame(msg);
                    Thread.Sleep(500);
                    information = new Information("Inf3", i, DateTime.Now);
                    msg = JsonConvert.SerializeObject(information, Formatting.Indented);
                    Console.WriteLine("Sending message: {0}", msg);
                    pubSocket.SendMoreFrame("NewInformation").SendFrame(msg);
                    Thread.Sleep(500);
                }
            }
        }
    }
}
