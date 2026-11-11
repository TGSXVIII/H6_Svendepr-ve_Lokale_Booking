namespace MercBooking.Api.Models;

public class WaitlistOffers
{
    public int Id { get; set; }
    public WaitlistEntries WaitlistEntryId { get; set; }
    public Bookings BookingId { get; set; }
    public OfferStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
public enum OfferStatus { approved, Pending, rejected }
