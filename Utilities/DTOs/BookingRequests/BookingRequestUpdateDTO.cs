namespace MercBooking.Utilities.DTO;

public class BookingRequestUpdateDTO
{
    public int RoomId { get; set; }
    public DateTime BookedFrom { get; set; }
    public DateTime BookedTo { get; set; }
    public string Comment { get; set; } = string.Empty;
    public RequestStatus Status { get; set; }
    public int ParticipantCount { get; set; }
}
