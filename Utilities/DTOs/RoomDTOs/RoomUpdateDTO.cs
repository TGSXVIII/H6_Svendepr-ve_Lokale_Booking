namespace MercBooking.Utilities.DTO;

public class RoomUpdateDTO
{
    public string Name { get; set; } = string.Empty;
    public string floor { get; set; } = string.Empty;
    public string number { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public bool isActive { get; set; } = true;
    public int CategoryId { get; set; }
}
