using Mdaa.Communication;
using Mdaa.Communication.Messages;
using Serilog;
using System;
using System.Threading;

namespace TestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Log.Information("Started");

            var messagePublisher = new MessagePublisher(5555);

            var messageSubscriber = new MessageSubscriber<InformationMessage>("localhost", 5555, "testmessage");
            messageSubscriber.NewMessageReceived += NewMessageRecievd;
            //messageSubscriber.Connect();

            var random = new Random();
            var message = new InformationMessage("val", random.Next(), DateTime.Now);

            while (true)
            {
                message = new InformationMessage("val", random.Next(), DateTime.Now);
                messagePublisher.PublishMessage(message, "testmessage");
                Thread.Sleep(1000);
            }
        }

        private static void NewMessageRecievd(InformationMessage newMessage)
        {
            Log.Information($"{newMessage.Topic} : {newMessage.Value}");
        }
    }
}
