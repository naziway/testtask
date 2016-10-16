using System;

namespace Heathmill.FixAT.Server
{
    internal class SynchronousCommandProcessingStrategy : ICommandProcessingStrategy
    {
        public void ProcessCommand(Action processingFunction)
        {
            processingFunction();
        }
    }
}