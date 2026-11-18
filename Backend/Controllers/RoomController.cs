[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly AppDbContext _context;

    public RoomsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/rooms
    [Authorize(Roles = "Admin")]
    [HttpGet("getAll")]
    public async Task<IActionResult> GetAll()
    {
        var rooms = await _context.Rooms
            .Include(r => r.Category)
            .Select(r => new RoomGetDTO
            {
                Id = r.Id,
                Name = r.Name,
                floor = r.floor,
                number = r.number,
                Capacity = r.Capacity,
                isActive = r.isActive,
                Category = new CategoryGetDTO
                {
                    Id = r.Category.Id,
                    Name = r.Category.Name
                }
            })
            .ToListAsync();

        return Ok(rooms);
    }

    [Authorize]
    [HttpGet("getActive")]
    public async Task<IActionResult> GetActive()
    {
        var rooms = await _context.Rooms
            .Where(r => r.isActive)
            .Include(r => r.Category)
            .Select(r => new RoomGetDTO
            {
                Id = r.Id,
                Name = r.Name,
                floor = r.floor,
                number = r.number,
                Capacity = r.Capacity,
                isActive = r.isActive,
                Category = new CategoryGetDTO
                {
                    Id = r.Category.Id,
                    Name = r.Category.Name
                }
            })
            .ToListAsync();

        return Ok(rooms);
    }
    
    [Authorize]
    [HttpGet("getByCategory/{categoryId}")]
    public async Task<IActionResult> GetByCategory(int categoryId)
    {
        var rooms = await _context.Rooms
            .Where(r => r.CategoryId == categoryId)
            .Where(r => r.isActive)
            .Include(r => r.Category)
            .Select(r => new RoomGetDTO
            {
                Id = r.Id,
                Name = r.Name,
                floor = r.floor,
                number = r.number,
                Capacity = r.Capacity,
                isActive = r.isActive,
                Category = new CategoryGetDTO
                {
                    Id = r.Category.Id,
                    Name = r.Category.Name
                }
            })
            .ToListAsync();

        return Ok(rooms);
    }

    [Authorize]
    [HttpGet("getNotBooked/{start}/{end}")]
    public async Task<IActionResult> GetNotBooked(DateTime start, DateTime end)
    {
        if (start >= end)
            return BadRequest("Invalid date range.");

        start = DateTime.SpecifyKind(start, DateTimeKind.Utc);
        end = DateTime.SpecifyKind(end, DateTimeKind.Utc);

        var bookedRoomIds = await _context.Bookings
            .Where(b => b.BookingStart < end && b.BookingEnd > start)
            .Select(b => b.RoomId)
            .ToListAsync();

        var rooms = await _context.Rooms
            .Where(r => !bookedRoomIds.Contains(r.Id) && r.isActive)
            .Include(r => r.Category)
            .Select(r => new RoomGetDTO
            {
                Id = r.Id,
                Name = r.Name,
                floor = r.floor,
                number = r.number,
                Capacity = r.Capacity,
                isActive = r.isActive,
                Category = new CategoryGetDTO
                {
                    Id = r.Category.Id,
                    Name = r.Category.Name
                }
            })
            .ToListAsync();

        return Ok(rooms);
    }

    // GET: api/rooms/5
    [Authorize]
    [HttpGet("GetRoom/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var room = await _context.Rooms
            .Where(r => r.Id == id)
            .Include(r => r.Category)
            .Select(r => new RoomGetDTO
            {
                Id = r.Id,
                Name = r.Name,
                floor = r.floor,
                number = r.number,
                Capacity = r.Capacity,
                isActive = r.isActive,
                Category = new CategoryGetDTO
                {
                    Id = r.Category.Id,
                    Name = r.Category.Name
                }
            })
            .FirstOrDefaultAsync();

        if (room == null)
            return NotFound();

        return Ok(room);
    }

    // POST: api/rooms
    [Authorize]
    [HttpPost("Create")]
    public async Task<IActionResult> Create(RoomCreateDTO roomDTO)
    {
        
        var category = await _context.Categories.FindAsync(roomDTO.CategoryId);

        if (category == null)
            return BadRequest("Category does not exist.");

        Rooms newRoom = new Rooms
        {
            Name = roomDTO.Name,
            floor = roomDTO.floor,
            number = roomDTO.number,
            Capacity = roomDTO.Capacity,
            isActive = true,
            CategoryId = roomDTO.CategoryId,
            Category = category
        };

        _context.Rooms.Add(newRoom);
        await _context.SaveChangesAsync();

        RoomGetDTO room = new RoomGetDTO
        {
            Id = newRoom.Id,
            Name = newRoom.Name,
            floor = newRoom.floor,
            number = newRoom.number,
            Capacity = newRoom.Capacity,
            isActive = newRoom.isActive,
            Category = new CategoryGetDTO
            {
                Id = category.Id,
                Name = category.Name
            }
        };

        return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);
    }

    // PUT: api/rooms/5
    [Authorize]
    [HttpPut("update/{id}")]
    public async Task<IActionResult> Update(int id, RoomUpdateDTO roomDTO)
    {
        var room = await _context.Rooms.FindAsync(id);

        if (room == null)
            return NotFound();

        room.Name = roomDTO.Name;
        room.floor = roomDTO.floor;
        room.number = roomDTO.number;
        room.Capacity = roomDTO.Capacity;
        room.isActive = roomDTO.isActive;
        room.CategoryId = roomDTO.CategoryId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/rooms/5
    [Authorize]
    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var room = await _context.Rooms.FindAsync(id);

        if (room == null)
            return NotFound();

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
