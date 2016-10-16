using System.Globalization;
using System.Threading;

namespace Heathmill.FixAT.Client
{
    public class IncrementingClOrdIDGenerator : IClOrdIDGenerator
    {
        private const int RegularOffset = 1000;
        private const int ATOffset = 5000;
        private int _counter = 0;

        public string CreateClOrdID()
        {
            return (RegularOffset + Increment()).ToString(CultureInfo.CurrentUICulture);
        }

        public string CreateATClOrdID()
        {
            return (ATOffset + Increment()).ToString(CultureInfo.CurrentUICulture);
        }

        private int Increment()
        {
            return Interlocked.Increment(ref _counter);
        }
}
}
