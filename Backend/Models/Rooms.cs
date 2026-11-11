namespace MercBooking.Api.Models;

public class Rooms
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string floor { get; set; }
    public string number { get; set; }
    public int Capacity { get; set; }
    public bool isActive { get; set; }
    public categories CategoryId { get; set; }

}
