using Heathmill.FixAT.Domain;

namespace Heathmill.FixAT.Server.Commands
{
    internal class SendOrderFill : ICommand
    {
        private readonly SessionMediator _sessionMediator;
        private readonly OrderMatch _orderMatch;
        private readonly FixSessionID _sessionID;

        public SendOrderFill(SessionMediator sessionMediator,
                             OrderMatch orderMatch,
                             FixSessionID sessionID)
        {
            _sessionMediator = sessionMediator;
            _orderMatch = orderMatch;
            _sessionID = sessionID;
        }

        public void Execute()
        {
            _sessionMediator.OrderFilled(_sessionID, _orderMatch);
        }
    }
}