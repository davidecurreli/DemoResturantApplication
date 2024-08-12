using Domain.Models;
using Infrastructure.BaseController;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace MenuMicroservice.Controllers;

[ApiController]
[Route("menu/api/[controller]")]
public class MenuItemController(IGenericCoreService<Menu> dataService): GenericController<Menu>(dataService)
{
}