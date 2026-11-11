namespace MercBooking.Api.Models;

public class WaitlistOffers
{
    public int Id { get; set; }
    public WaitlistEntries WaitlistEntryId { get; set; }
    public Bookings BookingId { get; set; }
    public enum Status { approved, Pending, rejected }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
