namespace MercBooking.Utilities.DTO;

public class BookingGetDTO
{
    public int Id { get; set; }

    public UserGetDTO User { get; set; } = new UserGetDTO();

    public RoomGetDTO Room { get; set; } = new RoomGetDTO();

    public DateTime BookingStart { get; set; }
    public DateTime BookingEnd { get; set; }
    public string Comment { get; set; } = string.Empty;
    public BookingStatus Status { get; set; }
    public int ParticipantCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
