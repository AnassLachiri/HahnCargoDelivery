namespace HahnCargoDelivery.Models;

public class SimulationState
{
    private readonly object _transportersLock = new();
    private List<TransporterInfo> _transporters = new();
    public bool IsSimulationStarted { get; set; } = false;
    public Grid Grid { get; set; }
    public Queue<Order> Orders = new();

    public List<Order> DeliveredOrders = new();

    public List<TransporterInfo> Transporters
    {
        get
        {
            lock (_transportersLock)
            {
                return _transporters;
            }
        }
        set
        {
            lock (_transportersLock)
            {
                _transporters = value;
            }
        }
    }
}