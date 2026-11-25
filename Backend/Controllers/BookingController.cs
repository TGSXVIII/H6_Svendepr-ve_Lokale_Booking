namespace MercBooking.Api.Controllers;
 
[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
 
    public BookingsController(AppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
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

        if (dto.BookingStart >= dto.BookingEnd)
        {
            return BadRequest("Booking start tid skal være før slut tid.");
        }

        if (dto.BookingStart < DateTime.UtcNow)
        {
            return BadRequest("Booking start tid skal være i fremtiden.");
        }

        if (await _context.Bookings.AnyAsync(b =>
            b.RoomId == dto.RoomId &&
            b.BookingStart < dto.BookingEnd &&
            b.BookingEnd > dto.BookingStart))
        {
            return BadRequest("The room is already booked during this period.");
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

        await _emailService.SendEmailAsync(
            user.Email,
            "Booking oprettet - MercBooking",
            $"""
            <h2>Din booking er blevet oprettet!</h2>
            <p>Rum: {newBooking.Room.Name}</p>
            <p>Start: {TimeZoneInfo.ConvertTimeFromUtc(newBooking.BookingStart, TimeZoneInfo.Local)}</p>
            <p>Slut: {TimeZoneInfo.ConvertTimeFromUtc(newBooking.BookingEnd, TimeZoneInfo.Local)}</p>
            """
        );
 
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
        
        DateTime oldStart = DateTime.MinValue;
        if(booking.BookingStart != dto.BookingStart && dto.BookingStart != DateTime.MinValue)
            oldStart = booking.BookingStart;

        DateTime oldEnd = DateTime.MinValue;
        if(booking.BookingEnd != dto.BookingEnd && dto.BookingEnd != DateTime.MinValue)
            oldEnd = booking.BookingEnd;

        booking.RoomId = dto.RoomId != 0 ? dto.RoomId : booking.RoomId;
        booking.BookingStart = dto.BookingStart != DateTime.MinValue ? dto.BookingStart : booking.BookingStart;
        booking.BookingEnd = dto.BookingEnd != DateTime.MinValue ? dto.BookingEnd : booking.BookingEnd;
        booking.Comment = dto.Comment;
        booking.Status = dto.Status;
        booking.ParticipantCount = dto.ParticipantCount;
        booking.UpdatedAt = DateTime.UtcNow;
 
        await _context.SaveChangesAsync();

        string startChanges = booking.BookingStart.ToString();

        if (oldStart != DateTime.MinValue)
        {
            startChanges += $"""
                <p>Old start time: {TimeZoneInfo.ConvertTimeFromUtc(oldStart, TimeZoneInfo.Local)}</p>
                <p>New start time: {TimeZoneInfo.ConvertTimeFromUtc(booking.BookingStart, TimeZoneInfo.Local)}</p>
                """;
        }

        string endChanges = booking.BookingEnd.ToString();
        if (oldEnd != DateTime.MinValue)
        {
            endChanges = $"""
                <p>Old end time: {TimeZoneInfo.ConvertTimeFromUtc(oldEnd, TimeZoneInfo.Local)}</p>
                <p>New end time: {TimeZoneInfo.ConvertTimeFromUtc(booking.BookingEnd, TimeZoneInfo.Local)}</p>
                """;
        }
    
        await _emailService.SendEmailAsync(
            booking.User.Email,
            "Booking Opdateret - MercBooking",
            $"""
            <h2>Din booking er blevet opdateret</h2>
            <p>Rum: {booking.Room.Name}</p>
            <p>Start: </p>
            <P>{startChanges} </P>
            <p>Slut: </p>
            <p>{endChanges} </P>
            """
        );
    
        return Ok("Booking updated.");
    }

    [Authorize]
    [HttpPut("Cancel/{id}")]
    public async Task<IActionResult> Cancel(int id)
    {
        var booking = await _context.Bookings.FindAsync(id);

        if (booking == null)
            return NotFound();

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        if (booking.UserId != userId && !User.IsInRole("Admin"))
            return Forbid();

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _emailService.SendEmailAsync(
            booking.User.Email,
            "Booking anulleret - MercBooking",
            $"""
            <h2>Din booking er blevet aflyst</h2>
            <p>Rum: {booking.Room.Name}</p>
            <p>Start: {TimeZoneInfo.ConvertTimeFromUtc(booking.BookingStart, TimeZoneInfo.Local)}</p>
            <p>Slut: {TimeZoneInfo.ConvertTimeFromUtc(booking.BookingEnd, TimeZoneInfo.Local)}</p>
            """
        );

        var waitlistEntry = await _context.WaitlistEntries
            .Where(e => e.RoomId == booking.RoomId &&
                        e.RequestedStart < booking.BookingEnd &&
                        e.RequestedEnd > booking.BookingStart &&
                        e.status == WaitlistStatus.Pending)
            .OrderBy(e => e.CreatedAt)
            .FirstOrDefaultAsync();

        if (waitlistEntry != null)
        {
            var newOffer = new WaitlistOffers
            {
                WaitlistEntryId = waitlistEntry.Id,
                BookingId = booking.Id,
                Status = OfferStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.WaitlistOffers.Add(newOffer);
            await _context.SaveChangesAsync();

            await _emailService.SendEmailAsync(
                waitlistEntry.User.Email,
                "Nyt venteliste offer - MercBooking",
                $"""
                <h2>Du har fået tilbudt en booking!</h2>
                <p>Et lokale er blevet ledigt, og du har fået tilbuddet.</p>
                <p>Rum: {booking.Room.Name}</p>
                <p>Start: {TimeZoneInfo.ConvertTimeFromUtc(booking.BookingStart, TimeZoneInfo.Local)}</p>
                <p>Slut: {TimeZoneInfo.ConvertTimeFromUtc(booking.BookingEnd, TimeZoneInfo.Local)}</p>
                <p>Log ind på MercBooking for at acceptere eller afvise tilbuddet.</p>
                """
            );
        }

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
 