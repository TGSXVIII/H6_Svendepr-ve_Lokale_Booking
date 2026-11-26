namespace MercBooking.Api.Models;

public class Rooms
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string floor { get; set; } = string.Empty;
    public string number { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public bool isActive { get; set; } = true;

    public int? LastEditedById { get; set; }
    public Users? LastEditedBy { get; set; }

    public int CategoryId { get; set; }
    public Categories Category { get; set; } = new Categories();
    
    public ICollection<RoomEquipment> RoomEquipment { get; set; } = new List<RoomEquipment>();
}
