using HahnCargoDelivery.Models;

namespace HahnCargoDelivery.Dtos.Authentication;

public class SimulationStateDto
{
    public bool IsSimulationStarted { get; set; }
    public Grid Grid { get; set; }
    public List<TransporterInfo> Transporters { get; set; }
    
    public List<Order> DeliveredOrders { get; set; }
    public int CoinAmount { get; set; }
}