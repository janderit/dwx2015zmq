using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;

namespace zmq_demo_dwx
{
    public static class Konfiguration
    {
        public static string client_endpoint = "inproc://balancer";
        public static string worker_endpoint = "tcp://127.0.0.1:10010";
        public static string supervisor_endpoint = "tcp://127.0.0.1:10099";
    }

    class Program
    {
        static void Main(string[] args)
        {

            var worker_poolsize = 4; // or set to 0 and use the external worker console applications (or multiple)

            using (var context = NetMQContext.Create())
            {
                var supervisor_host = new SupervisorHost(context, Konfiguration.supervisor_endpoint);
                var supervisor = new Supervisor(context, Konfiguration.supervisor_endpoint);

                foreach (var id in Enumerable.Range(0, worker_poolsize))
                {
                    Worker.Start(
                        context,
                        "Worker "+id,
                        Konfiguration.worker_endpoint,
                        supervisor);
                }

                LoadBalancer.Start(context, Konfiguration.worker_endpoint, Konfiguration.client_endpoint, supervisor);
                Thread.Sleep(1000);

                Console.Out.WriteLine("Sending 15 async requests...");

                var random = new Random();

                var client_tasks =
                    Enumerable.Range(0, 15)
                        .Select(
                            client_id =>
                                Client.Request(context, client_id, Konfiguration.client_endpoint, delay: random.Next(2000)))
                                    .ToArray();

                Task.WaitAll(client_tasks);
                
                Console.Out.WriteLine("Press <enter>...");
                Console.ReadLine();

                Console.Out.WriteLine("Shutdown via supervisor...");
                supervisor_host.KillAll();

                Console.Out.WriteLine("Press <enter>...");
                Console.ReadLine();
                Console.Out.WriteLine("terminating zmq context...");
            }

            Console.Out.WriteLine("all terminated.");

            Console.Out.WriteLine("Press <enter>...");
            Console.ReadLine();
        }
    }
}
