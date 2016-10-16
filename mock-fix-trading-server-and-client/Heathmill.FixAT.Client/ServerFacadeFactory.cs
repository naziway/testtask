using Heathmill.FixAT.Services;

namespace Heathmill.FixAT.Client
{
    public static class ServerFacadeFactory
    {
        public static IServerFacade CreateFixServer(string configFilepath,
                                                    IFixStrategy fixStrategy,
                                                    IExecIDGenerator execIDGenerator,
                                                    IFixMessageGenerator messageGenerator,
                                                    IMessageSink messageSink)
        {
            var clientApp = ClientApplicationFactory.Create(configFilepath,
                                                            fixStrategy,
                                                            messageGenerator,
                                                            messageSink);
            
            return new FixServerFacade(clientApp, execIDGenerator, messageGenerator);
        }
    }
}
