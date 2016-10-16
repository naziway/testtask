using Heathmill.FixAT.Domain;
using Heathmill.FixAT.Services;

namespace Heathmill.FixAT.Server.Commands
{
    internal class SendAcceptOrderCancel : ICommand
    {
        private readonly string _execID;
        private readonly IFixMessageGenerator _messageGenerator;
        private readonly SessionMediator _sessionMediator;
        private readonly IOrder _order;
        private readonly FixSessionID _sessionID;

        public SendAcceptOrderCancel(IFixMessageGenerator messageGenerator,
                                     SessionMediator sessionMediator,
                                     IOrder cancelledOrder,
                                     string execID,
                                     FixSessionID sessionID)
        {
            _messageGenerator = messageGenerator;
            _sessionMediator = sessionMediator;
            _order = cancelledOrder;
            _execID = execID;
            _sessionID = sessionID;
        }

        public void Execute()
        {
            var accept = _messageGenerator.CreateOrderCancelAccept(_order, _execID);
            _sessionMediator.SendMessage(accept, _sessionID);
        }
    }
}