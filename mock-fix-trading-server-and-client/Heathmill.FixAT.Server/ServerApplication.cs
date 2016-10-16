using System;
using System.Globalization;
using Heathmill.FixAT.Domain;
using Heathmill.FixAT.Server.Commands;
using Heathmill.FixAT.Services;
using QuickFix;
using QuickFix.FIX44;
using QuickFix.FixValues;
using Message = QuickFix.Message;

namespace Heathmill.FixAT.Server
{
    public class ServerApplication : MessageCracker, IApplication
    {
        // Implement other message handlers to support other FIX versions
        private readonly Fix42MessageHandler _fix42MessageHandler;
        private readonly Fix44MessageHandler _fix44MessageHandler;

        private readonly CommandProcessor _inputCommandProcessor;
        private readonly CommandQueue _inputQueue;
        private readonly CommandProcessor _outputCommandProcessor;
        private readonly CommandQueue _outputQueue;

        private readonly CommandFactory _commandFactory;
        private readonly Action<string> _messageCallback;
        private readonly OrderMediator _orderMediator;
        private readonly SessionMediator _sessionMediator;

        // TODO A better ID tracking system?
        private UInt64 _execID;

        public ServerApplication(Action<string> messageCallback)
        {
            _messageCallback = messageCallback;

            // Were this a production server I'd recommend getting all these types from a
            // type provider layer (MEF, Castle, etc)
            var sessionRepository = new SessionRepository();
            var fixSessionFacade = new StandardFixFacade(messageCallback);
            _sessionMediator = new SessionMediator(sessionRepository, fixSessionFacade);

            _inputQueue = new CommandQueue();
            _outputQueue = new CommandQueue();

            var asyncProcessing = new TaskBasedCommandProcessingStrategy();
            var syncProcessing = new SynchronousCommandProcessingStrategy();
            _inputCommandProcessor = new CommandProcessor(asyncProcessing, _inputQueue);
            _outputCommandProcessor = new CommandProcessor(syncProcessing, _outputQueue);

            var orderMatcher = new StandardOrderMatcher();
            var orderRepository = new StandardOrderRepository(orderMatcher);
            _orderMediator = new OrderMediator(orderRepository, OnOrderMatched);

            _commandFactory = new CommandFactory(_inputQueue,
                                                 _outputQueue,
                                                 _orderMediator,
                                                 _sessionMediator);

            var messageHandlerCommandFactory = new MessageHandlerCommandFactory(_sessionMediator,
                                                                                _commandFactory);

            var fix42MessageGenerator = new Fix42MessageGenerator();
            _fix42MessageHandler = new Fix42MessageHandler(messageHandlerCommandFactory,
                                                           fix42MessageGenerator,
                                                           fixSessionFacade,
                                                           GenExecID);

            var fix44MessageGenerator = new Fix44MessageGenerator();
            _fix44MessageHandler = new Fix44MessageHandler(messageHandlerCommandFactory,
                                                           fix44MessageGenerator,
                                                           fixSessionFacade,
                                                           GenExecID);
        }

        public void Stop()
        {
            _inputQueue.Clear();
            _inputCommandProcessor.Stop();
            _outputQueue.Clear();
            _outputCommandProcessor.Stop();
        }

        private string GenExecID()
        {
            return (++_execID).ToString(CultureInfo.InvariantCulture);
        }

        private void OnOrderMatched(OrderMatch matchDetails, FixSessionID sessionID)
        {
            var cmd = _commandFactory.CreateSendOrderFill(matchDetails, sessionID);
            _outputQueue.Enqueue(cmd);
        }

        #region QuickFix.IApplication Methods

        public void FromApp(Message message, SessionID sessionID)
        {
            _messageCallback("IN:  " + message);
            try
            {
                Crack(message, sessionID);
            }
            catch (UnsupportedMessageType)
            {
                _messageCallback(
                    string.Format("Unsupported message type: {0}", message.GetType()));
            }
        }

        public void ToApp(Message message, SessionID sessionID)
        {
            _messageCallback("OUT: " + message);
        }

        public void FromAdmin(Message message, SessionID sessionID)
        {
            // Handle this if needed
        }

        public void OnCreate(SessionID sessionID)
        {
            try
            {
                _sessionMediator.AddSession(sessionID, GetHandler(sessionID)); 
            }
            catch (FixATServerException e)
            {
                _messageCallback("ERROR: " + e.Message);
                // TODO Log e.ToString()
            }
        }

        public void OnLogout(SessionID sessionID)
        {
            try
            {
                _sessionMediator.SessionLoggedOut(sessionID);

                // TODO Optional suspend or delete orders, for the moment hardcode to delete all
                var internalSessionID = _sessionMediator.LookupInternalSessionID(sessionID);
                _orderMediator.DeleteAllOrders(internalSessionID);
            }
            catch (FixATServerException e)
            {
                // TODO Should be warning?
                _messageCallback(e.Message);
            }
        }

        public void OnLogon(SessionID sessionID)
        {
            try
            {
                _sessionMediator.SessionLoggedIn(sessionID);
                _sessionMediator.SendOrders(sessionID, _orderMediator.GetAllOrders());
            }
            catch (FixATServerException e)
            {
                _messageCallback("ERROR: " + e.Message);
                // TODO Log e.ToString()
            }
        }

        public void ToAdmin(Message message, SessionID sessionID)
        {
            // Handle this if needed
        }

        private IFixMessageHandler GetHandler(SessionID sessionID)
        {
            switch (sessionID.BeginString)
            {
                case BeginString.FIX42:
                    return _fix42MessageHandler;
                case BeginString.FIX44:
                    return _fix44MessageHandler;
                default:
                    throw new FixATServerException(
                        string.Format("FIX version {0} not supported by server",
                                      sessionID.BeginString));
            }
        }

        #endregion

        #region FIX44 message handlers

        public void OnMessage(NewOrderSingle n, SessionID sessionID)
        {
            _fix44MessageHandler.OnMessage(n, sessionID);
        }

        public void OnMessage(News n, SessionID sessionID)
        {
            _fix44MessageHandler.OnMessage(n, sessionID);
        }

        public void OnMessage(OrderCancelRequest msg, SessionID sessionID)
        {
            _fix44MessageHandler.OnMessage(msg, sessionID);
        }

        public void OnMessage(OrderCancelReplaceRequest msg, SessionID sessionID)
        {
            _fix44MessageHandler.OnMessage(msg, sessionID);
        }

        public void OnMessage(BusinessMessageReject msg, SessionID sessionID)
        {
            _fix44MessageHandler.OnMessage(msg, sessionID);
        }

        #endregion

        #region FIX42 message handlers

        public void OnMessage(QuickFix.FIX42.NewOrderSingle n, SessionID sessionID)
        {
            _fix42MessageHandler.OnMessage(n, sessionID);
        }

        public void OnMessage(QuickFix.FIX42.News n, SessionID sessionID)
        {
            _fix42MessageHandler.OnMessage(n, sessionID);
        }

        public void OnMessage(QuickFix.FIX42.OrderCancelRequest msg, SessionID sessionID)
        {
            _fix42MessageHandler.OnMessage(msg, sessionID);
        }

        public void OnMessage(QuickFix.FIX42.OrderCancelReplaceRequest msg, SessionID sessionID)
        {
            _fix42MessageHandler.OnMessage(msg, sessionID);
        }

        public void OnMessage(QuickFix.FIX42.BusinessMessageReject msg, SessionID sessionID)
        {
            _fix42MessageHandler.OnMessage(msg, sessionID);
        }

        #endregion

        // TODO Try to auto-dispatch the messages to the appropriate handler.  TypeInfo based?
        // Whatever it is will need to place nice with MessageCracker.Crack, or I need to
        // do the FromApp typed dispatch another way (visitors?)

        // TODO Do we need to worry about permissions? 
        // Currently just restrict cancel to the same session.
        // Any other perms issues?  If we return a list of orders then must be anonymised.

        // TODO Improve logging and report errors, warnings, info etc
    }
}