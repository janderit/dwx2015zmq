using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;

namespace zmq_demo_dwx
{
    public class Client
    {
        /// <summary>
        /// Starts an async client request
        /// </summary>
        /// <param name="delay">simulated workload for the worker -> sleep</param>
        public static Task Request(NetMQContext context, int clientId, string workerEndpoint, int delay)
        {
            return Task.Factory.StartNew(() =>
            {
                using (var socket = context.CreateRequestSocket())
                {
                    socket.Connect(workerEndpoint);

                    var start = Environment.TickCount;
                    Console.Out.WriteLine(clientId.ToString());

                    socket.SendMore("Hey from client " + clientId, Encoding.UTF8); // message frame 1...
                    socket.Send(BitConverter.GetBytes(delay)); // final frame 2

                    var response = socket.ReceiveString(Encoding.UTF8);
                    var latency = Environment.TickCount - start - delay;
                    Console.Out.WriteLine(clientId + " (Latenz " + latency + " ms) " + response);
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}