using System;
using System.Runtime.Serialization;

namespace Heathmill.FixAT.Domain
{
    [Serializable]
    public class DomainException : ApplicationException
    {
        public DomainException()
        {}

        public DomainException(string message)
            : base(message)
        {}

        public DomainException(string message, Exception innerException)
            : base(message, innerException)
        {}

        protected DomainException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }
}
