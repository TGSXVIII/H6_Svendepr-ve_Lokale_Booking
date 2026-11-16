namespace MercBooking.Utilities.DTO;

public class RoomCreateDTO
{
    public string Name { get; set; }
    public string floor { get; set; }
    public string number { get; set; }
    public int Capacity { get; set; }
    public categories CategoryId { get; set; }
}
