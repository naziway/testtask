using System;
using System.Collections.Generic;

namespace Heathmill.FixAT.Server
{
    internal interface ISessionRepository
    {
        void AddSession(FixSessionID sessionID, IFixMessageHandler messageHandler);

        void RemoveSession(FixSessionID sessionID);

        void SessionLoggedIn(FixSessionID sessionID);

        void SessionLoggedOut(FixSessionID sessionID);

        IEnumerable<FixSessionID> GetLoggedInSessions();

        void SendMessageToHandler(FixSessionID sessionID, Action<IFixMessageHandler> f);
    }
}