using Domain.Entities;

namespace OrdersMicroservice.Models;

public record GetMyOrdersResponse(List<Order>? Items);

public record GetMyOrderDetailResponse(List<MenuItem>? Items);

public record SendOrderRequest(Dictionary<int, int> OrderItems, string CustomerEmail,
    string CustomerFirstname, string CustomerLastname, string CustomerPhone);
