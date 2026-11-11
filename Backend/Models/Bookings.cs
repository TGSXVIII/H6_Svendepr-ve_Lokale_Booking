namespace MercBooking.Api.Models;

public class Bookings
{
    public int Id { get; set; }
    public Users UserId { get; set; }
    public Rooms RoomId { get; set; }
    public DateTime RequestedStart { get; set; }
    public DateTime RequestedEnd { get; set; }
    public string Comment { get; set; }
    public int ParticipantCount { get; set; }
    public enum Status { booked, Pending }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
