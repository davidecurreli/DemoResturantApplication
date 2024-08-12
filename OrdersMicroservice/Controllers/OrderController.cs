using Domain.Models;
using Infrastructure.BaseController;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace OrdersMicroservice.Controllers;

[ApiController]
[Route("orders/api/[controller]")]
public class OrderController(IGenericCoreService<Order> dataService, IGenericCoreService<Customer> customerService) : GenericController<Order>(dataService)
{
    [HttpGet]
    [Route("GetMyOrders/{mail}")]
    public ActionResult<dynamic> GetMyOrders(string mail)
    {
        var customer = customerService.GetQueryable(x=> x.Email == mail).FirstOrDefault();

        if (customer is null)
            return new GetMyOrdersResponse([]);

        var orders = _dataService.GetQueryable(x => x.CustomerID == customer.Id).ToList();

        return Ok(new GetMyOrdersResponse(orders));
    }

    public record GetMyOrdersResponse(List<Order>? Items);
}
