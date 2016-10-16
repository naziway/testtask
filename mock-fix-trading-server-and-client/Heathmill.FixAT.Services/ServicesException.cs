using System;
using System.Runtime.Serialization;

namespace Heathmill.FixAT.Services
{
    [Serializable]
    public class ServicesException : ApplicationException
    {
        public ServicesException()
        {
        }

        public ServicesException(string message)
            : base(message)
        {
        }

        public ServicesException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ServicesException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


    }
}
