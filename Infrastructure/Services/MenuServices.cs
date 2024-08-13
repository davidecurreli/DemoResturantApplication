using Domain.Entities;
using Infrastructure.Repositories;

namespace Infrastructure.Services;

public class MenuServices
{
    public class MenuService(IGenericRepository<MenuItem> repository) : GenericCoreService<IGenericRepository<MenuItem>, MenuItem>(repository), IGenericCoreService<MenuItem>
    {
    }
    public class CustomerService(IGenericRepository<Customer> repository) : GenericCoreService<IGenericRepository<Customer>, Customer>(repository), IGenericCoreService<Customer>
    {
    }
}
