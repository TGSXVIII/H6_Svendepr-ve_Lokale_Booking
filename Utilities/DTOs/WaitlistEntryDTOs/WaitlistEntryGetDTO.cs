namespace MercBooking.Utilities.DTO;

public class WaitlistEntryGetDTO
{
    public int Id { get; set; }

    public UserGetDTO User { get; set; } = new UserGetDTO();

    public RoomGetDTO Room { get; set; } = new RoomGetDTO();

    public DateTime RequestedStart { get; set; }
    public DateTime RequestedEnd { get; set; }
    public string Comment { get; set; } = string.Empty;
    public int ParticipantCount { get; set; }
    public WaitlistStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
