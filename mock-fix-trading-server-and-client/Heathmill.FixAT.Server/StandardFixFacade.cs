using System;
using QuickFix;

namespace Heathmill.FixAT.Server
{
    internal class StandardFixFacade : IFixFacade
    {
        private readonly Action<string> _messageCallback;

        public StandardFixFacade(Action<string> messageCallback)
        {
            _messageCallback = messageCallback;
        }

        public bool DoesSessionExist(SessionID sessionID)
        {
            return Session.DoesSessionExist(sessionID);
        }

        public bool SendToTarget(Message message, SessionID sessionID)
        {
            try
            {
                return Session.SendToTarget(message, sessionID);
            }
            catch (SessionNotFound ex)
            {
                _messageCallback("==session not found exception!==");
                _messageCallback(ex.ToString());
            }
            catch (Exception ex)
            {
                _messageCallback(ex.ToString());
            }
            return false;
        }
    }
}