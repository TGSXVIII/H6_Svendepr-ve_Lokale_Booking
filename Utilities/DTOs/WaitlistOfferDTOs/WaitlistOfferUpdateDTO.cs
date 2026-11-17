namespace MercBooking.Utilities.DTO;

public class WaitlistOfferUpdateDTO
{
    public int WaitlistEntryId { get; set; }
    public int BookingId { get; set; }
    public OfferStatus Status { get; set; }
}
