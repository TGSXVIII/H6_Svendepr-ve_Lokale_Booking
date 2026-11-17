namespace MercBooking.Utilities.DTO;

public class BookingRequestGetDTO
{
    public UserGetDTO User { get; set; } = new UserGetDTO();

    public RoomGetDTO Room { get; set; } = new RoomGetDTO();

    public UserGetDTO? ReviewedBy { get; set; }

    public DateTime BookedFrom { get; set; }
    public DateTime BookedTo { get; set; }
    public string Comment { get; set; } = string.Empty;
    public RequestStatus Status { get; set; }
    public int ParticipantCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
