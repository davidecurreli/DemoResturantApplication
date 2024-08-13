using Domain.Entities;
using Infrastructure.BaseController;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace MenuMicroservice.Controllers;

[ApiController]
[Route("menu/api/[controller]")]
public class CustomerController(IGenericCoreService<Customer> dataService) : GenericController<Customer>(dataService)
{
}