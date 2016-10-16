
using System;

namespace Heathmill.FixAT.Client
{
    public interface IMessageSink
    {
        void SetMessageSink(Action<string> messageCallback);

        void Trace(Func<string> message);
        void Message(Func<string> message);
        void Error(Func<string> message);
    }
}
