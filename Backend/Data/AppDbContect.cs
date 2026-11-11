namespace MercBooking.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    public DbSet<Users> Users { get; set; }
    public DbSet<Rooms> Rooms { get; set; }
    public DbSet<Bookings> Bookings { get; set; }
    public DbSet<categories> Categories { get; set; }
    public DbSet<equipment> Equipment { get; set; }
    public DbSet<RoomEquipment> RoomEquipment { get; set; }
    public DbSet<WaitlistEntries> WaitlistEntries { get; set; }
    public DbSet<WaitlistOffers> WaitlistOffers { get; set; }
    public DbSet<BookingRequests> BookingRequests { get; set; }
}
