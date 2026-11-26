using MailKit;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<Users> _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
 
    public AuthController(
        AppDbContext context,
        IConfiguration configuration,
        IEmailService emailService)
    {
        _context = context;
        _configuration = configuration;
        _passwordHasher = new PasswordHasher<Users>();
        _emailService = emailService;
    }

    [Authorize]
    [HttpPost("mailtest")]
    public async Task<IActionResult> MailTest()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return NotFound();

        await _emailService.SendEmailAsync(
            user.Email,
            "Test email",
            "<h2>Hello from MercBooking!</h2>"
        );

        return Ok("Test email sent!");
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
}
