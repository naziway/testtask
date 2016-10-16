using System.Collections.Generic;

namespace Heathmill.FixAT.Client.Model
{
    public interface IATOrderRepository
    {
        List<IcebergOrder> IcebergOrders { get; }
    }
}
