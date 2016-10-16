using Heathmill.FixAT.ATOrderBook.View;
using Heathmill.FixAT.Client;
using Heathmill.FixAT.Client.Model;
using Heathmill.FixAT.Client.ViewModel;
using Heathmill.FixAT.Services;

namespace Heathmill.FixAT.ATOrderBook
{
    internal class AppRunner
    {
        private IServerFacade _server;

        public void Run(
            string configFilepath,
            IFixStrategy strategy,
            IFixMessageGenerator messageGenerator,
            IMessageSink messageSink,
            IExecIDGenerator execIDGenerator,
            IClOrdIDGenerator clOrdIDGenerator)
        {
            _server = ServerFacadeFactory.CreateFixServer(configFilepath,
                                                          strategy,
                                                          execIDGenerator,
                                                          messageGenerator,
                                                          messageSink);

            var atOrderRepository = new ATOrderRepository();
            var atOrderMediator = new ATOrderMediator(atOrderRepository,
                                                      _server,
                                                      clOrdIDGenerator);

            // Setup the data contexts for the child views in the main view
            // Ideally we'd do this in each view but due to the need to pass _app and the 
            // message sink to the view models for expediency purposes we do it here
            var mainWindow = new MainWindow
            {
                ATOrderBook =
                {
                    DataContext = new ATOrderBookViewModel(_server, atOrderMediator)
                },
                OrderBook =
                {
                    DataContext = new OrderBookViewModel(_server, clOrdIDGenerator, messageSink)
                },
                ConnectionView = { DataContext = new ConnectionViewModel(_server, messageSink) }
            };

            // Set the main UI dispatcher
            SmartDispatcher.SetDispatcher(mainWindow.Dispatcher);

            // Send messages to the status bar
            messageSink.SetMessageSink(
                s =>
                {
                    var vm = (ConnectionViewModel) mainWindow.ConnectionView.DataContext;
                    SmartDispatcher.Invoke(() => vm.StatusMessage = s);
                });

            _server.Start();
            mainWindow.Show();
        }

        public void Stop()
        {
            _server.Stop();
        }
    }
}
