
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<Users> _passwordHasher;

    public UsersController(AppDbContext context)
    {
        _context = context;
        _passwordHasher = new PasswordHasher<Users>();
    }

    // GET: api/users
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _context.Users.ToListAsync();
        return Ok(users);
    }

    // GET: api/users/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    // POST: api/users
    [HttpPost]
    public async Task<IActionResult> Create(Users user)
    {
        user.Password = _passwordHasher.HashPassword(user, user.Password);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = user.Id },
            user
        );
    }

    // PUT: api/users/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Users updatedUser)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        user.Username = updatedUser.Username;
        user.Email = updatedUser.Email;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PUT: api/users/changepassword/5
    [HttpPut("changepassword/{id}")]
    public async Task<IActionResult> ChangePassword(int id, Users updatedUser)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        user.Password = _passwordHasher.HashPassword(user, updatedUser.Password);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/users/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
