namespace HahnCargoDelivery.Models;

public class TransporterInfo
{
    public int Id { get; set; }
    public List<Order> Orders { get; set; } = new List<Order>();
    public Queue<int> PathRemained { get; set; }
}