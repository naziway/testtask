
namespace Heathmill.FixAT.Domain
{
    public enum OrderStatus
    {
        Unknown,
        New,
        PartiallyFilled,
        Filled,
        Canceled,
        Replaced,
        Rejected,
        Suspended,
        Traded
    };
}
