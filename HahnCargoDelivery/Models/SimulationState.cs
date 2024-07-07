namespace HahnCargoDelivery.Models;

public class SimulationState
{
    public Boolean IsSimulationStarted { get; set; } = false;
    public Grid Grid { get; set; }
    
    public List<TransporterInfo> Transporters { get; set; }
}