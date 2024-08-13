using Domain.Entities;
using Infrastructure.Repositories;

namespace Infrastructure.Services;

public class OrderServices
{
    public class OrderService(IGenericRepository<Order> repository) : GenericCoreService<IGenericRepository<Order>, Order>(repository), IGenericCoreService<Order>
    {
    }

    public class OrderItemService(IGenericRepository<OrderItem> repository) : GenericCoreService<IGenericRepository<OrderItem>, OrderItem>(repository), IGenericCoreService<OrderItem>
    {
    }
}
