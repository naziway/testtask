using System;

namespace Heathmill.FixAT.Server
{
    internal interface ICommandProcessingStrategy
    {
        void ProcessCommand(Action processingFunction);
    }
}