using System;

namespace Heathmill.FixAT.Server
{
    public class FixSessionID
    {
        public FixSessionID()
        {
            ID = Guid.NewGuid();
        }

        public Guid ID { get; private set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            return ID.Equals(((FixSessionID) obj).ID);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override string ToString()
        {
            return ID.ToString();
        }
    }
}
