namespace MercBooking.Api.Models;

public class Users
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public enum Role { Student, Teacher, Admin }
    public Role role { get; set; }
}
