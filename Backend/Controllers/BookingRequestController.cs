namespace MercBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingRequestsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;

    public BookingRequestsController(AppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
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

    [Authorize(Roles = "Teacher,Admin")]
    [HttpGet("getPendingRequests")]
    public async Task<IActionResult> GetPendingRequests()
    {
        var requests = await _context.BookingRequests
            .Where(r => r.Status == RequestStatus.Pending)
            .Include(r => r.User)
            .Include(r => r.Room)
            .Include(r => r.Room.Category)
            .Include(r => r.ReviewedBy)
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

    // GET: api/BookingRequests/getCurrentUserRequests
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

        await _emailService.SendEmailAsync(
            user.Email,
            "Booking anmodning oprettet - MercBooking",
            $"""
            <h2>Din booking anmodning er blevet oprettet!</h2>
            <p>Rum: {room.Name}</p>
            <p>Start: {TimeZoneInfo.ConvertTimeFromUtc(newRequest.BookedFrom, TimeZoneInfo.Local)}</p>
            <p>Slut: {TimeZoneInfo.ConvertTimeFromUtc(newRequest.BookedTo, TimeZoneInfo.Local)}</p>
            <p>Status: Afventer godkendelse</p>
            """
        );

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
        var request = await _context.BookingRequests
            .Include(r => r.User)
            .Include(r => r.Room)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
            return NotFound();
            
        DateTime oldStart = DateTime.MinValue;
        if (request.BookedFrom != dto.BookedFrom && dto.BookedFrom != DateTime.MinValue)
            oldStart = request.BookedFrom;

        DateTime oldEnd = DateTime.MinValue;
        if (request.BookedTo != dto.BookedTo && dto.BookedTo != DateTime.MinValue)
            oldEnd = request.BookedTo;

        request.RoomId = dto.RoomId;
        request.BookedFrom = dto.BookedFrom;
        request.BookedTo = dto.BookedTo;
        request.Comment = dto.Comment;
        request.Status = dto.Status;
        request.ParticipantCount = dto.ParticipantCount;
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        string startChanges = request.BookedFrom.ToString();

        if (oldStart != DateTime.MinValue)
        {
            startChanges = $"""
                <p>Old start time: {TimeZoneInfo.ConvertTimeFromUtc(oldStart, TimeZoneInfo.Local)}</p>
                <p>New start time: {TimeZoneInfo.ConvertTimeFromUtc(request.BookedFrom, TimeZoneInfo.Local)}</p>
                """;
        }

        string endChanges = request.BookedTo.ToString();

        if (oldEnd != DateTime.MinValue)
        {
            endChanges = $"""
                <p>Old end time: {TimeZoneInfo.ConvertTimeFromUtc(oldEnd, TimeZoneInfo.Local)}</p>
                <p>New end time: {TimeZoneInfo.ConvertTimeFromUtc(request.BookedTo, TimeZoneInfo.Local)}</p>
                """;
        }

        await _emailService.SendEmailAsync(
            request.User.Email,
            "Booking anmodning opdateret - MercBooking",
            $"""
            <h2>Din booking anmodning er blevet opdateret</h2>
            <p>Rum: {request.Room.Name}</p>
            <p>Start:</p>
            {startChanges}
            <p>Slut:</p>
            {endChanges}
            """
        );

        return NoContent();
    }

    [Authorize(Roles = "Teacher,Admin")]
    [HttpPut("review/{id}")]
    public async Task<IActionResult> Review(int id, BookingRequestReviewDTO dto)
    {
        var request = await _context.BookingRequests
            .Include(r => r.User)
            .Include(r => r.Room)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
            return NotFound();

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var reviewer = await _context.Users.FindAsync(userId);
        if (reviewer == null)
        {
            return BadRequest("Reviewer does not exist.");
        }

        request.Status = dto.Status;
        request.ReviewedBy = reviewer;
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        if (dto.Status == RequestStatus.Approved)
        {
            var newBooking = new Bookings
            {
                UserId = request.UserId,
                User = request.User,
                RoomId = request.RoomId,
                Room = request.Room,
                BookingStart = request.BookedFrom,
                BookingEnd = request.BookedTo,
                Comment = request.Comment,
                Status = BookingStatus.Booked,
                ParticipantCount = request.ParticipantCount,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Bookings.Add(newBooking);
            await _context.SaveChangesAsync();

            await _emailService.SendEmailAsync(
                request.User.Email,
                dto.Status == RequestStatus.Approved
                    ? "Booking godkendt - MercBooking"
                    : "Booking anmodning afvist - MercBooking",
                $"""
                <h2>
                    {(dto.Status == RequestStatus.Approved
                        ? "Din booking er blevet godkendt!"
                        : "Din booking anmodning er blevet afvist.")}
                </h2>
                <p>Rum: {request.Room.Name}</p>
                <p>Start: {TimeZoneInfo.ConvertTimeFromUtc(request.BookedFrom, TimeZoneInfo.Local)}</p>
                <p>Slut: {TimeZoneInfo.ConvertTimeFromUtc(request.BookedTo, TimeZoneInfo.Local)}</p>
                <p>Status: {request.Status}</p>
                """
            );
        }

        return NoContent();
    }

    // PUT: api/BookingRequests/cancelRequest/5
    [Authorize]
    [HttpPut("cancelRequest/{id}")]
    public async Task<IActionResult> CancelOwn(int id)
    {
        var request = await _context.BookingRequests
            .Include(r => r.User)
            .Include(r => r.Room)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
            return NotFound();

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        if (request.UserId != userId)
        {
            return Forbid("Du kan ikke annullere andre brugers booking requests.");
        }

        request.Status = RequestStatus.Cancelled;
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _emailService.SendEmailAsync(
            request.User.Email,
            "Booking anmodning annulleret - MercBooking",
            $"""
            <h2>Din booking anmodning er blevet annulleret</h2>
            <p>Rum: {request.Room.Name}</p>
            <p>Start: {TimeZoneInfo.ConvertTimeFromUtc(request.BookedFrom, TimeZoneInfo.Local)}</p>
            <p>Slut: {TimeZoneInfo.ConvertTimeFromUtc(request.BookedTo, TimeZoneInfo.Local)}</p>
            <p>Status: Annulleret</p>
            """
        );

        return Ok("Din booking anmodning er blevet annulleret.");
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
