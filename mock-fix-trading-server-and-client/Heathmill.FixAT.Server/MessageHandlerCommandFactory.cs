using Heathmill.FixAT.Server.Commands;
using Heathmill.FixAT.Services;
using QuickFix;

namespace Heathmill.FixAT.Server
{
    /// <summary>
    /// Wrapper around the command factory so that the FIX message handlers can
    /// use Quickfix Session IDs when creating commands
    /// </summary>
    internal class MessageHandlerCommandFactory
    {
        private readonly SessionMediator _sessionMediator;
        private readonly CommandFactory _commandFactory;

        public MessageHandlerCommandFactory(SessionMediator sessionMediator,
                                            CommandFactory commandFactory)
        {
            _sessionMediator = sessionMediator;
            _commandFactory = commandFactory;
        }

        public void EnqueueAddOrder(IFixMessageGenerator messageGenerator,
                                    SessionID sessionID,
                                    OrderData orderData,
                                    string execID)
        {
            var internalSessionID = _sessionMediator.LookupInternalSessionID(sessionID);
            var cmd = _commandFactory.CreateAddOrder(messageGenerator,
                                                     internalSessionID,
                                                     orderData,
                                                     execID);
            _commandFactory.IncomingQueue.Enqueue(cmd);
        }

        public void EnqueueCancelOrder(IFixMessageGenerator messageGenerator,
                                       SessionID sessionID,
                                       long orderID,
                                       string clOrdID,
                                       string origClOrdID,
                                       string execID)
        {
            var internalSessionID = _sessionMediator.LookupInternalSessionID(sessionID);
            var cmd = _commandFactory.CreateCancelOrder(messageGenerator,
                                                        internalSessionID,
                                                        orderID,
                                                        clOrdID,
                                                        origClOrdID,
                                                        execID);
            _commandFactory.IncomingQueue.Enqueue(cmd);
        }
    }
}
