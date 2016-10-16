using System.Collections.Generic;
using Heathmill.FixAT.Server.Commands;

namespace Heathmill.FixAT.Server
{
    internal class CommandQueue : ICommandQueue
    {
        private readonly Queue<ICommand> _queue = new Queue<ICommand>();
        private readonly object _queueLock = new object();

        public void Enqueue(ICommand command)
        {
            lock (_queueLock)
            {
                _queue.Enqueue(command);
            }
        }

        public ICommand Dequeue()
        {
            lock (_queueLock)
            {
                return _queue.Count > 0 ? _queue.Dequeue() : null;
            }
        }

        public void Clear()
        {
            lock (_queueLock)
            {
                _queue.Clear();
            }
        }
    }
}