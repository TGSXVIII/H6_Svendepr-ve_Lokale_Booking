namespace MercBooking.Utilities.DTO;

public class BookingRequestUpdateDTO
{
    public int RoomId { get; set; } = 0;
    public DateTime BookedFrom { get; set; } = DateTime.MinValue;
    public DateTime BookedTo { get; set; } = DateTime.MinValue;
    public string Comment { get; set; } = string.Empty;
    public RequestStatus Status { get; set; }
    public int ParticipantCount { get; set; } = 0;
}
