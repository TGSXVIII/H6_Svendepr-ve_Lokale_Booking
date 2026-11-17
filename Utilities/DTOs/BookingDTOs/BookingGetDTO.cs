namespace MercBooking.Utilities.DTO;

public class BookingGetDTO
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;

    public int RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;

    public DateTime RequestedStart { get; set; }
    public DateTime RequestedEnd { get; set; }
    public string Comment { get; set; } = string.Empty;
    public BookingStatus Status { get; set; }
    public int ParticipantCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
