var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration
    .GetConnectionString("MercBookingDb")
    ?? throw new InvalidOperationException(
        "Databaseforbindelsen MercBookingDb mangler.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Midlertidigt endpoint til at teste databaseforbindelsen.
    app.MapGet("/health/database", async (AppDbContext db) =>
    {
        var canConnect = await db.Database.CanConnectAsync();

        return canConnect
            ? Results.Ok(new { message = "Forbindelsen til databasen virker." })
            : Results.Problem(
                detail: "Kunne ikke forbinde til databasen.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();