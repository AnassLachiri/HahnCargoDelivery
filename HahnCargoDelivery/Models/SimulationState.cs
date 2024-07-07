namespace HahnCargoDelivery.Models;

public class SimulationState
{
    private readonly object _transportersLock = new object();
    private List<TransporterInfo> _transporters = new List<TransporterInfo>();
    public bool IsSimulationStarted { get; set; } = false;
    public Grid Grid { get; set; }
    public Queue<Order> Orders = new();

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

    public void AddTransporter(TransporterInfo transporter)
    {
        lock (_transportersLock)
        {
            _transporters.Add(transporter);
        }
    }

    public bool RemoveTransporter(TransporterInfo transporter)
    {
        lock (_transportersLock)
        {
            return _transporters.Remove(transporter);
        }
    }

    public TransporterInfo GetTransporter(int index)
    {
        lock (_transportersLock)
        {
            return _transporters[index];
        }
    }

    public int TransporterCount
    {
        get
        {
            lock (_transportersLock)
            {
                return _transporters.Count;
            }
        }
    }
}