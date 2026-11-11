namespace MercBooking.Api.Models;

public class BookingRequests
{
    public int Id { get; set; }
    public Users UserId { get; set; }
    public Rooms roomId { get; set; }
    public Users ReviewedById { get; set; }
    public DateTime BookedFrom { get; set; }
    public DateTime BookedTo { get; set; }
    public string Comment { get; set; }
    public RequestStatus Status { get; set; }
    public int ParticipantCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
public enum RequestStatus { approved, Pending, rejected }
