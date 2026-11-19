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
    [Authorize(Roles = "Admin")]
    [HttpGet("getAll")]
    public async Task<IActionResult> GetAll()
    {
        var users = await _context.Users.Select(u => new UserGetDTO
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.role.ToString()
            }).ToListAsync();
        return Ok(users);
    }

    // GET: api/users/5
    [Authorize(Roles = "Teacher, Admin")]
    [HttpGet("GetUser/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _context.Users.Where(u => u.Id == id).Select(u => new UserGetDTO
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.role.ToString()
            }).FirstOrDefaultAsync();

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    // GET: api/users/CurrentUser
    [Authorize]
    [HttpGet("CurrentUser")]
    public async Task<IActionResult> CurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
 
        if (!int.TryParse(userId, out var id))
            return Unauthorized();
 
        var user = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.role
            })
            .FirstOrDefaultAsync();
 
        if (user == null)
            return Unauthorized();
 
        return Ok(new
        {
            user.Id,
            user.Username,
            user.Email,
            role = user.role.ToString()
        });
    }

    [Authorize(Roles = "Teacher, Admin")]
    [HttpGet("GetAllStudents")]
    public async Task<IActionResult> GetAllStudents()
    {
        var students = await _context.Users
            .Where(u => u.role == Role.Student)
            .Select(u => new UserGetDTO
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.role.ToString()
            })
            .ToListAsync();

        return Ok(students);
    }

    // POST: api/users
    [HttpPost("Register")]
    public async Task<IActionResult> Create(UserCreateDTO userDTO)
    {
        Users newUser = new Users
        {
            Username = userDTO.Username,
            Email = userDTO.Email,
            role = Role.Student // Default role
        };
        newUser.Password = _passwordHasher.HashPassword(newUser, userDTO.Password);
        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        UserGetDTO user = new UserGetDTO
        {
            Id = newUser.Id,
            Username = newUser.Username,
            Email = newUser.Email,
            Role = newUser.role.ToString()
        };

        return CreatedAtAction(
            nameof(GetById),
            new { id = user.Id },
            user
        );
    }
    

    // PUT: api/users/5
    [Authorize]
    [HttpPut("update/{id}")]
    public async Task<IActionResult> Update(int id, UserUpdateDTO updatedUser)
    {
        var request = await _context.BookingRequests.FindAsync(id);

        if (request == null)
            return NotFound();

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        if (request.UserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid("Du kan ikke ændre andre brugers oplysninger.");
        }

        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        user.Username = updatedUser.Username;
        user.Email = updatedUser.Email;

        await _context.SaveChangesAsync();

        return Ok("Brugeren er blevet opdateret.");
    }

    // PUT: api/users/changepassword/5
    [Authorize]
    [HttpPut("changepassword/{id}")]
    public async Task<IActionResult> ChangePassword(int id, String newPassword)
    {
        var request = await _context.BookingRequests.FindAsync(id);

        if (request == null)
            return NotFound();

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        if (request.UserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid("Du kan ikke ændre andre brugers adgangskoder.");
        }

        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        user.Password = _passwordHasher.HashPassword(user, newPassword);

        await _context.SaveChangesAsync();

        return Ok("Adgangskoden er blevet ændret.");
    }

    // DELETE: api/users/5
    [Authorize]
    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var request = await _context.BookingRequests.FindAsync(id);

        if (request == null)
            return NotFound();

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        if (request.UserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid("Du kan ikke slette andre brugere.");
        }

        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Ok("Brugeren er blevet slettet.");
    }
}
