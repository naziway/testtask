using Heathmill.FixAT.Services;

namespace Heathmill.FixAT.Server.Commands
{
    internal class SendRejectOrderCancel : ICommand
    {
        private readonly string _clOrdID;
        private readonly IFixMessageGenerator _messageGenerator;
        private readonly SessionMediator _sessionMediator;
        private readonly long _orderID;
        private readonly string _origClOrdID;
        private readonly int _rejectionReason;
        private readonly string _rejectionReasonText;
        private readonly FixSessionID _sessionID;

        public SendRejectOrderCancel(IFixMessageGenerator messageGenerator,
                                     SessionMediator sessionMediator,
                                     int rejectionReason,
                                     string rejectionReasonText,
                                     long orderID,
                                     string clOrdID,
                                     string origClOrdID,
                                     FixSessionID sessionID)
        {
            _messageGenerator = messageGenerator;
            _sessionMediator = sessionMediator;
            _rejectionReason = rejectionReason;
            _rejectionReasonText = rejectionReasonText;
            _orderID = orderID;
            _clOrdID = clOrdID;
            _origClOrdID = origClOrdID;
            _sessionID = sessionID;
        }

        public void Execute()
        {
            var reject = _messageGenerator.CreateOrderCancelReject(_orderID,
                                                                   _clOrdID,
                                                                   _origClOrdID,
                                                                   _rejectionReason,
                                                                   _rejectionReasonText);
            _sessionMediator.SendMessage(reject, _sessionID);
        }
    }
}