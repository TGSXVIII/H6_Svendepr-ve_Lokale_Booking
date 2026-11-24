namespace MercBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WaitlistOffersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;

    public WaitlistOffersController(AppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    [Authorize(Roles = "Teacher,Admin")]
    [HttpGet("getAll")]
    public async Task<IActionResult> GetAll()
    {
        var offers = await _context.WaitlistOffers
            .Include(o => o.WaitlistEntry)
                .ThenInclude(w => w.User)
            .Include(o => o.WaitlistEntry)
                .ThenInclude(w => w.Room)
                    .ThenInclude(r => r.Category)
            .Include(o => o.Booking)
                .ThenInclude(b => b.User)
            .Include(o => o.Booking)
                .ThenInclude(b => b.Room)
                    .ThenInclude(r => r.Category)
            .Select(o => new WaitlistOfferGetDTO
            {
                Id = o.Id,
                WaitlistEntry = new WaitlistEntryGetDTO
                {
                    Id = o.WaitlistEntry.Id,
                    User = new UserGetDTO
                    {
                        Id = o.WaitlistEntry.User.Id,
                        Username = o.WaitlistEntry.User.Username,
                        Email = o.WaitlistEntry.User.Email,
                        Role = o.WaitlistEntry.User.role.ToString()
                    },
                    Room = new RoomGetDTO
                    {
                        Id = o.WaitlistEntry.Room.Id,
                        Name = o.WaitlistEntry.Room.Name,
                        Capacity = o.WaitlistEntry.Room.Capacity,
                        floor = o.WaitlistEntry.Room.floor,
                        number = o.WaitlistEntry.Room.number,
                        isActive = o.WaitlistEntry.Room.isActive,
                        Category = new CategoryGetDTO
                        {
                            Id = o.WaitlistEntry.Room.Category.Id,
                            Name = o.WaitlistEntry.Room.Category.Name
                        }
                    },
                    RequestedStart = o.WaitlistEntry.RequestedStart,
                    RequestedEnd = o.WaitlistEntry.RequestedEnd,
                    Comment = o.WaitlistEntry.Comment,
                    ParticipantCount = o.WaitlistEntry.ParticipantCount,
                    Status = o.WaitlistEntry.status,
                    CreatedAt = o.WaitlistEntry.CreatedAt,
                    UpdatedAt = o.WaitlistEntry.UpdatedAt
                },
                Booking = new BookingGetDTO
                {
                    Id = o.Booking.Id,
                    User = new UserGetDTO
                    {
                        Id = o.Booking.User.Id,
                        Username = o.Booking.User.Username,
                        Email = o.Booking.User.Email,
                        Role = o.Booking.User.role.ToString()
                    },
                    Room = new RoomGetDTO
                    {
                        Id = o.Booking.Room.Id,
                        Name = o.Booking.Room.Name,
                        Capacity = o.Booking.Room.Capacity,
                        floor = o.Booking.Room.floor,
                        number = o.Booking.Room.number,
                        isActive = o.Booking.Room.isActive,
                        Category = new CategoryGetDTO
                        {
                            Id = o.Booking.Room.Category.Id,
                            Name = o.Booking.Room.Category.Name
                        }
                    },
                    BookingStart = o.Booking.BookingStart,
                    BookingEnd = o.Booking.BookingEnd,
                    Comment = o.Booking.Comment,
                    Status = o.Booking.Status,
                    ParticipantCount = o.Booking.ParticipantCount,
                    CreatedAt = o.Booking.CreatedAt,
                    UpdatedAt = o.Booking.UpdatedAt
                },
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt
            })
            .ToListAsync();

        return Ok(offers);
    }

    [Authorize(Roles = "Teacher,Admin")]
    [HttpGet("getById/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var offer = await _context.WaitlistOffers
            .Where(o => o.Id == id)
            .Include(o => o.WaitlistEntry)
                .ThenInclude(w => w.User)
            .Include(o => o.WaitlistEntry)
                .ThenInclude(w => w.Room)
                    .ThenInclude(r => r.Category)
            .Include(o => o.Booking)
                .ThenInclude(b => b.User)
            .Include(o => o.Booking)
                .ThenInclude(b => b.Room)
                    .ThenInclude(r => r.Category)
            .Select(o => new WaitlistOfferGetDTO
            {
                Id = o.Id,
                WaitlistEntry = new WaitlistEntryGetDTO
                {
                    Id = o.WaitlistEntry.Id,
                    User = new UserGetDTO
                    {
                        Id = o.WaitlistEntry.User.Id,
                        Username = o.WaitlistEntry.User.Username,
                        Email = o.WaitlistEntry.User.Email,
                        Role = o.WaitlistEntry.User.role.ToString()
                    },
                    Room = new RoomGetDTO
                    {
                        Id = o.WaitlistEntry.Room.Id,
                        Name = o.WaitlistEntry.Room.Name,
                        Capacity = o.WaitlistEntry.Room.Capacity,
                        floor = o.WaitlistEntry.Room.floor,
                        number = o.WaitlistEntry.Room.number,
                        isActive = o.WaitlistEntry.Room.isActive,
                        Category = new CategoryGetDTO
                        {
                            Id = o.WaitlistEntry.Room.Category.Id,
                            Name = o.WaitlistEntry.Room.Category.Name
                        }
                    },
                    
                    RequestedStart = o.WaitlistEntry.RequestedStart,
                    RequestedEnd = o.WaitlistEntry.RequestedEnd,
                    Comment = o.WaitlistEntry.Comment,
                    ParticipantCount = o.WaitlistEntry.ParticipantCount,
                    Status = o.WaitlistEntry.status,
                    CreatedAt = o.WaitlistEntry.CreatedAt,
                    UpdatedAt = o.WaitlistEntry.UpdatedAt
                },
                Booking = new BookingGetDTO
                {
                    Id = o.Booking.Id,
                    User = new UserGetDTO
                    {
                        Id = o.Booking.User.Id,
                        Username = o.Booking.User.Username,
                        Email = o.Booking.User.Email,
                        Role = o.Booking.User.role.ToString()
                    },
                    Room = new RoomGetDTO
                    {
                        Id = o.Booking.Room.Id,
                        Name = o.Booking.Room.Name,
                        Capacity = o.Booking.Room.Capacity,
                        floor = o.Booking.Room.floor,
                        number = o.Booking.Room.number,
                        isActive = o.Booking.Room.isActive,
                        Category = new CategoryGetDTO
                        {
                            Id = o.Booking.Room.Category.Id,
                            Name = o.Booking.Room.Category.Name
                        }
                    },
                    BookingStart = o.Booking.BookingStart,
                    BookingEnd = o.Booking.BookingEnd,
                    Comment = o.Booking.Comment,
                },
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (offer == null)
            return NotFound();

        return Ok(offer);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("create")]
    public async Task<IActionResult> Create(WaitlistOfferCreateDTO dto)
    {
        var waitlistEntry = await _context.WaitlistEntries
            .Include(w => w.User)
            .Include(w => w.Room)
                .ThenInclude(r => r.Category)
            .FirstOrDefaultAsync(w => w.Id == dto.WaitlistEntryId);
        
        if (waitlistEntry == null)
            return BadRequest("Waitlist entry does not exist.");
        
        var booking = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Room)
                .ThenInclude(r => r.Category)
            .FirstOrDefaultAsync(b => b.Id == dto.BookingId);
        
        if (booking == null)
            return BadRequest("Booking does not exist.");
        
        var newOffer = new WaitlistOffers
        {
            WaitlistEntryId = dto.WaitlistEntryId,
            WaitlistEntry = waitlistEntry,
            BookingId = dto.BookingId,
            Booking = booking,
            Status = OfferStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.WaitlistOffers.Add(newOffer);
        await _context.SaveChangesAsync();

        await _emailService.SendEmailAsync(
            newOffer.WaitlistEntry.User.Email,
            "Venteliste tilbud - MercBooking",
            $"""
            <h2>Du har modtaget et tilbud på en booking!</h2>
            <p>Rum: {newOffer.Booking.Room.Name}</p>
            <p>Start: {newOffer.Booking.BookingStart}</p>
            <p>Slut: {newOffer.Booking.BookingEnd}</p>
            <p>Du kan nu acceptere eller afvise tilbuddet i MercBooking.</p>
            <p>Status: Afventer svar</p>
            """
        );

        var result = new WaitlistOfferGetDTO
        {
            Id = newOffer.Id,
            WaitlistEntry = new WaitlistEntryGetDTO
            {
                Id = newOffer.WaitlistEntry.Id,
                User = new UserGetDTO
                {
                    Id = newOffer.WaitlistEntry.User.Id,
                    Username = newOffer.WaitlistEntry.User.Username,
                    Email = newOffer.WaitlistEntry.User.Email,
                    Role = newOffer.WaitlistEntry.User.role.ToString()
                },
                Room = new RoomGetDTO
                {
                    Id = newOffer.WaitlistEntry.Room.Id,
                    Name = newOffer.WaitlistEntry.Room.Name,
                    Capacity = newOffer.WaitlistEntry.Room.Capacity,
                    floor = newOffer.WaitlistEntry.Room.floor,
                    number = newOffer.WaitlistEntry.Room.number,
                    isActive = newOffer.WaitlistEntry.Room.isActive,
                    Category = new CategoryGetDTO
                    {
                        Id = newOffer.WaitlistEntry.Room.Category.Id,
                        Name = newOffer.WaitlistEntry.Room.Category.Name
                    }
                },
                RequestedStart = newOffer.WaitlistEntry.RequestedStart,
                RequestedEnd = newOffer.WaitlistEntry.RequestedEnd,
                Comment = newOffer.WaitlistEntry.Comment,
                ParticipantCount = newOffer.WaitlistEntry.ParticipantCount,
                Status = newOffer.WaitlistEntry.status,
                CreatedAt = newOffer.WaitlistEntry.CreatedAt,
                UpdatedAt = newOffer.WaitlistEntry.UpdatedAt
            },
            Booking = new BookingGetDTO
            {
                Id = newOffer.Booking.Id,
                User = new UserGetDTO
                {
                    Id = newOffer.Booking.User.Id,
                    Username = newOffer.Booking.User.Username,
                    Email = newOffer.Booking.User.Email,
                    Role = newOffer.Booking.User.role.ToString()
                },
                Room = new RoomGetDTO
                {
                    Id = newOffer.Booking.Room.Id,
                    Name = newOffer.Booking.Room.Name,
                    Capacity = newOffer.Booking.Room.Capacity,
                    floor = newOffer.Booking.Room.floor,
                    number = newOffer.Booking.Room.number,
                    isActive = newOffer.Booking.Room.isActive,
                    Category = new CategoryGetDTO
                    {
                        Id = newOffer.Booking.Room.Category.Id,
                        Name = newOffer.Booking.Room.Category.Name
                    }
                },
                BookingStart = newOffer.Booking.BookingStart,
                BookingEnd = newOffer.Booking.BookingEnd,
                Comment = newOffer.Booking.Comment,
                Status = newOffer.Booking.Status,
                ParticipantCount = newOffer.Booking.ParticipantCount,
                CreatedAt = newOffer.Booking.CreatedAt,
                UpdatedAt = newOffer.Booking.UpdatedAt
            },
            Status = newOffer.Status,
            CreatedAt = newOffer.CreatedAt,
            UpdatedAt = newOffer.UpdatedAt
        };

        return CreatedAtAction(nameof(GetById), new { id = newOffer.Id }, result);
    }

    [Authorize(Roles = "Teacher,Admin")]
    [HttpPut("AnswerOffer/{id}")]
    public async Task<IActionResult> AnswerOffer(int id, OfferStatus answer)
    {
        var offer = await _context.WaitlistOffers
            .Include(o => o.WaitlistEntry)
                .ThenInclude(w => w.User)
            .Include(o => o.Booking)
                .ThenInclude(b => b.Room)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (offer == null)
            return NotFound();

        offer.Status = answer;
        offer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        if (answer == OfferStatus.Approved)
        {
            var booking = await _context.Bookings.FindAsync(offer.BookingId);
            if (booking != null)
            {
                booking.UserId = offer.WaitlistEntry.UserId;
                booking.Status = BookingStatus.Booked;
                booking.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            await _emailService.SendEmailAsync(
                offer.WaitlistEntry.User.Email,
                "Venteliste tilbud godkendt - MercBooking",
                $"""
                <h2>Dit venteliste tilbud er blevet godkendt!</h2>
                <p>Rum: {offer.Booking.Room.Name}</p>
                <p>Start: {offer.Booking.BookingStart}</p>
                <p>Slut: {offer.Booking.BookingEnd}</p>
                <p>Status: Godkendt</p>
                """
            );
        }
        else if (answer == OfferStatus.Rejected)
        {
            await _emailService.SendEmailAsync(
                offer.WaitlistEntry.User.Email,
                "Venteliste tilbud afvist - MercBooking",
                $"""
                <h2>Du har afvist venteliste tilbuddet</h2>
                <p>Rum: {offer.Booking.Room.Name}</p>
                <p>Start: {offer.Booking.BookingStart}</p>
                <p>Slut: {offer.Booking.BookingEnd}</p>
                <p>Status: Afvist</p>
                """
            );

            var waitlistEntry = await _context.WaitlistEntries
                .Where(e => e.RoomId == offer.Booking.RoomId &&
                    e.RequestedStart < offer.Booking.BookingEnd &&
                    e.RequestedEnd > offer.Booking.BookingStart &&
                    e.status == WaitlistStatus.Pending)
                .OrderBy(e => e.CreatedAt)
                .FirstOrDefaultAsync();

            if (waitlistEntry != null)
            {
                var newOffer = new WaitlistOffers
                {
                    WaitlistEntryId = waitlistEntry.Id,
                    BookingId = offer.Booking.Id,
                    Status = OfferStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.WaitlistOffers.Add(newOffer);
                await _context.SaveChangesAsync();

                await _emailService.SendEmailAsync(
                    newOffer.WaitlistEntry.User.Email,
                    "Nyt venteliste tilbud - MercBooking",
                    $"""
                    <h2>Du har modtaget et nyt tilbud på en booking!</h2>
                    <p>Rum: {offer.Booking.Room.Name}</p>
                    <p>Start: {offer.Booking.BookingStart}</p>
                    <p>Slut: {offer.Booking.BookingEnd}</p>
                    <p>Du kan nu acceptere eller afvise tilbuddet i MercBooking.</p>
                    <p>Status: Afventer svar</p>
                    """
                );
            }
        }

        return NoContent();
    }


    [Authorize(Roles = "Admin")]
    [HttpPut("update/{id}")]
    public async Task<IActionResult> Update(int id, WaitlistOfferUpdateDTO dto)
    {
        var offer = await _context.WaitlistOffers.FindAsync(id);

        if (offer == null)
            return NotFound();

        var waitlistEntryExists = await _context.WaitlistEntries
            .AnyAsync(w => w.Id == dto.WaitlistEntryId);

        if (!waitlistEntryExists)
            return BadRequest("Waitlist entry does not exist.");

        var bookingExists = await _context.Bookings
            .AnyAsync(b => b.Id == dto.BookingId);

        if (!bookingExists)
            return BadRequest("Booking does not exist.");

        offer.WaitlistEntryId = dto.WaitlistEntryId;
        offer.BookingId = dto.BookingId;
        offer.Status = dto.Status;
        offer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _emailService.SendEmailAsync(
            offer.WaitlistEntry.User.Email,
            "Venteliste tilbud opdateret - MercBooking",
            $"""
            <h2>Dit venteliste tilbud er blevet opdateret</h2>
            <p>Rum: {offer.Booking.Room.Name}</p>
            <p>Start: {offer.Booking.BookingStart}</p>
            <p>Slut: {offer.Booking.BookingEnd}</p>
            <p>Status: {offer.Status}</p>
            """
        );

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var offer = await _context.WaitlistOffers.FindAsync(id);

        if (offer == null)
            return NotFound();

        _context.WaitlistOffers.Remove(offer);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
