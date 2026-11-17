namespace MercBooking.Utilities.DTO;

public class WaitlistOfferGetDTO
{
    public int Id { get; set; }
    public int WaitlistEntryId { get; set; }
    public int BookingId { get; set; }
    public OfferStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
