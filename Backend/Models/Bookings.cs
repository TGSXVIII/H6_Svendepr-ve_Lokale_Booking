namespace MercBooking.Api.Models;

public class Bookings
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public Users User { get; set; } = new Users();

    public int RoomId { get; set; }
    public Rooms Room { get; set; } = new Rooms();
    
    public DateTime RequestedStart { get; set; }
    public DateTime RequestedEnd { get; set; }
    public string Comment { get; set; } = string.Empty;
    public BookingStatus Status { get; set; }
    public int ParticipantCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
public enum BookingStatus { booked, Pending }
