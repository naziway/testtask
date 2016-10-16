using System;
using Heathmill.FixAT.Server;
using QuickFix;

namespace Heathmill.FixAT
{
    class Program
    {
        static void Main(string[] args)
        {
            var settingsFile = "FixAtServer.cfg";
            if (args.Length >= 1)
            {
                settingsFile = args[0];
            }

            Console.WriteLine("Starting server ...");
            try
            {
                var settings = new SessionSettings(settingsFile);
                var server = new ServerApplication(Console.WriteLine);
                var storeFactory = new FileStoreFactory(settings);
                var logFactory = new FileLogFactory(settings);
                var acceptor = new ThreadedSocketAcceptor(server,
                                                          storeFactory,
                                                          settings,
                                                          logFactory);

                acceptor.Start();
                Console.WriteLine("Server started");
                Console.WriteLine("Press Ctrl-C to quit");
                // TODO A better stop mechanism!

                // http://stackoverflow.com/questions/177856/how-do-i-trap-ctrl-c-in-a-c-sharp-console-app
                Console.CancelKeyPress += (sender, e) =>
                    {
                        Console.WriteLine("Stopping server ...");
                        acceptor.Stop();
                        server.Stop();
                        Console.WriteLine("Server stopped");
                    };
                
                while (true)
                {
                    System.Threading.Thread.Sleep(1000);
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e);
            }
        }
    }
}
