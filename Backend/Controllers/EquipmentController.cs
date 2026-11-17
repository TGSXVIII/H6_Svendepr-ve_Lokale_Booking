namespace MercBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EquipmentController : ControllerBase
{
    private readonly AppDbContext _context;

    public EquipmentController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Equipment
    [HttpGet("getAll")]
    public async Task<IActionResult> GetAll()
    {
        var equipment = await _context.Equipment
            .Select(e => new EquipmentGetDTO
            {
                Id = e.Id,
                Name = e.Name
            })
            .ToListAsync();

        return Ok(equipment);
    }

    // GET: api/Equipment/5
    [HttpGet("getById/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var equipment = await _context.Equipment
            .Where(e => e.Id == id)
            .Select(e => new EquipmentGetDTO
            {
                Id = e.Id,
                Name = e.Name
            })
            .FirstOrDefaultAsync();

        if (equipment == null)
            return NotFound();

        return Ok(equipment);
    }

    // POST: api/Equipment
    [HttpPost("create")]
    public async Task<IActionResult> Create(EquipmentCreateDTO dto)
    {
        var newEquipment = new Equipment
        {
            Name = dto.Name
        };

        _context.Equipment.Add(newEquipment);
        await _context.SaveChangesAsync();

        var result = new EquipmentGetDTO
        {
            Id = newEquipment.Id,
            Name = newEquipment.Name
        };

        return CreatedAtAction(
            nameof(GetById),
            new { id = newEquipment.Id },
            result
        );
    }

    // PUT: api/Equipment/5
    [HttpPut("update/{id}")]
    public async Task<IActionResult> Update(int id, EquipmentUpdateDTO dto)
    {
        var equipment = await _context.Equipment.FindAsync(id);

        if (equipment == null)
            return NotFound();

        equipment.Name = dto.Name;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Equipment/5
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var equipment = await _context.Equipment.FindAsync(id);

        if (equipment == null)
            return NotFound();

        _context.Equipment.Remove(equipment);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
