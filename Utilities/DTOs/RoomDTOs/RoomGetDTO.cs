namespace MercBooking.Utilities.DTO;

public class RoomGetDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string floor { get; set; } = string.Empty;
    public string number { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public bool isActive { get; set; } = true;
    public CategoryGetDTO Category { get; set; } = new CategoryGetDTO();
    public ICollection<RoomEquipmentWithoutRoomGetDTO> RoomEquipment { get; set; } = new List<RoomEquipmentWithoutRoomGetDTO>();
}
