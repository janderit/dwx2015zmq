using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;
using zmq_demo_dwx;

namespace WorkerProzess
{
    class Program
    {
        /// <summary>
        /// External worker process, to demonstrate cross-process messaging
        /// </summary>
        static void Main(string[] args)
        {
            var name = "Ext. worker " + Process.GetCurrentProcess().Id;

            using (var context = NetMQContext.Create())
            {
                var supervisor = new Supervisor(context, Konfiguration.supervisor_endpoint);

                Worker.Start(
                    context,
                    name,
                    Konfiguration.worker_endpoint, 
                    supervisor);

                Console.Out.WriteLine(name + " is active until supervisor receives kill signal");
                Console.ReadLine();
            }

            Console.Out.WriteLine(name + " done.");
            Console.ReadLine();
        }
    }
}
