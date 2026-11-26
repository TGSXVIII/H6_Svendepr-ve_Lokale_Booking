namespace MercBooking.Utilities.DTO;

public class BookingUpdateDTO
{
    public int RoomId { get; set; } = 0;
    public DateTime BookingStart { get; set; } = DateTime.MinValue;
    public DateTime BookingEnd { get; set; } = DateTime.MinValue;
    public string Comment { get; set; } = string.Empty;
    public BookingStatus Status { get; set; }
    public int ParticipantCount { get; set; }
}
