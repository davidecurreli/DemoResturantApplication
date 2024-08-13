using Domain.Entities;
using Infrastructure.BaseController;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace OrdersMicroservice.Controllers;

[ApiController]
[Route("orders/api/[controller]")]
public class OrderItemController(IGenericCoreService<OrderItem> dataService) : GenericController<OrderItem>(dataService)
{
}
