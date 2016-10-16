using System;
using System.Collections.Generic;
using Heathmill.FixAT.Domain;
using Heathmill.FixAT.Utilities;
using QuickFix;

namespace Heathmill.FixAT.Server
{
    /// <summary>
    /// Controls communication with FIX sessions. 
    /// Also responsible for translating from our internal FIX Session ID to the Quickfix.SessionID
    /// </summary>
    internal class SessionMediator
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IFixFacade _fixFacade;

        public SessionMediator(ISessionRepository sessionRepository, IFixFacade fixFacade)
        {
            _sessionRepository = sessionRepository;
            _fixFacade = fixFacade;
        }

        public void AddSession(SessionID fixID, IFixMessageHandler messageHandler)
        {
            var newID = new FixSessionID();
            _sessionIDMap.Add(fixID, newID);
            _sessionRepository.AddSession(newID, messageHandler);
        }

        public void RemoveSession(SessionID fixID)
        {
            var id = _sessionIDMap.GetByFirst(fixID);
            _sessionIDMap.RemoveByFirst(fixID);
            _sessionRepository.RemoveSession(id);
        }

        public void SessionLoggedIn(SessionID fixID)
        {
            var id = _sessionIDMap.GetByFirst(fixID);
            _sessionRepository.SessionLoggedIn(id);
        }

        public void SessionLoggedOut(SessionID fixID)
        {
            var id = _sessionIDMap.GetByFirst(fixID);
            _sessionRepository.SessionLoggedOut(id);
        }

        public void OrderFilled(FixSessionID ownerSessionID, OrderMatch matchDetails)
        {
            // If we ever support owner details etc then filter those out is session != sessionID
            foreach (var sessionID in GetAllLoggedInSessions())
            {
                var fixID = _sessionIDMap.GetBySecond(sessionID);

                Action<IFixMessageHandler> messageSendF =
                    handler => handler.OnOrderFilled(fixID, matchDetails);

                _sessionRepository.SendMessageToHandler(sessionID, messageSendF);
            }
        }

        public void NewOrderAccepted(FixSessionID ownerSessionID, IOrder order)
        {
            var orders = new List<IOrder> {order};
            foreach (var sessionID in GetAllLoggedInSessions())
            {
                SendOrders(sessionID, orders);
            }
        }

        public void SendMessage(Message message, SessionID sessionID)
        {
            _fixFacade.SendToTarget(message, sessionID);
        }

        public void SendMessage(Message message, FixSessionID sessionID)
        {
            SendMessage(message, _sessionIDMap.GetBySecond(sessionID));
        }

        public void SendOrders(FixSessionID sessionID, List<IOrder> orders)
        {
            var fixID = _sessionIDMap.GetBySecond(sessionID);

            Action<IFixMessageHandler> messageSendF =
                   handler => handler.SendOrdersToSession(fixID, orders);

            _sessionRepository.SendMessageToHandler(sessionID, messageSendF);
        }

        public void SendOrders(SessionID fixSessionID, List<IOrder> orders)
        {
            var internalID = _sessionIDMap.GetByFirst(fixSessionID);

            Action<IFixMessageHandler> messageSendF =
                   handler => handler.SendOrdersToSession(fixSessionID, orders);

            _sessionRepository.SendMessageToHandler(internalID, messageSendF);
        }

        public void SendToAllLoggedInSessions(Message message, SessionID ownerSessionID)
        {
            // If we need to anonymize or otherwise mutate outgoing messages to non-owning
            // sessions then this would be the place to do so.
            foreach (var session in GetAllLoggedInSessions())
            {
                SendMessage(message, session);
            }
        }

        public FixSessionID LookupInternalSessionID(SessionID fixSessionID)
        {
            return _sessionIDMap.GetByFirst(fixSessionID);
        }

        private IEnumerable<FixSessionID> GetAllLoggedInSessions()
        {
            return _sessionRepository.GetLoggedInSessions();
        }

        private readonly BidirectionalDictionary<SessionID, FixSessionID> _sessionIDMap =
            new BidirectionalDictionary<SessionID, FixSessionID>();
    }
}