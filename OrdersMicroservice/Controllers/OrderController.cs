using Domain.Models;
using Infrastructure.BaseController;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace OrdersMicroservice.Controllers;

[ApiController]
[Route("orders/api/[controller]")]
public class OrderController(
    IGenericCoreService<Order> dataService, 
    IGenericCoreService<Customer> customerService, 
    IGenericCoreService<MenuItem> menuItemService,
    IGenericCoreService<OrderItem> orderItemService) : GenericController<Order>(dataService)
{
    [HttpGet]
    [Route("GetOrderDetails/{orderId}")]
    public async Task<ActionResult<GetMyOrderDetailResponse>> GetOrderDetailsAsync(int orderId)
    {
        var orderItems = orderItemService.GetQueryable(x => x.OrderID == orderId).ToList();

        var menuItems = new List<MenuItem>();

        foreach (var orderItem in orderItems) 
        {
            menuItems.Add(await menuItemService.GetById(orderItem.MenuItemID) ?? throw new Exception());
        }

        return Ok(new GetMyOrderDetailResponse(menuItems));
    }
    public record GetMyOrderDetailResponse(List<MenuItem>? Items);

    [HttpGet]
    [Route("GetMyOrders/{mail}")]

    public ActionResult<GetMyOrdersResponse> GetMyOrders(string mail)
    {
        var customer = customerService.GetQueryable(x=> x.Email == mail).FirstOrDefault();

        if (customer is null)
            return new GetMyOrdersResponse([]);

        var orders = _dataService.GetQueryable(x => x.CustomerID == customer.Id).ToList();

        return Ok(new GetMyOrdersResponse(orders));
    }
    public record GetMyOrdersResponse(List<Order>? Items);

    [HttpPost]
    [Route("SaveOrder")]
    public async Task<ActionResult<int>> SaveOrderAsync(SendOrderRequest sendOrderRequest) 
    {
        int customerId;
        var customer = customerService.GetQueryable(x => x.Email == sendOrderRequest.CustomerEmail).FirstOrDefault();

        if (customer is null)
        {
            var newCustomer = await customerService.Insert(new Customer
            {
                FirstName = sendOrderRequest.CustomerFirstname,
                LastName = sendOrderRequest.CustomerLastname,
                Email = sendOrderRequest.CustomerEmail,
                Phone = sendOrderRequest.CustomerPhone
            }) ?? throw new Exception("Customer creation failed");

            customerId = newCustomer.Id;
        }
        else
            customerId = customer.Id;

        var orderTotal = 0m;
        var menuItems = new Dictionary<int, MenuItem>();
        foreach (var orderItem in sendOrderRequest.OrderItems) 
        {
            var menuItem = await menuItemService.GetById(orderItem.Key);
            if (menuItem is not null) 
            {
                orderTotal += menuItem.Price * orderItem.Value;
                menuItems.Add(menuItem.Id, menuItem);
            }
        }

        var newOrder = await _dataService.Insert(new Order
        {
            CustomerID = customerId,
            Status = "Pending",
            TotalAmount = orderTotal,
            OrderDate = DateTime.Now
        }) ?? throw new Exception("newOrder creation failed");

        foreach (var orderItem in sendOrderRequest.OrderItems)
        {
            await orderItemService.Insert(new OrderItem 
            {
                UnitPrice = menuItems[orderItem.Key].Price,
                Quantity = orderItem.Value,
                MenuItemID = orderItem.Key,
                OrderID = newOrder.Id
            });
        }

        return Ok(newOrder.Id);
    }

    public record SendOrderRequest(Dictionary<int, int> OrderItems, string CustomerEmail, 
        string CustomerFirstname, string CustomerLastname, string CustomerPhone);
}
