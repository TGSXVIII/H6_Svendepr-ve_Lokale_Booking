namespace MercBooking.Api.Models;

public class RoomEquipment
{
    public int Id { get; set; }
    public Rooms RoomId { get; set; }
    public equipment EquipmentId { get; set; }
    public int Quantity { get; set; }
}
