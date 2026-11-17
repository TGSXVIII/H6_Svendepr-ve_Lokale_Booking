namespace MercBooking.Utilities.DTO;

public class BookingUpdateDTO
{
    public int RoomId { get; set; }
    public DateTime RequestedStart { get; set; }
    public DateTime RequestedEnd { get; set; }
    public string Comment { get; set; } = string.Empty;
    public BookingStatus Status { get; set; }
    public int ParticipantCount { get; set; }
}
