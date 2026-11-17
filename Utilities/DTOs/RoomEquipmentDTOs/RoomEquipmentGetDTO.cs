namespace MercBooking.Utilities.DTO;

public class RoomEquipmentGetDTO
{
    public int Id { get; set; }
    public RoomGetDTO Room { get; set; } = new RoomGetDTO();
    public EquipmentGetDTO Equipment { get; set; } = new EquipmentGetDTO();
    public int Quantity { get; set; }
}
