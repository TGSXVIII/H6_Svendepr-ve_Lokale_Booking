namespace MercBooking.Utilities.DTO;

public class BookingRequestReviewDTO
{
    public RequestStatus Status { get; set; }
    public UserGetDTO ReviewedBy { get; set; } = new UserGetDTO();
}
