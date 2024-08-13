using Domain.Entities;
using Infrastructure.BaseController;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace MenuMicroservice.Controllers;

[ApiController]
[Route("menu/api/[controller]")]
public class MenuItemController(IGenericCoreService<MenuItem> dataService): GenericController<MenuItem>(dataService)
{
}