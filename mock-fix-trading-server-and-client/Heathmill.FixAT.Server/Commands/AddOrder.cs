using Heathmill.FixAT.Services;

namespace Heathmill.FixAT.Server.Commands
{
    internal class AddOrder : ICommand
    {
        private readonly CommandFactory _commandFactory;
        private readonly string _execID;
        private readonly IFixMessageGenerator _fixMessageGenerator;
        private readonly OrderData _orderData;
        private readonly OrderMediator _orderMediator;
        private readonly FixSessionID _sessionID;

        public AddOrder(CommandFactory commandFactory,
                        IFixMessageGenerator fixMessageGenerator,
                        OrderMediator orderMediator,
                        FixSessionID sessionID,
                        OrderData orderData,
                        string execID)
        {
            _commandFactory = commandFactory;
            _fixMessageGenerator = fixMessageGenerator;
            _orderMediator = orderMediator;
            _sessionID = sessionID;
            _orderData = orderData;
            _execID = execID;
        }

        public void Execute()
        {
            try
            {
                var order = _orderMediator.AddOrder(_sessionID,
                                                    _orderData.OrderType,
                                                    _orderData.Symbol,
                                                    _orderData.MarketSide,
                                                    _orderData.ClOrdID,
                                                    _orderData.Account,
                                                    _orderData.Quantity,
                                                    _orderData.Price);

                var successCmd = _commandFactory.CreateSendAcceptNewOrder(_sessionID, order);
                _commandFactory.OutgoingQueue.Enqueue(successCmd);

                // Kick off matching for the contract given we have a new order
                var matchOrders = _commandFactory.CreateMatchOrders(_orderData.Symbol);
                _commandFactory.IncomingQueue.Enqueue(matchOrders);
            }
            catch (FixATServerException e)
            {
                var rejectMessage = "Unable to add order: " + e.Message;
                var rejectCmd = _commandFactory.CreateSendRejectNewOrder(_fixMessageGenerator,
                                                                         _sessionID,
                                                                         _orderData,
                                                                         _execID,
                                                                         rejectMessage,
                                                                         e.RejectionCode);
                _commandFactory.OutgoingQueue.Enqueue(rejectCmd);
            }
        }
    }
}