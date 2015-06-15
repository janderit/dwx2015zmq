using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;

namespace zmq_sandbox
{
    class Speedtest
    {
        static void Main2()
        {

            // Primitiver Geschwindigkeitstest 
            // TCP/IP unidirektional push->pull
            // gemessen auf Empfängerseite

            // auch einmal im Release-Mode starten !

            using (var context = NetMQContext.Create())
            {

                var receiver_thread = new Thread(() =>
                {
                    using (var socket = context.CreatePullSocket())
                    {
                        var i = 0;
                        var start = Environment.TickCount;

                        socket.ReceiveReady += (s, args) =>
                        {
                            var msg = socket.ReceiveMessage();
                            if (i++ >= 10000)
                            {
                                i = 0;
                                Console.Out.WriteLine(1000 * 10000 / (Environment.TickCount - start) + " msgs/sec");
                                start = Environment.TickCount;
                            }
                        };
                        socket.Bind("tcp://127.0.0.1:10000");
                        using (var poller = new Poller(socket)) poller.Start();
                    }
                }) {};
                receiver_thread.Start();

                var sender_thread = new Thread(() =>
                {
                    using (var socket = context.CreatePushSocket())
                    {
                        socket.Connect("tcp://127.0.0.1:10000");

                        for (int i = 0; i < 200000; i++)
                        {
                            socket.Send("Hello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello WorldHello World");
                        }
                    }
                }) {};
                sender_thread.Start();


                Console.ReadLine();

                receiver_thread.Abort();
                sender_thread.Abort();

                receiver_thread.Join();
                sender_thread.Join();

            }

            Console.ReadLine();

        }
    }
}
