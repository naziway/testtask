using System;

namespace Heathmill.FixAT.Server
{
    // This is not a serializable exception, do not allow it to cross AppDomain boundaries
    internal class FixATServerException : ApplicationException
    {
        public FixATServerException(string message)
            : base(message)
        {
        }

        public FixATServerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public int? RejectionCode { get; set; }
    }
}