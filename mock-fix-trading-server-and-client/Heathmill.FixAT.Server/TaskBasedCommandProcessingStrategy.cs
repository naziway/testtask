using System;
using System.Threading.Tasks;

namespace Heathmill.FixAT.Server
{
    internal class TaskBasedCommandProcessingStrategy : ICommandProcessingStrategy
    {
        private readonly TaskFactory _taskFactory = new TaskFactory();

        public void ProcessCommand(Action processingFunction)
        {
            _taskFactory.StartNew(processingFunction);
        }
    }
}