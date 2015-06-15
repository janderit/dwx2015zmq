using System;
using System.Threading;
using NetMQ;

namespace zmq_demo_dwx
{
    /// <summary>
    /// Helper to provide connections to the supervisor host
    /// </summary>
    public class Supervisor
    {
        private readonly NetMQContext _context;
        private readonly string _endpoint;

        public Supervisor(NetMQContext context, string endpoint)
        {
            _context = context;
            _endpoint = endpoint;
        }

        /// <summary>
        /// Takes over a poller and lets it run until kill signal on the supervisor channel
        /// </summary>
        public void Supervise(Poller poller)
        {
            using (var socket = _context.CreateSubscriberSocket())
            {
                socket.ReceiveReady += (s, args) =>
                {
                    args.Socket.ReceiveMessage(); // discard
                    poller.Stop(waitForCloseToComplete: false); // elementary, poller will wait for itself otherwise
                };
                socket.Connect(_endpoint);
                socket.Subscribe(SupervisorHost.KillSignal); // subscribers need to have at least one subscription
                poller.AddSocket(socket);
                try
                {
                    poller.Start();
                }
                finally
                {
                    poller.RemoveSocket(socket); // avoid disposed socket left in poller
                }
            }
        }

        /// <summary>
        /// Helper for components without own thread.
        /// Ideally, all components use this method, so no more Thread.Start in "worker", "balancer"...
        /// </summary>
        public void Supervise_in_thread(Action on_done, params NetMQSocket[] sockets)
        {
            Thread.MemoryBarrier(); // ZMQ sockets are not threadsafe, but may be migrated via a full memory barrier
            var thread = new Thread(() =>
            {
                Thread.MemoryBarrier();
                using (var poller = new Poller())
                {
                    foreach (var socket in sockets)
                    {
                        poller.AddSocket(socket);
                    }
                    Supervise(poller);
                }

                if (on_done != null) on_done();
            });
            thread.Start();
        }        
    }
}