namespace MercBooking.Utilities.DTO;

public class BookingCreateDTO
{
    public int RoomId { get; set; }
    public DateTime BookingStart { get; set; }
    public DateTime BookingEnd { get; set; }
    public string Comment { get; set; } = string.Empty;
    public int ParticipantCount { get; set; }
}
