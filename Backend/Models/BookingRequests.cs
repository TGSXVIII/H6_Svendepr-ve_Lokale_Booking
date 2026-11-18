namespace MercBooking.Api.Models;

public class BookingRequests
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public Users User { get; set; } = new Users();

    public int RoomId { get; set; }
    public Rooms Room { get; set; } = new Rooms();

    public int? ReviewedById { get; set; }
    public Users? ReviewedBy { get; set; }

    public DateTime BookedFrom { get; set; }
    public DateTime BookedTo { get; set; }
    public string Comment { get; set; } = string.Empty;
    public RequestStatus Status { get; set; }
    public int ParticipantCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
public enum RequestStatus { Approved, Pending, Rejected, Cancelled }
