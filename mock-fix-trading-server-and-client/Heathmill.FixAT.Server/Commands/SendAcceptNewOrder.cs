using Heathmill.FixAT.Domain;

namespace Heathmill.FixAT.Server.Commands
{
    internal class SendAcceptNewOrder : ICommand
    {
        private readonly SessionMediator _sessionMediator;
        private readonly IOrder _order;
        private readonly FixSessionID _sessionID;

        public SendAcceptNewOrder(SessionMediator sessionMediator,
                                  FixSessionID sessionID,
                                  IOrder order)
        {
            _sessionMediator = sessionMediator;
            _sessionID = sessionID;
            _order = order;
        }

        public void Execute()
        {
            _sessionMediator.NewOrderAccepted(_sessionID, _order);
        }
    }
}