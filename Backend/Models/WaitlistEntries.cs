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
    public enum Status { approved, Pending, rejected }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
