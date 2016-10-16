namespace Heathmill.FixAT.Server
{
    internal class SessionContext
    {
        public SessionContext(IFixMessageHandler messageHandler,
                              SessionLoginStatus loginStatus = SessionLoginStatus.LoggedOut)
        {
            MessageHandler = messageHandler;
            LoginStatus = loginStatus;
        }

        public IFixMessageHandler MessageHandler { get; private set; }
        public SessionLoginStatus LoginStatus { get; set; }
    }
}