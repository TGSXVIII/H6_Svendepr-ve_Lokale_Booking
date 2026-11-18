namespace MercBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingRequestsController : ControllerBase
{
    private readonly AppDbContext _context;

    public BookingRequestsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/BookingRequests
    [Authorize(Roles = "Teacher,Admin")]
    [HttpGet("getAll")]
    public async Task<IActionResult> GetAll()
    {
        var requests = await _context.BookingRequests
            .Include(r => r.User)
            .Include(r => r.Room)
            .Include(r => r.Room.Category)
            .Include(r => r.ReviewedBy)
            .Select(r => new BookingRequestGetDTO
            {
                User = new UserGetDTO
                {
                    Id = r.User.Id,
                    Username = r.User.Username,
                    Email = r.User.Email,
                    Role = r.User.role.ToString()
                },
                Room = new RoomGetDTO
                {
                    Id = r.Room.Id,
                    Name = r.Room.Name,
                    Capacity = r.Room.Capacity,
                    floor = r.Room.floor,
                    number = r.Room.number,
                    isActive = r.Room.isActive,
                    Category = new CategoryGetDTO
                    {
                        Id = r.Room.Category.Id,
                        Name = r.Room.Category.Name
                    }
                },
                ReviewedBy = r.ReviewedBy != null
                    ? new UserGetDTO
                    {
                        Id = r.ReviewedBy.Id,
                        Username = r.ReviewedBy.Username,
                        Email = r.ReviewedBy.Email,
                        Role = r.ReviewedBy.role.ToString()
                    }
                    : null,
                BookedFrom = r.BookedFrom,
                BookedTo = r.BookedTo,
                Comment = r.Comment,
                Status = r.Status,
                ParticipantCount = r.ParticipantCount,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            })
            .ToListAsync();

        return Ok(requests);
    }

    [Authorize]
    [HttpGet("getCurrentUserRequests")]
    public async Task<IActionResult> GetCurrentUserRequests()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var requests = await _context.BookingRequests
            .Where(r => r.UserId == userId)
            .Include(r => r.Room)
                .ThenInclude(r => r.Category)
            .Include(r => r.ReviewedBy)
            .OrderByDescending(r => r.BookedFrom)
            .Select(r => new BookingRequestGetDTO
            {
                Id = r.Id,
                User = new UserGetDTO
                {
                    Id = r.User.Id,
                    Username = r.User.Username,
                    Email = r.User.Email,
                    Role = r.User.role.ToString()
                },
                Room = new RoomGetDTO
                {
                    Id = r.Room.Id,
                    Name = r.Room.Name,
                    Capacity = r.Room.Capacity,
                    floor = r.Room.floor,
                    number = r.Room.number,
                    isActive = r.Room.isActive,
                    Category = new CategoryGetDTO
                    {
                        Id = r.Room.Category.Id,
                        Name = r.Room.Category.Name
                    }
                },
                ReviewedBy = r.ReviewedBy != null
                    ? new UserGetDTO
                    {
                        Id = r.ReviewedBy.Id,
                        Username = r.ReviewedBy.Username,
                        Email = r.ReviewedBy.Email,
                        Role = r.ReviewedBy.role.ToString()
                    }
                    : null,
                BookedFrom = r.BookedFrom,
                BookedTo = r.BookedTo,
                Comment = r.Comment,
                Status = r.Status,
                ParticipantCount = r.ParticipantCount,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            })
            .ToListAsync();

        return Ok(requests);
    }

    // GET: api/BookingRequests/5
    [Authorize(Roles = "Teacher,Admin")]
    [HttpGet("getById/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var request = await _context.BookingRequests
            .Where(r => r.Id == id)
            .Include(r => r.User)
            .Include(r => r.Room)
            .Include(r => r.Room.Category)
            .Include(r => r.ReviewedBy)
            .Select(r => new BookingRequestGetDTO
            {
                User = new UserGetDTO
                {
                    Id = r.User.Id,
                    Username = r.User.Username,
                    Email = r.User.Email,
                    Role = r.User.role.ToString()
                },
                Room = new RoomGetDTO
                {
                    Id = r.Room.Id,
                    Name = r.Room.Name,
                    Capacity = r.Room.Capacity,
                    floor = r.Room.floor,
                    number = r.Room.number,
                    isActive = r.Room.isActive,
                    Category = new CategoryGetDTO
                    {
                        Id = r.Room.Category.Id,
                        Name = r.Room.Category.Name
                    }
                },
                ReviewedBy = r.ReviewedBy != null
                    ? new UserGetDTO
                    {
                        Id = r.ReviewedBy.Id,
                        Username = r.ReviewedBy.Username,
                        Email = r.ReviewedBy.Email,
                        Role = r.ReviewedBy.role.ToString()
                    }
                    : null,
                BookedFrom = r.BookedFrom,
                BookedTo = r.BookedTo,
                Comment = r.Comment,
                Status = r.Status,
                ParticipantCount = r.ParticipantCount,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (request == null)
            return NotFound();

        return Ok(request);
    }

    // POST: api/BookingRequests
    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> Create(BookingRequestCreateDTO dto)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

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

        var newRequest = new BookingRequests
        {
            UserId = userId,
            User = user,
            RoomId = dto.RoomId,
            Room = room,
            BookedFrom = dto.BookedFrom,
            BookedTo = dto.BookedTo,
            Comment = dto.Comment,
            Status = RequestStatus.Pending,
            ParticipantCount = dto.ParticipantCount,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.BookingRequests.Add(newRequest);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = newRequest.Id },
            newRequest
        );
    }

    // PUT: api/BookingRequests/update/5
    [Authorize(Roles = "Teacher,Admin")]
    [HttpPut("update/{id}")]
    public async Task<IActionResult> Update(int id, BookingRequestUpdateDTO dto)
    {
        var request = await _context.BookingRequests.FindAsync(id);

        if (request == null)
            return NotFound();

        request.RoomId = dto.RoomId;
        request.BookedFrom = dto.BookedFrom;
        request.BookedTo = dto.BookedTo;
        request.Comment = dto.Comment;
        request.Status = dto.Status;
        request.ParticipantCount = dto.ParticipantCount;
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/BookingRequests/delete/5
    [Authorize(Roles = "Teacher,Admin")]
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var request = await _context.BookingRequests.FindAsync(id);

        if (request == null)
            return NotFound();

        _context.BookingRequests.Remove(request);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
