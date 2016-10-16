using System;

namespace Heathmill.FixAT.Client
{
    public class GuidExecIDGenerator : IExecIDGenerator
    {
        public string CreateExecID()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
