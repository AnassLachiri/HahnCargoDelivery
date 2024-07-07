using HahnCargoDelivery.Configs;
using HahnCargoDelivery.Models;
using Microsoft.Extensions.Options;

namespace HahnCargoDelivery.Services;

public interface IOrderService
{
    public Task AcceptOrder(int orderId);
    public Task<List<Order>> GetAllOrders();
}
public class OrderService: IOrderService
{
    private readonly IExternalApiService _externalApiService;
    private readonly HahnCargoSimApiConfig _hahnCargoSimApiConfig;

    public OrderService(IExternalApiService externalApiService, IOptions<HahnCargoSimApiConfig> hahnCargoSimApiConfig)
    {
        if (hahnCargoSimApiConfig.Value == null)
        {
            throw new Exception("Configs can't be null");
        }
        _externalApiService = externalApiService;
        _hahnCargoSimApiConfig = hahnCargoSimApiConfig.Value;
    }
    
    public async Task AcceptOrder(int orderId)
    {
        await _externalApiService.PostAsync(_hahnCargoSimApiConfig.Uri + $"order/accept?orderId={orderId}", null);
    }
    
    public async Task<List<Order>> GetAllOrders()
    {
        var orders = await _externalApiService.GetAsync<List<Order>>(_hahnCargoSimApiConfig.Uri + $"Order/GetAllAvailable");
        if (orders == null)
        {
            throw new Exception("Something went wrong with fetching all orders");
        }

        return orders;
    }
}