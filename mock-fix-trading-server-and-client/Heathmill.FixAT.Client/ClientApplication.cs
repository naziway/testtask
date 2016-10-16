using System;
using Heathmill.FixAT.Services;

namespace Heathmill.FixAT.Client
{
    /// <summary>
    /// Largely based on the quickfixn UIDemo application class UIDemo.QFApp
    /// </summary>
    public class ClientApplication : QuickFix.MessageCracker, QuickFix.IApplication
    {
        public QuickFix.SessionID ActiveSessionID { get; set; }
        public QuickFix.SessionSettings MySessionSettings { get; set; }

        private readonly IFixMessageGenerator _messageGenerator;

        private readonly IFixStrategy _strategy;
        private readonly IMessageSink _messageSink;

        private QuickFix.IInitiator _initiator;

        public QuickFix.IInitiator Initiator
        {
            set
            {
                if (_initiator != null)
                    throw new ApplicationException("Initiator has already been set");
                _initiator = value;
            }
            get
            {
                if (_initiator == null)
                    throw new ApplicationException("Initiator has not been set");
                return _initiator;
            }
        }

        public event Action LogonEvent;
        public event Action LogoutEvent;

        public event Action<QuickFix.FIX44.ExecutionReport> Fix44ExecReportEvent;

        /// <summary>
        /// Triggered on any message sent or received (bool: isIncoming)
        /// </summary>
        public event Action<QuickFix.Message, bool> MessageEvent;


        public ClientApplication(
            QuickFix.SessionSettings settings,
            IFixMessageGenerator messageGenerator,
            IFixStrategy strategy,
            IMessageSink messageSink)
        {
            _messageGenerator = messageGenerator;
            _strategy = strategy;
            _messageSink = messageSink;
            ActiveSessionID = null;
            MySessionSettings = settings;
        }

        public void Start()
        {
            _messageSink.Trace(() => "ClientApplication::Start() called");
            if (Initiator.IsStopped)
                Initiator.Start();
            else
                _messageSink.Trace(() => "(already started)");
        }

        public void Stop()
        {
            _messageSink.Trace(() => "ClientApplication::Stop() called");
            Initiator.Stop();
        }

        /// <summary>
        /// Tries to send the message; throws if not logged on.
        /// </summary>
        /// <param name="m"></param>
        public bool Send(QuickFix.Message m)
        {
            if (Initiator.IsLoggedOn == false)
            {
                _messageSink.Error(() => "Can't send a message.  We're not logged on.");
                return false;
            }
            if (ActiveSessionID == null)
            {
                _messageSink.Error(
                    () => "Can't send a message.  ActiveSessionID is null (not logged on?).");
                return false;
            }

            return QuickFix.Session.SendToTarget(m, ActiveSessionID);
        }

        #region Application Members

        public void FromAdmin(QuickFix.Message message, QuickFix.SessionID sessionID)
        {
            if (message.Header.GetString(35) == QuickFix.FIX44.Reject.MsgType)
                _messageSink.Message(() => "REJECT RECEIVED: " + message);

            _messageSink.Trace(() => "## FromAdmin: " + message);
            if (MessageEvent != null)
                MessageEvent(message, true);
        }

        public void FromApp(QuickFix.Message message, QuickFix.SessionID sessionID)
        {
            _messageSink.Trace(() => "## FromApp: " + message);
            if (MessageEvent != null)
                MessageEvent(message, true);
            Crack(message, sessionID);
        }

        public void OnCreate(QuickFix.SessionID sessionID)
        {
            _messageSink.Trace(() => "## OnCreate: " + sessionID);
        }

        public void OnLogon(QuickFix.SessionID sessionID)
        {
            ActiveSessionID = sessionID;
            _messageSink.Trace(() => String.Format("==OnLogon: {0}==", ActiveSessionID));
            if (LogonEvent != null)
                LogonEvent();
        }

        public void OnLogout(QuickFix.SessionID sessionID)
        {
            // not sure how ActiveSessionID could ever be null, but it happened.
            var a = (ActiveSessionID == null) ? "null" : ActiveSessionID.ToString();
            _messageSink.Trace(() => String.Format("==OnLogout: {0}==", a));
            if (LogoutEvent != null)
                LogoutEvent();
        }

        public void ToAdmin(QuickFix.Message message, QuickFix.SessionID sessionID)
        {
            _strategy.ProcessToAdmin(message, QuickFix.Session.LookupSession(sessionID));
            _messageSink.Trace(() => "## ToAdmin: " + message);
            if (MessageEvent != null)
                MessageEvent(message, false);
        }

        public void ToApp(QuickFix.Message message, QuickFix.SessionID sessionId)
        {
            _strategy.ProcessToApp(message, QuickFix.Session.LookupSession(sessionId));
            _messageSink.Trace(() => "## ToApp: " + message);
            if (MessageEvent != null)
                MessageEvent(message, false);
        }

        #endregion

        public void OnMessage(QuickFix.FIX44.BusinessMessageReject msg, QuickFix.SessionID s)
        {
            _messageSink.Error(() => "BusinessMessageReject: " + msg);
        }

        public void OnMessage(QuickFix.FIX44.OrderCancelReject msg, QuickFix.SessionID s)
        {
            _messageSink.Error(
                () => string.Format("OrderCanel rejected for order ClOrdID {0}", msg.ClOrdID));
        }

        public void OnMessage(QuickFix.FIX44.ExecutionReport msg, QuickFix.SessionID s)
        {
            if (Fix44ExecReportEvent != null)
                Fix44ExecReportEvent(msg);
        }

        public void OnMessage(QuickFix.FIX44.News msg, QuickFix.SessionID s)
        {
            if (msg.Headline.Obj == "TESTECHO")
                Send(_messageGenerator.CreateNews("ECHORESPONSE"));
        }
    }
}