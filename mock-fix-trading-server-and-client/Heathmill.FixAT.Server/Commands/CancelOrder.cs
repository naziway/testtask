using Heathmill.FixAT.Services;
using QuickFix.Fields;

namespace Heathmill.FixAT.Server.Commands
{
    internal class CancelOrder : ICommand
    {
        private readonly string _clOrdID;
        private readonly CommandFactory _commandFactory;
        private readonly string _execID;
        private readonly IFixMessageGenerator _messageGenerator;
        private readonly long _orderID;
        private readonly OrderMediator _orderMediator;
        private readonly string _origClOrdID;
        private readonly FixSessionID _sessionID;


        public CancelOrder(CommandFactory commandFactory,
                           IFixMessageGenerator messageGenerator,
                           OrderMediator orderMediator,
                           FixSessionID sessionID,
                           long orderID,
                           string clOrdID,
                           string origClOrdID,
                           string execID)
        {
            _commandFactory = commandFactory;
            _messageGenerator = messageGenerator;
            _orderMediator = orderMediator;
            _sessionID = sessionID;
            _orderID = orderID;
            _clOrdID = clOrdID;
            _origClOrdID = origClOrdID;
            _execID = execID;
        }

        public void Execute()
        {
            try
            {
                var cancelledOrder = _orderMediator.CancelOrder(_orderID, _sessionID);
                var accept = _commandFactory.CreateSendAcceptOrderCancel(_messageGenerator,
                                                                         cancelledOrder,
                                                                         _execID,
                                                                         _sessionID);
                _commandFactory.OutgoingQueue.Enqueue(accept);
            }
            catch (FixATServerException e)
            {
                var rejReasonText = e.Message;
                var rejReason = CxlRejReason.OTHER;
                if (e.Data.Contains(OrderMediator.RejectReasonExceptionString))
                {
                    var reason =
                        (OrderMediator.OrderCancelRejectReason)
                        e.Data[OrderMediator.RejectReasonExceptionString];
                    switch (reason)
                    {
                        case OrderMediator.OrderCancelRejectReason.OrderNotFound:
                            rejReason = CxlRejReason.UNKNOWN_ORDER;
                            break;
                        default:
                            rejReason = CxlRejReason.OTHER;
                            break;
                    }
                }

                var reject = _commandFactory.CreateSendRejectOrderCancel(_messageGenerator,
                                                                         rejReason,
                                                                         rejReasonText,
                                                                         _orderID,
                                                                         _clOrdID,
                                                                         _origClOrdID,
                                                                         _sessionID);
                _commandFactory.OutgoingQueue.Enqueue(reject);
            }
        }
    }
}