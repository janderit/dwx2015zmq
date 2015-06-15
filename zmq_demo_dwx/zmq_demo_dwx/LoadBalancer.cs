using System.Threading;
using NetMQ;

namespace zmq_demo_dwx
{
    public class LoadBalancer
    {
        /// <summary>
        /// starts the load balancer which will forward requests and responses client - worker
        /// </summary>
        public static void Start(NetMQContext context, string workerEndpoint, string clientEndpoint, Supervisor supervisor)
        {
            var wait = new ManualResetEventSlim();
            var thread = new Thread(() =>
            {
                
                using (var frontend = context.CreateRouterSocket()) // frontend should reoute responses to the right client -> router
                using (var backend = context.CreateDealerSocket()) // backend can distribute round-robin -> dealer
                using (var poller = new Poller(frontend,backend))
                {

                    // ReSharper disable AccessToDisposedClosure -- socket event handlers will never fire after poller (and then) sockets are disposed
                    frontend.ReceiveReady += (s, ars) =>
                    {
                        var msg = frontend.ReceiveMessage(); 
                        backend.SendMessage(msg);
                    };
                    backend.ReceiveReady += (s, ars) =>
                    {
                        var msg = backend.ReceiveMessage();
                        frontend.SendMessage(msg);
                    };
                    // ReSharper restore AccessToDisposedClosure


                    backend.Bind(workerEndpoint);
                    frontend.Bind(clientEndpoint);

                    wait.Set(); // important for inproc transport, where "bind" must be called before "connect"
                    supervisor.Supervise(poller);
                }

            }) {Name="balancer"};
            thread.Start();
            wait.Wait();
        }
    }
}