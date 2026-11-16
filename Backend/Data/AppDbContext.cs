namespace MercBooking.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    
        modelBuilder.Entity<RoomEquipment>()
            .HasOne(re => re.Room)
            .WithMany(r => r.RoomEquipment)
            .HasForeignKey(re => re.RoomId);
    
        modelBuilder.Entity<RoomEquipment>()
            .HasOne(re => re.Equipment)
            .WithMany(e => e.RoomEquipment)
            .HasForeignKey(re => re.EquipmentId);
    }
    public DbSet<Users> Users { get; set; }
    public DbSet<Rooms> Rooms { get; set; }
    public DbSet<Bookings> Bookings { get; set; }
    public DbSet<Categories> Categories { get; set; }
    public DbSet<Equipment> Equipment { get; set; }
    public DbSet<RoomEquipment> RoomEquipment { get; set; }
    public DbSet<WaitlistEntries> WaitlistEntries { get; set; }
    public DbSet<WaitlistOffers> WaitlistOffers { get; set; }
    public DbSet<BookingRequests> BookingRequests { get; set; }
}
