using System;
using System.Collections.Generic;
using Heathmill.FixAT.Domain;
using Heathmill.FixAT.Services;
using QuickFix;
using QuickFix.FIX44;
using QuickFix.Fields;

namespace Heathmill.FixAT.Server
{
    internal class Fix44MessageHandler : IFixMessageHandler
    {
        private readonly MessageHandlerCommandFactory _commandFactory;
        private readonly Func<string> _execIdGenerator;
        private readonly IFixFacade _fixFacade;
        private readonly IFixMessageGenerator _messageGenerator;

        public Fix44MessageHandler(MessageHandlerCommandFactory commandFactory,
                                   IFixMessageGenerator messageGenerator,
                                   IFixFacade fixFacade,
                                   Func<string> execIdGenerator)
        {
            _commandFactory = commandFactory;
            _messageGenerator = messageGenerator;
            _fixFacade = fixFacade;
            _execIdGenerator = execIdGenerator;
        }

        public void OnOrderFilled(SessionID sessionID, OrderMatch match)
        {
            // Send the order filled execution report to the owning session
            var message = _messageGenerator.CreateFillReport(match, _execIdGenerator());
            _fixFacade.SendToTarget(message, sessionID);

            // TODO Send the trade message to other connections
            //var loggedInSessions = _sessionMediator.GetLoggedInSessions();
        }

        public void SendOrdersToSession(SessionID sessionID, IEnumerable<IOrder> orders)
        {
            foreach (var order in orders)
            {
                var msg =
                    _messageGenerator.CreateNewOrderExecutionReport(order, _execIdGenerator());
                _fixFacade.SendToTarget(msg, sessionID);
            }
        }

        public void OnMessage(NewOrderSingle n, SessionID sessionID)
        {
            var execID = _execIdGenerator();
            try
            {
                var orderData = TranslateFixMessages.Translate(n);
                _commandFactory.EnqueueAddOrder(_messageGenerator, sessionID, orderData, execID);
            }
            catch (QuickFIXException e)
            {
                var rejectMessage = "Unable to add order: " + e.Message;
                var message = CreateFix44Message.CreateRejectNewOrderExecutionReport(n,
                                                                                     execID,
                                                                                     rejectMessage);
                _fixFacade.SendToTarget(message, sessionID);
            }
        }

        public void OnMessage(SecurityDefinition secDef, SessionID sessionID)
        {
            // Add handling if needed
        }

        public void OnMessage(News n, SessionID sessionID)
        {
            // Add handling if needed
        }

        public void OnMessage(OrderCancelRequest msg, SessionID sessionID)
        {
            var execID = _execIdGenerator();
            try
            {
                var orderID = TranslateFixMessages.GetOrderIdFromMessage(msg);
                _commandFactory.EnqueueCancelOrder(_messageGenerator,
                                                   sessionID,
                                                   orderID,
                                                   msg.ClOrdID.getValue(),
                                                   msg.OrigClOrdID.getValue(),
                                                   execID);
            }
            catch (QuickFIXException e)
            {
                var reply = CreateFix44Message.CreateOrderCancelReject(msg,
                                                                       CxlRejReason
                                                                           .OTHER,
                                                                       e.Message);
                _fixFacade.SendToTarget(reply, sessionID);
            }
        }

        public void OnMessage(OrderCancelReplaceRequest msg, SessionID sessionID)
        {
            var ocj = CreateFix44Message.CreateOrderCancelReplaceReject(
                msg,
                CxlRejReason.OTHER,
                "Server currently does not support order cancel/replaces");

            _fixFacade.SendToTarget(ocj, sessionID);
        }

        public void OnMessage(BusinessMessageReject n, SessionID sessionID)
        {
            // TODO Handle this if needed
        }
    }
}