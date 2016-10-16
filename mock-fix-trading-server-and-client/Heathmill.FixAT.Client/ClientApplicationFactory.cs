using Heathmill.FixAT.Services;

namespace Heathmill.FixAT.Client
{
    public static class ClientApplicationFactory
    {
        public static ClientApplication Create(string configFilepath,
                                               IFixStrategy strategy,
                                               IFixMessageGenerator messageGenerator,
                                               IMessageSink messageSink)
        {
            // FIX app settings and related
            var settings = new QuickFix.SessionSettings(configFilepath);
            strategy.SessionSettings = settings;

            // FIX application setup
            var storeFactory = new QuickFix.FileStoreFactory(settings);
            var logFactory = new QuickFix.FileLogFactory(settings);
            var app = new ClientApplication(settings, messageGenerator, strategy, messageSink);

            var initiator =
                new QuickFix.Transport.SocketInitiator(app, storeFactory, settings, logFactory);
            app.Initiator = initiator;

            return app;
        }
    }
}