namespace HahnCargoDelivery.Models;

public class TransporterInfo
{
    public int Id { get; set; }
    public List<int> Orders { get; set; }
    public List<int> PathRemained { get; set; }
}