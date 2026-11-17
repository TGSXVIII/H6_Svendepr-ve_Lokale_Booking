namespace MercBooking.Utilities.DTO;

public class WaitlistOfferGetDTO
{
    public int Id { get; set; }
    public WaitlistEntryGetDTO WaitlistEntry { get; set; } = new WaitlistEntryGetDTO();
    public BookingGetDTO Booking { get; set; } = new BookingGetDTO();
    public OfferStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
