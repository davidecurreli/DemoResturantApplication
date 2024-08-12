using Domain.Models;
using Infrastructure.BaseController;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace OrdersMicroservice.Controllers;

[ApiController]
[Route("orders/api/[controller]")]
public class OrderController(IGenericCoreService<Order> dataService) : GenericController<Order>(dataService)
{
}
