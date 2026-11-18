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
                User = new UserGetDTO
                {
                    Id = b.User.Id,
                    Username = b.User.Username,
                    Email = b.User.Email,
                    Role = b.User.role.ToString()
                },
                Room = new RoomGetDTO
                {
                    Id = b.Room.Id,
                    Name = b.Room.Name,
                    Capacity = b.Room.Capacity,
                    floor = b.Room.floor,
                    number = b.Room.number,
                    isActive = b.Room.isActive,
                    Category = new CategoryGetDTO
                    {
                        Id = b.Room.Category.Id,
                        Name = b.Room.Category.Name
                    },
                    RoomEquipment = b.Room.RoomEquipment
                    .Select(re => new RoomEquipmentWithoutRoomGetDTO
                    {
                        Id = re.Id,
                        Equipment = new EquipmentGetDTO
                        {
                            Id = re.Equipment.Id,
                            Name = re.Equipment.Name
                        },
                        Quantity = re.Quantity
                    })
                    .ToList()
                },
                BookingStart = b.BookingStart,
                BookingEnd = b.BookingEnd,
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
            .Include(r => r.User)
            .Include(r => r.Room)
            .Include(r => r.Room.Category)
            .Select(b => new BookingGetDTO
            {
                Id = b.Id,
                User = new UserGetDTO
                {
                    Id = b.User.Id,
                    Username = b.User.Username,
                    Email = b.User.Email,
                    Role = b.User.role.ToString()
                },
                Room = new RoomGetDTO
                {
                    Id = b.Room.Id,
                    Name = b.Room.Name,
                    Capacity = b.Room.Capacity,
                    floor = b.Room.floor,
                    number = b.Room.number,
                    isActive = b.Room.isActive,
                    Category = new CategoryGetDTO
                    {
                        Id = b.Room.Category.Id,
                        Name = b.Room.Category.Name
                    }
                },
                BookingStart = b.BookingStart,
                BookingEnd = b.BookingEnd,
                Comment = b.Comment,
                Status = b.Status,
                ParticipantCount = b.ParticipantCount,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            })
            .FirstOrDefaultAsync();
 
        if (booking == null)
            return NotFound();
 
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
 
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();
 
        if (booking.User.Id != userId &&
            !User.IsInRole("Teacher") &&
            !User.IsInRole("Admin"))
        {
            return Forbid();
        }
 
        return Ok(booking);
    }
 
    // POST: api/Bookings
    [Authorize (Roles = "Teacher,Admin")]
    [HttpPost("create")]
    public async Task<IActionResult> Create(BookingCreateDTO dto)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
 
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();
 
        var user = await _context.Users.FindAsync(userId);
 
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
            User = user,
            RoomId = dto.RoomId,
            Room = room,
            BookingStart = dto.BookingStart,
            BookingEnd = dto.BookingEnd,
            Comment = dto.Comment,
            Status = BookingStatus.Booked,
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
    [Authorize(Roles = "Teacher,Admin")]
    [HttpPut("update/{id}")]
    public async Task<IActionResult> Update(int id, BookingUpdateDTO dto)
    {
        var booking = await _context.Bookings.FindAsync(id);
 
        if (booking == null)
            return NotFound();
 
        booking.RoomId = dto.RoomId;
        booking.BookingStart = dto.BookingStart;
        booking.BookingEnd = dto.BookingEnd;
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
 
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
 
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();
 
        if (booking.User.Id != userId && !User.IsInRole("Admin"))
            return Forbid();
 
        _context.Bookings.Remove(booking);
        await _context.SaveChangesAsync();
 
        return NoContent();
    }
 
    // GET: api/bookings/CurrentUserBookings
    [Authorize]
    [HttpGet("CurrentUserBookings")]
    public async Task<IActionResult> CurrentUserBookings()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
    
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();
    
        var bookings = await _context.Bookings
            .AsNoTracking()
            .Where(b => b.UserId == userId)
            .Include(b => b.User)
            .Include(b => b.Room)
                .ThenInclude(r => r.Category)
            .OrderByDescending(b => b.BookingStart)
            .Select(b => new BookingGetDTO
            {
                Id = b.Id,
                User = new UserGetDTO
                {
                    Id = b.User.Id,
                    Username = b.User.Username,
                    Email = b.User.Email,
                    Role = b.User.role.ToString()
                },
                Room = new RoomGetDTO
                {
                    Id = b.Room.Id,
                    Name = b.Room.Name,
                    Capacity = b.Room.Capacity,
                    floor = b.Room.floor,
                    number = b.Room.number,
                    isActive = b.Room.isActive,
                    Category = new CategoryGetDTO
                    {
                        Id = b.Room.Category.Id,
                        Name = b.Room.Category.Name
                    }
                },
                BookingStart = b.BookingStart,
                BookingEnd = b.BookingEnd,
                Comment = b.Comment,
                Status = b.Status,
                ParticipantCount = b.ParticipantCount,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            })
            .ToListAsync();
    
        return Ok(bookings);
    }
}
 