using Heathmill.FixAT.Services;

namespace Heathmill.FixAT.Server.Commands
{
    internal class SendRejectNewOrder : ICommand
    {
        private readonly string _execID;
        private readonly IFixMessageGenerator _messageGenerator;
        private readonly SessionMediator _sessionMediator;
        private readonly OrderData _order;
        private readonly int? _rejectionCode;
        private readonly string _rejectionMessage;
        private readonly FixSessionID _sessionID;

        public SendRejectNewOrder(IFixMessageGenerator messageGenerator,
                                  SessionMediator sessionMediator,
                                  OrderData orderData,
                                  string execID,
                                  string rejectionMessage,
                                  int? rejectionCode,
                                  FixSessionID sessionID)
        {
            _messageGenerator = messageGenerator;
            _sessionMediator = sessionMediator;
            _order = orderData;
            _execID = execID;
            _rejectionMessage = rejectionMessage;
            _rejectionCode = rejectionCode;
            _sessionID = sessionID;
        }

        public void Execute()
        {
            var msg = _messageGenerator.CreateRejectNewOrderExecutionReport(_order.Symbol,
                                                                            _order.MarketSide,
                                                                            _order.ClOrdID,
                                                                            _order.Quantity,
                                                                            _order.Account,
                                                                            _execID,
                                                                            _rejectionMessage,
                                                                            _rejectionCode);
            _sessionMediator.SendMessage(msg, _sessionID);
        }
    }
}