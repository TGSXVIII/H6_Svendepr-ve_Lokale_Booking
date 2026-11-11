var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration
    .GetConnectionString("MercBookingDb")
    ?? throw new InvalidOperationException(
        "Databaseforbindelsen MercBookingDb mangler.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));


builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(
                "http://127.0.0.1:5500",
                "http://localhost:5500"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
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

// Lokalt bruger vi HTTP. Behold HTTPS-redirect uden for Development.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseCors("Frontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
