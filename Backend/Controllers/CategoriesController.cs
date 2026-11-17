namespace MercBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Categories
    [HttpGet("getAll")]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _context.Categories
            .Select(c => new CategoryGetDTO
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToListAsync();

        return Ok(categories);
    }

    // GET: api/Categories/5
    [HttpGet("getById/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _context.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryGetDTO
            {
                Id = c.Id,
                Name = c.Name
            })
            .FirstOrDefaultAsync();

        if (category == null)
            return NotFound();

        return Ok(category);
    }

    // POST: api/Categories
    [HttpPost("create")]
    public async Task<IActionResult> Create(CategoryCreateDTO dto)
    {
        var newCategory = new Categories
        {
            Name = dto.Name
        };

        _context.Categories.Add(newCategory);
        await _context.SaveChangesAsync();

        var result = new CategoryGetDTO
        {
            Id = newCategory.Id,
            Name = newCategory.Name
        };

        return CreatedAtAction(
            nameof(GetById),
            new { id = newCategory.Id },
            result
        );
    }

    // PUT: api/Categories/5
    [HttpPut("update/{id}")]
    public async Task<IActionResult> Update(int id, CategoryUpdateDTO dto)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
            return NotFound();

        category.Name = dto.Name;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Categories/5
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
            return NotFound();

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
