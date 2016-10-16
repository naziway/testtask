using System;
using System.Collections.Generic;
using Heathmill.FixAT.Domain;
using Heathmill.FixAT.Services;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX42;

namespace Heathmill.FixAT.Server
{
    // TODO Factor out common code with the other handlers

    internal class Fix42MessageHandler : IFixMessageHandler
    {
        private readonly MessageHandlerCommandFactory _commandFactory;
        private readonly Func<string> _execIdGenerator;
        private readonly IFixFacade _fixFacade;
        private readonly IFixMessageGenerator _messageGenerator;


        public Fix42MessageHandler(MessageHandlerCommandFactory commandFactory,
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
                var message = CreateFix42Message.CreateRejectNewOrderExecutionReport(n,
                                                                                     execID,
                                                                                     rejectMessage);
                _fixFacade.SendToTarget(message, sessionID);
            }
        }

        public void OnMessage(News n, SessionID s)
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
                var reply = CreateFix42Message.CreateOrderCancelReject(msg,
                                                                       CxlRejReason
                                                                           .OTHER,
                                                                       e.Message);
                _fixFacade.SendToTarget(reply, sessionID);
            }
        }

        public void OnMessage(OrderCancelReplaceRequest msg, SessionID s)
        {
            var ocj = CreateFix42Message.CreateOrderCancelReplaceReject(
                msg,
                CxlRejReason.OTHER,
                "Server currently does not support order cancel/replaces");

            _fixFacade.SendToTarget(ocj, s);
        }

        public void OnMessage(BusinessMessageReject n, SessionID s)
        {
            // Add handling if needed
        }

        public void OnMessage(SecurityDefinition secDef, SessionID sessionID)
        {
            // Add handling if needed
        }
    }
}