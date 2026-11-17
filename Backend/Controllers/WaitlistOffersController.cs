namespace MercBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WaitlistOffersController : ControllerBase
{
    private readonly AppDbContext _context;

    public WaitlistOffersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("getAll")]
    public async Task<IActionResult> GetAll()
    {
        var offers = await _context.WaitlistOffers
            .Select(o => new WaitlistOfferGetDTO
            {
                Id = o.Id,
                WaitlistEntryId = o.WaitlistEntryId,
                BookingId = o.BookingId,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt
            })
            .ToListAsync();

        return Ok(offers);
    }

    [HttpGet("getById/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var offer = await _context.WaitlistOffers
            .Where(o => o.Id == id)
            .Select(o => new WaitlistOfferGetDTO
            {
                Id = o.Id,
                WaitlistEntryId = o.WaitlistEntryId,
                BookingId = o.BookingId,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (offer == null)
            return NotFound();

        return Ok(offer);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(WaitlistOfferCreateDTO dto)
    {
        var waitlistEntryExists = await _context.WaitlistEntries
            .AnyAsync(w => w.Id == dto.WaitlistEntryId);

        if (!waitlistEntryExists)
            return BadRequest("Waitlist entry does not exist.");

        var bookingExists = await _context.Bookings
            .AnyAsync(b => b.Id == dto.BookingId);

        if (!bookingExists)
            return BadRequest("Booking does not exist.");

        var newOffer = new WaitlistOffers
        {
            WaitlistEntryId = dto.WaitlistEntryId,
            WaitlistEntry = await _context.WaitlistEntries.FindAsync(dto.WaitlistEntryId) ?? new WaitlistEntries(),
            BookingId = dto.BookingId,
            Booking = await _context.Bookings.FindAsync(dto.BookingId) ?? new Bookings(),
            Status = OfferStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.WaitlistOffers.Add(newOffer);
        await _context.SaveChangesAsync();

        var result = new WaitlistOfferGetDTO
        {
            Id = newOffer.Id,
            WaitlistEntryId = newOffer.WaitlistEntryId,
            BookingId = newOffer.BookingId,
            Status = newOffer.Status,
            CreatedAt = newOffer.CreatedAt,
            UpdatedAt = newOffer.UpdatedAt
        };

        return CreatedAtAction(nameof(GetById), new { id = newOffer.Id }, result);
    }

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

        return NoContent();
    }

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
