using System;
using System.Windows;
using Heathmill.FixAT.Client;

namespace Heathmill.FixAT.ATOrderBook
{
    public class StandardMessageSink : IMessageSink
    {
        private Action<string> _messageCallback;

        public void SetMessageSink(Action<string> messageCallback)
        {
            _messageCallback = messageCallback;
        }

        public void Trace(Func<string> message)
        {
            System.Diagnostics.Trace.WriteLine(message());
        }

        public void Message(Func<string> message)
        {
            if (_messageCallback != null)
                _messageCallback(message());
        }

        public void Error(Func<string> message)
        {
            MessageBox.Show(message(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
