using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;

namespace zmq_sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            const string address = "tcp://127.0.0.1:10000";

            var context = NetMQContext.Create();

            var sender = context.CreateDealerSocket();
            sender.Connect(address);
            sender.Send("Hello Nuernberg");

            Thread.Sleep(1000);

            var receiver = context.CreateDealerSocket();
            receiver.Bind(address);

            Console.Out.WriteLine(receiver.ReceiveString());
            Console.ReadLine();
        }
    }
}
