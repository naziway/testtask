using System.Collections.Generic;

namespace Heathmill.FixAT.Client.Model
{
    public class ATOrderRepository : IATOrderRepository
    {
        public ATOrderRepository()
        {
            IcebergOrders = new List<IcebergOrder>();
        }

        public List<IcebergOrder> IcebergOrders { get; private set; }
    }
}
