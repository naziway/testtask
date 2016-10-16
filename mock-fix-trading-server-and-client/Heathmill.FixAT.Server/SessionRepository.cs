using System;
using System.Collections.Generic;
using System.Linq;

namespace Heathmill.FixAT.Server
{
    internal class SessionRepository : ISessionRepository
    {
        private readonly object _lock = new object();

        private readonly Dictionary<FixSessionID, SessionContext> _sessions =
            new Dictionary<FixSessionID, SessionContext>();

        public void AddSession(FixSessionID sessionID, IFixMessageHandler messageHandler)
        {
            lock (_lock)
            {
                _sessions.Add(sessionID, new SessionContext(messageHandler));
            }
        }

        public void RemoveSession(FixSessionID sessionID)
        {
            lock (_lock)
            {
                _sessions.Remove(sessionID);
            }
        }

        public void SessionLoggedIn(FixSessionID sessionID)
        {
            lock (_lock)
            {
                _sessions[sessionID].LoginStatus = SessionLoginStatus.LoggedIn;
            }
        }

        public void SessionLoggedOut(FixSessionID sessionID)
        {
            lock (_lock)
            {
                _sessions[sessionID].LoginStatus = SessionLoginStatus.LoggedOut;
            }
        }

        public IEnumerable<FixSessionID> GetLoggedInSessions()
        {
            lock (_lock)
            {
                return _sessions.Where(s => s.Value.LoginStatus == SessionLoginStatus.LoggedIn)
                                .Select(s => s.Key).ToList();
            }
        }

        public void SendMessageToHandler(FixSessionID sessionID, Action<IFixMessageHandler> f)
        {
            lock (_lock)
            {
                var handler = _sessions[sessionID];
                f(handler.MessageHandler);
            }
        }
    }
}