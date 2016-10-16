
namespace Heathmill.FixAT.Domain
{
    public class Contract
    {
        // TODO Make this a bit less basic! Futures, dates, etc etc
        public Contract(string symbol)
        {
            Symbol = symbol;
        }

        public string Symbol { get; private set; }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Contract))
                return false;

            return Symbol == ((Contract) obj).Symbol;
        }

        public override int GetHashCode()
        {
            return Symbol.GetHashCode();
        }

        public override string ToString()
        {
            return Symbol;
        }
    }
}
