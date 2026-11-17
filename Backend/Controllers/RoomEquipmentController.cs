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

    [HttpGet("getAll")]
    public async Task<IActionResult> GetAll()
    {
        var roomEquipment = await _context.RoomEquipment
            .Select(re => new RoomEquipmentGetDTO
            {
                Id = re.Id,
                RoomId = re.RoomId,
                EquipmentId = re.EquipmentId,
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
            .Select(re => new RoomEquipmentGetDTO
            {
                Id = re.Id,
                RoomId = re.RoomId,
                EquipmentId = re.EquipmentId,
                Quantity = re.Quantity
            })
            .FirstOrDefaultAsync();

        if (roomEquipment == null)
            return NotFound();

        return Ok(roomEquipment);
    }

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
            RoomId = newRoomEquipment.RoomId,
            EquipmentId = newRoomEquipment.EquipmentId,
            Quantity = newRoomEquipment.Quantity
        };

        return CreatedAtAction(nameof(GetById), new { id = newRoomEquipment.Id }, result);
    }

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
