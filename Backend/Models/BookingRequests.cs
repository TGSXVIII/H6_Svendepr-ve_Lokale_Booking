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
    public int ParticipantCount { get; set; }
    public enum Status { approved, Pending, rejected }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
