using System;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace zmq_demo_dwx
{
    public class SupervisorHost
    {
        private readonly PublisherSocket _socket;
        public static string KillSignal = "KILL";

        public SupervisorHost(NetMQContext context, string endpoint)
        {
            _socket = context.CreatePublisherSocket();
            _socket.Bind(endpoint);
            new Supervisor(context, endpoint).Supervise_in_thread(Stop);
        }

        public void KillAll()
        {
            Publish(KillSignal);
        }

        public void Publish(string message)
        {
            _socket.Send(message);
        }

        private void Stop()
        {
            Task.Run(() =>
            {
                _socket.Dispose();
                Console.Out.WriteLine("Supervisor stopped");
            });
        }
    }
}