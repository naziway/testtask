using QuickFix;

namespace Heathmill.FixAT.Server
{
    internal interface IFixFacade
    {
        bool DoesSessionExist(SessionID sessionID);
        bool SendToTarget(Message message, SessionID sessionID);
    }
}