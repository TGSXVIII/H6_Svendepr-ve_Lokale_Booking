namespace MercBooking.Utilities.DTO;

public class WaitlistEntryCreateDTO
{
    public int UserId { get; set; }
    public int RoomId { get; set; }
    public DateTime RequestedStart { get; set; }
    public DateTime RequestedEnd { get; set; }
    public string Comment { get; set; } = string.Empty;
    public int ParticipantCount { get; set; }
}
