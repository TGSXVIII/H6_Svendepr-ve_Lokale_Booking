namespace MercBooking.Api.Models;

public class WaitlistEntries
{
    public int Id { get; set; }
    public Users UserId { get; set; }
    public Rooms RoomId { get; set; }
    public DateTime RequestedStart { get; set; }
    public DateTime RequestedEnd { get; set; }
    public string Comment { get; set; }
    public int ParticipantCount { get; set; }
    public WaitlistStatus status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
public enum WaitlistStatus { approved, Pending, rejected }
