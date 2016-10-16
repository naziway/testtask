
namespace Heathmill.FixAT.Client.ViewModel
{
    public class ConnectionViewModel : NotifyPropertyChangedBase
    {
        private readonly IMessageSink _messageSink;

        public ConnectionViewModel(IServerFacade serverFacade, IMessageSink messageSink)
        {
            _messageSink = messageSink;

            SessionString = serverFacade.GetServerSessionID();

            serverFacade.LogonEvent += OnLogon;
            serverFacade.LogoutEvent += OnLogout;
        }

        private void OnLogon()
        {
            _messageSink.Trace(() => "ConnectionViewModel.OnLogon");
            ConnectionStatus = "Connected";
        }

        private void OnLogout()
        {
            _messageSink.Trace(() => "ConnectionViewModel.OnLogout");
            ConnectionStatus = "Disconnected ... attempting to reconnect";
        }

        private string _session = "";
        public string SessionString
        {
            get { return _session; }
            set { _session = value; base.OnPropertyChanged("SessionString"); }
        }

        private string _connectionStatus = "Disconnected";
        public string ConnectionStatus
        {
            get { return _connectionStatus; }
            set { _connectionStatus = value; OnPropertyChanged("ConnectionStatus"); }
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get { return _statusMessage; }
            set { _statusMessage = value; OnPropertyChanged("StatusMessage"); }
        }

    }
}
