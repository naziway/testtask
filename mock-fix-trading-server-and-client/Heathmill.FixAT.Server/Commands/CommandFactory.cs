using Heathmill.FixAT.Domain;
using Heathmill.FixAT.Services;

namespace Heathmill.FixAT.Server.Commands
{
    internal class CommandFactory
    {
        private readonly OrderMediator _orderMediator;
        private readonly SessionMediator _sessionMediator;

        public CommandFactory(ICommandQueue incomingQueue,
                              ICommandQueue outgoingQuue,
                              OrderMediator orderMediator,
                              SessionMediator sessionMediator)
        {
            IncomingQueue = incomingQueue;
            OutgoingQueue = outgoingQuue;
            _orderMediator = orderMediator;
            _sessionMediator = sessionMediator;
        }

        public ICommandQueue IncomingQueue { get; private set; }
        public ICommandQueue OutgoingQueue { get; private set; }

        public ICommand CreateAddOrder(IFixMessageGenerator fixMessageGenerator,
                                       FixSessionID sessionID,
                                       OrderData orderData,
                                       string execID)
        {
            return new AddOrder(this,
                                fixMessageGenerator,
                                _orderMediator,
                                sessionID,
                                orderData,
                                execID);
        }

        public ICommand CreateCancelOrder(IFixMessageGenerator fixMessageGenerator,
                                          FixSessionID sessionID,
                                          long orderID,
                                          string clOrdID,
                                          string origClOrdID,
                                          string execID)
        {
            return new CancelOrder(this,
                                   fixMessageGenerator,
                                   _orderMediator,
                                   sessionID,
                                   orderID,
                                   clOrdID,
                                   origClOrdID,
                                   execID);
        }

        public ICommand CreateSendAcceptNewOrder(FixSessionID sessionID,
                                                 IOrder order)
        {
            return new SendAcceptNewOrder(_sessionMediator,
                                          sessionID,
                                          order);
        }

        public ICommand CreateSendRejectNewOrder(IFixMessageGenerator fixMessageGenerator,
                                                 FixSessionID sessionID,
                                                 OrderData orderData,
                                                 string execID,
                                                 string rejectionMessage,
                                                 int? rejectionCode)
        {
            return new SendRejectNewOrder(fixMessageGenerator,
                                          _sessionMediator,
                                          orderData,
                                          execID,
                                          rejectionMessage,
                                          rejectionCode,
                                          sessionID);
        }

        public ICommand CreateSendAcceptOrderCancel(IFixMessageGenerator messageGenerator,
                                                    IOrder cancelledOrder,
                                                    string execID,
                                                    FixSessionID sessionID)
        {
            return new SendAcceptOrderCancel(messageGenerator,
                                             _sessionMediator,
                                             cancelledOrder,
                                             execID,
                                             sessionID);
        }

        public ICommand CreateSendRejectOrderCancel(IFixMessageGenerator messageGenerator,
                                                    int rejectionReason,
                                                    string rejectionReasonText,
                                                    long orderID,
                                                    string clOrdID,
                                                    string origClOrdID,
                                                    FixSessionID sessionID)
        {
            return new SendRejectOrderCancel(messageGenerator,
                                             _sessionMediator,
                                             rejectionReason,
                                             rejectionReasonText,
                                             orderID,
                                             clOrdID,
                                             origClOrdID,
                                             sessionID);
        }

        public ICommand CreateSendOrderFill(OrderMatch orderMatch, FixSessionID sessionID)
        {
            return new SendOrderFill(_sessionMediator, orderMatch, sessionID);
        }

        public ICommand CreateMatchOrders(string symbol)
        {
            return new MatchOrders(_orderMediator, symbol);
        }
    }
}