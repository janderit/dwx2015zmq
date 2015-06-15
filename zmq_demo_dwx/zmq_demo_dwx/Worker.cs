using System;
using System.Text;
using System.Threading;
using NetMQ;
using Poller = NetMQ.Poller;

namespace zmq_demo_dwx
{
    public class Worker
    {
        /// <summary>
        /// Starts a new worker, waiting for requests
        /// </summary>
        public static void Start(NetMQContext context, string name, string endpoint, Supervisor supervisor)
        {
            var thread = new Thread(() =>
            {

                using (var socket = context.CreateResponseSocket())
                using (var poller = new Poller(socket))
                {

                    socket.ReceiveReady += (s, args) =>
                    {
                        var msg = args.Socket.ReceiveMessage();

                        var request = msg[0].ConvertToString(Encoding.UTF8);  // message frame 1
                        var delay = BitConverter.ToInt32(msg[1].Buffer, 0); // message frame 2

                        var response = string.Format("{0}: [{1}]", name, request);

                        Thread.Sleep(TimeSpan.FromMilliseconds(delay));

                        args.Socket.Send(response, Encoding.UTF8);
                    };
                    socket.Connect(endpoint);

                    Console.Out.WriteLine(name + " ready");
                    supervisor.Supervise(poller);
                    Console.Out.WriteLine(name + " stopped");
                }

            }) {Name = name};
            thread.Start();
        }
    }
}