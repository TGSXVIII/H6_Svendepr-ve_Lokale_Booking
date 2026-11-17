namespace MercBooking.Api.Models;

public class Users
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Role role { get; set; }
}
public enum Role { Student, Teacher, Admin }
