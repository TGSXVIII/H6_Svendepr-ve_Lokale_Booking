[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<Users> _passwordHasher;
    private readonly IConfiguration _configuration;
 
    public AuthController(
        AppDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        _passwordHasher = new PasswordHasher<Users>();
    }
 
    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDTO login)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == login.Username);
 
        if (user == null)
            return Unauthorized("Invalid username or password");
 
        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.Password,
            login.Password
        );
 
        if (result == PasswordVerificationResult.Failed)
            return Unauthorized("Invalid username or password");
 
        // Generate JWT
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.role.ToString())
        };
 
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"]!
            )
        );
 
        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );
 
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials
        );
 
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
 
        return Ok(new
        {
            token = jwt,
            user = new
            {
                user.Id,
                user.Username,
                role = user.role.ToString()
            }
        });
    }
 
    // GET: api/auth/CurrentUser
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
}
