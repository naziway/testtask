using System;
using System.Windows;
using System.Windows.Threading;

namespace Heathmill.WpfUtilities
{
    public interface IMarshallToUi
    {
        void Marshall(Action uiAction);
        Exception MarshallWait(Action uiAction);
    }

    public class SimpleUiExecutive : IMarshallToUi
    {
        private readonly Dispatcher _uiDispatcher;

        public SimpleUiExecutive() : this(Application.Current.Dispatcher)
        {
        }

        public SimpleUiExecutive(Dispatcher wpfDispatcher)
        {
            _uiDispatcher = wpfDispatcher;
        }

        public void Marshall(Action uiAction)
        {
            if (_uiDispatcher.CheckAccess())
            {
                uiAction();
            }
            else
            {
                _uiDispatcher.BeginInvoke(uiAction, DispatcherPriority.DataBind);
            }
        }

        public Exception MarshallWait(Action uiAction)
        {
            Func<Exception> f = () => TryUiAction(uiAction);
            if (_uiDispatcher.CheckAccess())
            {
                return f();
            }
            object ex = _uiDispatcher.Invoke(f, DispatcherPriority.DataBind);
            return ex as Exception;
        }

        private Exception TryUiAction(Action uiAction)
        {
            try
            {
                uiAction();
            }
            catch (Exception ex)
            {
                return ex;
            }
            return null;
        }
    }
}
