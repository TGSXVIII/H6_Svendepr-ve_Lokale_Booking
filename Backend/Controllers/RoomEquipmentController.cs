namespace MercBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomEquipmentController : ControllerBase
{
    private readonly AppDbContext _context;

    public RoomEquipmentController(AppDbContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpGet("getAll")]
    public async Task<IActionResult> GetAll()
    {
        var roomEquipment = await _context.RoomEquipment
            .Include(re => re.Room)
                .ThenInclude(r => r.Category)
            .Include(re => re.Equipment)
            .Select(re => new RoomEquipmentGetDTO
            {
                Id = re.Id,
                Room = new RoomGetDTO
                {
                    Id = re.Room.Id,
                    Name = re.Room.Name,
                    floor = re.Room.floor,
                    number = re.Room.number,
                    Capacity = re.Room.Capacity,
                    isActive = re.Room.isActive,
                    Category = new CategoryGetDTO
                    {
                        Id = re.Room.Category.Id,
                        Name = re.Room.Category.Name
                    }
                },
                Equipment = new EquipmentGetDTO
                {
                    Id = re.Equipment.Id,
                    Name = re.Equipment.Name,
                },
                Quantity = re.Quantity
            })
            .ToListAsync();

        return Ok(roomEquipment);
    }

    [HttpGet("getById/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var roomEquipment = await _context.RoomEquipment
            .Where(re => re.Id == id)
            .Include(re => re.Room)
                .ThenInclude(r => r.Category)
            .Include(re => re.Equipment)
            .Select(re => new RoomEquipmentGetDTO
            {
                Id = re.Id,
                Room = new RoomGetDTO
                {
                    Id = re.Room.Id,
                    Name = re.Room.Name,
                    floor = re.Room.floor,
                    number = re.Room.number,
                    Capacity = re.Room.Capacity,
                    isActive = re.Room.isActive,
                    Category = new CategoryGetDTO
                    {
                        Id = re.Room.Category.Id,
                        Name = re.Room.Category.Name
                    }
                },
                Equipment = new EquipmentGetDTO
                {
                    Id = re.Equipment.Id,
                    Name = re.Equipment.Name,
                },
                Quantity = re.Quantity
            })
            .FirstOrDefaultAsync();

        if (roomEquipment == null)
            return NotFound();

        return Ok(roomEquipment);
    }

    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> Create(RoomEquipmentCreateDTO dto)
    {
        var room = await _context.Rooms.FindAsync(dto.RoomId);
        if (room == null)
        {
            return BadRequest("Room does not exist.");
        }

        var equipment = await _context.Equipment.FindAsync(dto.EquipmentId);
        if (equipment == null)
        {
            return BadRequest("Equipment does not exist.");
        }

        var newRoomEquipment = new RoomEquipment
        {
            RoomId = dto.RoomId,
            Room = room,
            EquipmentId = dto.EquipmentId,
            Equipment = equipment,
            Quantity = dto.Quantity
        };

        _context.RoomEquipment.Add(newRoomEquipment);
        await _context.SaveChangesAsync();

        var result = new RoomEquipmentGetDTO
        {
            Id = newRoomEquipment.Id,
            Room = new RoomGetDTO
            {
                Id = room.Id,
                Name = room.Name,
                floor = room.floor,
                number = room.number,
                Capacity = room.Capacity,
                isActive = room.isActive,
                Category = new CategoryGetDTO
                {
                    Id = room.Category.Id,
                    Name = room.Category.Name
                }
            },
            Equipment = new EquipmentGetDTO
            {
                Id = equipment.Id,
                Name = equipment.Name
            },
            Quantity = newRoomEquipment.Quantity
        };

        return CreatedAtAction(nameof(GetById), new { id = newRoomEquipment.Id }, result);
    }
    
    [Authorize]
    [HttpPut("update/{id}")]
    public async Task<IActionResult> Update(int id, RoomEquipmentUpdateDTO dto)
    {
        var roomEquipment = await _context.RoomEquipment.FindAsync(id);

        if (roomEquipment == null)
            return NotFound();

        var room = await _context.Rooms.FindAsync(dto.RoomId);
        if (room == null)
        {
            return BadRequest("Room does not exist.");
        }

        var equipment = await _context.Equipment.FindAsync(dto.EquipmentId);
        if (equipment == null)
        {
            return BadRequest("Equipment does not exist.");
        }

        roomEquipment.RoomId = dto.RoomId;
        roomEquipment.Room = room;
        roomEquipment.EquipmentId = dto.EquipmentId;
        roomEquipment.Equipment = equipment;
        roomEquipment.Quantity = dto.Quantity;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [Authorize]
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var roomEquipment = await _context.RoomEquipment.FindAsync(id);

        if (roomEquipment == null)
            return NotFound();

        _context.RoomEquipment.Remove(roomEquipment);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
