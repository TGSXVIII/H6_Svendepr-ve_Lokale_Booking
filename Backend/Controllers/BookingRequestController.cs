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
    [HttpGet("getAll")]
    public async Task<IActionResult> GetAll()
    {
        var requests = await _context.BookingRequests
            .Select(r => new BookingRequestGetDTO
            {
                Id = r.Id,
                UserId = r.UserId,
                Username = r.User.Username,
                RoomId = r.RoomId,
                RoomName = r.Room.Name,
                ReviewedById = r.ReviewedById,
                ReviewedByUsername = r.ReviewedBy != null
                    ? r.ReviewedBy.Username
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
    [HttpGet("getById/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var request = await _context.BookingRequests
            .Where(r => r.Id == id)
            .Select(r => new BookingRequestGetDTO
            {
                Id = r.Id,
                UserId = r.UserId,
                Username = r.User.Username,
                RoomId = r.RoomId,
                RoomName = r.Room.Name,
                ReviewedById = r.ReviewedById,
                ReviewedByUsername = r.ReviewedBy != null
                    ? r.ReviewedBy.Username
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
    [HttpPost("create")]
    public async Task<IActionResult> Create(BookingRequestCreateDTO dto)
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

        var newRequest = new BookingRequests
        {
            UserId = dto.UserId,
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
