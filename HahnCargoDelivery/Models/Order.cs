namespace HahnCargoDelivery.Models;

public class Order
{
    public int Id { get; set; }
    public int OriginNodeId { get; set; }
    public int TargetNodeId { get; set; }
    public int Load { get; set; }
    public int Value { get; set; }
    public DateTime DeliveryDate { get; set; }
    public DateTime ExpirationDate { get; set; }
}