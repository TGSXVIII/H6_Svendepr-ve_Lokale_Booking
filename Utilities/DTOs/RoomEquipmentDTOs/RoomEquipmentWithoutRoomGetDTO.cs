namespace MercBooking.Utilities.DTO;

public class RoomEquipmentWithoutRoomGetDTO
{
    public int Id { get; set; }
    public EquipmentGetDTO Equipment { get; set; } = new EquipmentGetDTO();
    public int Quantity { get; set; }
}
