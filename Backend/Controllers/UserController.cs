[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<Users> _passwordHasher;
    private readonly IEmailService _emailService;

    public UsersController(AppDbContext context, IEmailService emailService)
    {
        _context = context;
        _passwordHasher = new PasswordHasher<Users>();
        _emailService = emailService;
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
                Role = u.role.ToString(),
                LastEditedBy = u.LastEditedBy == null ? null : new UserGetDTO
                {
                    Id = u.LastEditedBy.Id,
                    Username = u.LastEditedBy.Username,
                    Email = u.LastEditedBy.Email,
                    Role = u.LastEditedBy.role.ToString()
                }
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
        if (await _context.Users.AnyAsync(u =>
            u.Username == userDTO.Username ||
            u.Email == userDTO.Email))
        {
            return BadRequest("Brugernavn eller email er allerede i brug.");
        }

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
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        if (user.Id != userId && !User.IsInRole("Admin"))
        {
            return StatusCode(403, "Du kan ikke ændre andre brugers oplysninger.");
        }

        if (await _context.Users.AnyAsync(u =>
            u.Id != id &&
            (u.Username == updatedUser.Username ||
            u.Email == updatedUser.Email)))
        {
            return BadRequest("Brugernavn eller email er allerede i brug.");
        }

        user.LastEditedById = userId;
        user.Username = updatedUser.Username;
        user.Email = updatedUser.Email;

        await _context.SaveChangesAsync();

        return Ok("Brugeren er blevet opdateret.");
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("updateRole/{id}")]
    public async Task<IActionResult> UpdateRole(int id, Role newRole)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        if (!Enum.IsDefined(typeof(Role), newRole))
        {
            return BadRequest("Invalid role specified.");
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        user.LastEditedById = userId;
        user.role = newRole;

        await _context.SaveChangesAsync();

        return Ok("Brugerens rolle er blevet opdateret.");
    }

    // PUT: api/users/changepassword/5
    [Authorize]
    [HttpPut("changepassword/{id}")]
    public async Task<IActionResult> ChangePassword(int id, UserChangePassDTO dto)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();
        
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        if (user.Id != userId && !User.IsInRole("Admin"))
        {
            return StatusCode(403, "Du kan ikke ændre andre brugers oplysninger.");
        }


        if (!User.IsInRole("Admin"))
        {
            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.Password,
                dto.OldPassword
            );

            if (result == PasswordVerificationResult.Failed)
                return BadRequest("Den gamle adgangskode er forkert.");
        }

        user.Password = _passwordHasher.HashPassword(user, dto.NewPassword);

        await _context.SaveChangesAsync();

        return Ok("Adgangskoden er blevet ændret.");
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("ResetPassword/{id}")]
    public async Task<IActionResult> ResetPassword(int id)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        // Generate a random temporary password
        const string characters = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz1234567890!@#$%";
        var random = new Random();
        var temporaryPassword = new string(
            Enumerable.Range(0, 16)
                .Select(_ => characters[random.Next(characters.Length)])
                .ToArray()
        );

        // Hash the password
        user.Password = _passwordHasher.HashPassword(user, temporaryPassword);
        user.LastEditedById = userId;

        await _context.SaveChangesAsync();

        // Send email
        await _emailService.SendEmailAsync(
            user.Email,
            "MercBooking - Nulstilling af adgangskode",
            $"""
            <h2>Din adgangskode er blevet nulstillet</h2>
            <p>En administrator har nulstillet din adgangskode.</p>
            <p>Din nye adgangskode er:</p>
            <h3>{temporaryPassword}</h3>
            <p>Du bedes ændre din adgangskode ved næste login.</p>
            <p>Med venlig hilsen<br>MercBooking</p>
            """
        );

        return Ok("En mail er blevet sendt til brugeren.");
    }

    // DELETE: api/users/5
    [Authorize]
    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        if (user.Id != userId && !User.IsInRole("Admin"))
        {
            return StatusCode(403, "Du kan ikke slette andre brugere.");
        }


        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Ok("Brugeren er blevet slettet.");
    }
}
