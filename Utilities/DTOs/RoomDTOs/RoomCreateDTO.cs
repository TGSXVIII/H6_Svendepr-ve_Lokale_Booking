namespace MercBooking.Utilities.DTO;

public class RoomCreateDTO
{
    public string Name { get; set; } = string.Empty;
    public string floor { get; set; } = string.Empty;
    public string number { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public Categories CategoryId { get; set; } = new Categories();
}
