using System.Threading;
using System.Threading.Tasks;

namespace Heathmill.FixAT.Server
{
    internal class CommandProcessor
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ICommandQueue _commandQueue;
        private readonly ICommandProcessingStrategy _processingStrategy;

        public CommandProcessor(ICommandProcessingStrategy processingStrategy,
                                ICommandQueue commandQueue)
        {
            _processingStrategy = processingStrategy;
            _commandQueue = commandQueue;

            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;
            var task = new Task(
                () =>
                    {
                        while (!token.IsCancellationRequested)
                        {
                            var cmd = _commandQueue.Dequeue();
                            while (cmd != null)
                            {
                                _processingStrategy.ProcessCommand(cmd.Execute);
                                cmd = commandQueue.Dequeue();
                            }
                            Thread.Sleep(100);
                        }
                    },
                token,
                TaskCreationOptions.LongRunning);
            task.Start();
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}