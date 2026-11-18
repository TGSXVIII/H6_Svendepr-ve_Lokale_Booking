namespace MercBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WaitlistEntriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public WaitlistEntriesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/WaitlistEntries
    [Authorize(Roles = "Teacher,Admin")]
    [HttpGet("getAll")]
    public async Task<IActionResult> GetAll()
    {
        var entries = await _context.WaitlistEntries
            .Include(w => w.User)
            .Include(w => w.Room)
            .Include(w => w.Room.Category)
            .Select(w => new WaitlistEntryGetDTO
            {
                Id = w.Id,
                User = new UserGetDTO
                {
                    Id = w.User.Id,
                    Username = w.User.Username,
                    Email = w.User.Email,
                    Role = w.User.role.ToString()
                },
                Room = new RoomGetDTO
                {
                    Id = w.Room.Id,
                    Name = w.Room.Name,
                    Capacity = w.Room.Capacity,
                    floor = w.Room.floor,
                    number = w.Room.number,
                    isActive = w.Room.isActive,
                    Category = new CategoryGetDTO
                    {
                        Id = w.Room.Category.Id,
                        Name = w.Room.Category.Name
                    }
                },
                RequestedStart = w.RequestedStart,
                RequestedEnd = w.RequestedEnd,
                Comment = w.Comment,
                ParticipantCount = w.ParticipantCount,
                Status = w.status,
                CreatedAt = w.CreatedAt,
                UpdatedAt = w.UpdatedAt
            })
            .ToListAsync();

        return Ok(entries);
    }

    // GET: api/WaitlistEntries/5
    [Authorize(Roles = "Teacher,Admin")]
    [HttpGet("getById/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var entry = await _context.WaitlistEntries
            .Where(w => w.Id == id)
            .Include(w => w.User)
            .Include(w => w.Room)
            .Include(w => w.Room.Category)
            .Select(w => new WaitlistEntryGetDTO
            {
                Id = w.Id,
                User = new UserGetDTO
                {
                    Id = w.User.Id,
                    Username = w.User.Username,
                    Email = w.User.Email,
                    Role = w.User.role.ToString()
                },
                Room = new RoomGetDTO
                {
                    Id = w.Room.Id,
                    Name = w.Room.Name,
                    Capacity = w.Room.Capacity,
                    floor = w.Room.floor,
                    number = w.Room.number,
                    isActive = w.Room.isActive,
                    Category = new CategoryGetDTO
                    {
                        Id = w.Room.Category.Id,
                        Name = w.Room.Category.Name
                    }
                },

                RequestedStart = w.RequestedStart,
                RequestedEnd = w.RequestedEnd,
                Comment = w.Comment,
                ParticipantCount = w.ParticipantCount,
                Status = w.status,
                CreatedAt = w.CreatedAt,
                UpdatedAt = w.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (entry == null)
            return NotFound();

        return Ok(entry);
    }

    // POST: api/WaitlistEntries
    [Authorize(Roles = "Teacher,Admin")]
    [HttpPost("create")]
    public async Task<IActionResult> Create(WaitlistEntryCreateDTO dto)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return BadRequest("User does not exist.");

        var room = await _context.Rooms.FindAsync(dto.RoomId);
        if (room == null)
            return BadRequest("Room does not exist.");

        var newEntry = new WaitlistEntries
        {
            UserId = userId,
            User = user,
            RoomId = dto.RoomId,
            Room = room,
            RequestedStart = dto.RequestedStart,
            RequestedEnd = dto.RequestedEnd,
            Comment = dto.Comment,
            ParticipantCount = dto.ParticipantCount,
            status = WaitlistStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.WaitlistEntries.Add(newEntry);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = newEntry.Id },
            newEntry
        );
    }

    // PUT: api/WaitlistEntries/update/5
    [Authorize(Roles = "Teacher,Admin")]
    [HttpPut("update/{id}")]
    public async Task<IActionResult> Update(int id, WaitlistEntryUpdateDTO dto)
    {
        var entry = await _context.WaitlistEntries.FindAsync(id);

        if (entry == null)
            return NotFound();

        var room = await _context.Rooms.FindAsync(dto.RoomId);
        if (room == null)
            return BadRequest("Room does not exist.");
        
        int actualUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        string userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)!.Value;

        if (entry.UserId != actualUserId && userRole != "admin")
        {
            return Forbid("You are not authorized to update this waitlist entry.");
        }

        entry.RoomId = dto.RoomId;
        entry.Room = room;
        entry.RequestedStart = dto.RequestedStart;
        entry.RequestedEnd = dto.RequestedEnd;
        entry.Comment = dto.Comment;
        entry.ParticipantCount = dto.ParticipantCount;
        entry.status = dto.Status;
        entry.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [Authorize(Roles = "Teacher,Admin")]
    [HttpPut("Cancel/{id}")]
    public async Task<IActionResult> Cancel(int id)
    {
        var entry = await _context.WaitlistEntries.FindAsync(id);

        if (entry == null)
            return NotFound();

        entry.status = WaitlistStatus.Cancelled;
        entry.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/WaitlistEntries/delete/5
    [Authorize(Roles = "Admin")]
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entry = await _context.WaitlistEntries.FindAsync(id);

        if (entry == null)
            return NotFound();

        _context.WaitlistEntries.Remove(entry);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
