namespace MercBooking.Utilities.DTO;

public class BookingRequestGetDTO
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;

    public int RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;

    public int? ReviewedById { get; set; }
    public string? ReviewedByUsername { get; set; }

    public DateTime BookedFrom { get; set; }
    public DateTime BookedTo { get; set; }
    public string Comment { get; set; } = string.Empty;
    public RequestStatus Status { get; set; }
    public int ParticipantCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
