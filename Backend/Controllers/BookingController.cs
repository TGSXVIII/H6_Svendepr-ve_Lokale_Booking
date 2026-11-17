namespace MercBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly AppDbContext _context;

    public BookingsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Bookings
    [Authorize]
    [HttpGet("getAll")]
    public async Task<IActionResult> GetAll()
    {
        var bookings = await _context.Bookings
            .Select(b => new BookingGetDTO
            {
                Id = b.Id,
                UserId = b.UserId,
                Username = b.User.Username,
                RoomId = b.RoomId,
                RoomName = b.Room.Name,
                RequestedStart = b.RequestedStart,
                RequestedEnd = b.RequestedEnd,
                Comment = b.Comment,
                Status = b.Status,
                ParticipantCount = b.ParticipantCount,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            })
            .ToListAsync();

        return Ok(bookings);
    }

    // GET: api/Bookings/5
    [Authorize]
    [HttpGet("getById/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var booking = await _context.Bookings
            .Where(b => b.Id == id)
            .Select(b => new BookingGetDTO
            {
                Id = b.Id,
                UserId = b.UserId,
                Username = b.User.Username,
                RoomId = b.RoomId,
                RoomName = b.Room.Name,
                RequestedStart = b.RequestedStart,
                RequestedEnd = b.RequestedEnd,
                Comment = b.Comment,
                Status = b.Status,
                ParticipantCount = b.ParticipantCount,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (booking == null)
            return NotFound();

        return Ok(booking);
    }

    // POST: api/Bookings
    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> Create(BookingCreateDTO dto)
    {
        var user = await _context.Users.FindAsync(dto.UserId);
        if (user == null)
        {
            return BadRequest("User does not exist.");
        }

        var room = await _context.Rooms.FindAsync(dto.RoomId);
        if (room == null)
        {
            return BadRequest("Room does not exist.");
        }

        var newBooking = new Bookings
        {
            UserId = dto.UserId,
            User = user,
            RoomId = dto.RoomId,
            Room = room,
            RequestedStart = dto.RequestedStart,
            RequestedEnd = dto.RequestedEnd,
            Comment = dto.Comment,
            Status = BookingStatus.Pending,
            ParticipantCount = dto.ParticipantCount,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(newBooking);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = newBooking.Id },
            newBooking
        );
    }

    // PUT: api/Bookings/5
    [Authorize]
    [HttpPut("update/{id}")]
    public async Task<IActionResult> Update(int id, BookingUpdateDTO dto)
    {
        var booking = await _context.Bookings.FindAsync(id);

        if (booking == null)
            return NotFound();

        booking.RoomId = dto.RoomId;
        booking.RequestedStart = dto.RequestedStart;
        booking.RequestedEnd = dto.RequestedEnd;
        booking.Comment = dto.Comment;
        booking.Status = dto.Status;
        booking.ParticipantCount = dto.ParticipantCount;
        booking.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Bookings/5
    [Authorize]
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var booking = await _context.Bookings.FindAsync(id);

        if (booking == null)
            return NotFound();

        _context.Bookings.Remove(booking);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
