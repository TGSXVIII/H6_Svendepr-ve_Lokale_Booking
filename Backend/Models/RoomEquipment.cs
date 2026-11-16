namespace MercBooking.Api.Models;

public class RoomEquipment
{
    public int Id { get; set; }

    public int RoomId { get; set; }
    public Rooms Room { get; set; } = null!;

    public int EquipmentId { get; set; }
    public Equipment Equipment { get; set; } = null!;

    public int Quantity { get; set; }
}
