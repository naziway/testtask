using System;
using System.Windows;
using Heathmill.FixAT.Client;
using Heathmill.FixAT.Services;

namespace Heathmill.FixAT.ATOrderBook
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string ConfigFilepath = "quickfix.cfg";
        private AppRunner _appRunner;
        
        protected override void OnStartup(StartupEventArgs args)
        {
            base.OnStartup(args);
            //ShutdownMode = ShutdownMode.OnMainWindowClose;

            try
            {
                _appRunner = new AppRunner();
                var fixStrategy = new EmptyFixStrategy();
                var messageGenerator = new Fix44MessageGenerator();
                var messageSink = new StandardMessageSink();
                var execIDGenerator = new GuidExecIDGenerator();
                var clOrdIDGenerator = new IncrementingClOrdIDGenerator();
                _appRunner.Run(ConfigFilepath,
                               fixStrategy,
                               messageGenerator,
                               messageSink,
                               execIDGenerator,
                               clOrdIDGenerator);
            }
            catch (Exception ex)
            {
                //TODO Should we really have this?
                MessageBox.Show(
                    ex.ToString(),
                    "AT Order Book error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                //throw new ApplicationException("ATOrderBook Error", ex);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            _appRunner.Stop();
        }
    }
}
