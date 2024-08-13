using Domain.Entities;
using Infrastructure.BaseController;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace MenuMicroservice.Controllers;

[ApiController]
[Route("front/api/[controller]")]
public class CustomerController(IGenericCoreService<Customer> dataService) : GenericController<Customer>(dataService)
{
    [HttpPost]
    [Route("ValidateUser")]
    public ActionResult<Customer?> ValidateUser(ValidateUserRequest validateUserRequest)
    {
        var customer = _dataService
            .GetQueryable(x => x.FirstName == validateUserRequest.Firstname && x.Email == validateUserRequest.Email).FirstOrDefault();

        if (customer == null)
            return NotFound();

        return Ok(customer);
    }
    public record ValidateUserRequest(string Firstname, string Email);
}