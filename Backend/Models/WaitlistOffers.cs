namespace MercBooking.Api.Models;

public class WaitlistOffers
{
    public int Id { get; set; }
    
    public int WaitlistEntryId { get; set; }
    public WaitlistEntries WaitlistEntry { get; set; } = new WaitlistEntries();

    public int BookingId { get; set; }
    public Bookings Booking { get; set; } = new Bookings();

    public OfferStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
public enum OfferStatus { approved, Pending, rejected }
