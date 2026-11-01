using Hostel_hub.Data;
using Hostel_hub.Models;
using Hostel_hub.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HostelHub.Tests;

// SQLite exercises real foreign keys, unique indexes and transactions.
// SQL Server-generated rowversion is deliberately excluded from this test model.
// Its concurrency behavior must also be tested against SQL Server (see TESTING.md).
public sealed class TestDbContext(DbContextOptions<ApplicationDbContext> options) : ApplicationDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Room>().Property(r => r.RowVersion).IsRowVersion().ValueGeneratedNever();
    }
}

public sealed class TestDatabase : IDisposable
{
    public SqliteConnection Connection { get; } = new("Data Source=:memory:");
    public ApplicationDbContext Db { get; }
    public TestDatabase()
    {
        Connection.Open();
        Db = new TestDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(Connection).Options);
        Db.Database.EnsureCreated();
        Seed(Db);
    }
    public static void Seed(ApplicationDbContext db)
    {
        // Synthetic test data only. This password is never used by the production seeder.
        var hash = new AuthService().HashPassword("Test-only-123!");
        db.Users.AddRange(
            new User { UserId=1, Email="admin@test.local", PasswordHash=hash, Role=UserRole.Admin },
            new User { UserId=2, Email="warden@test.local", PasswordHash=hash, Role=UserRole.Admin },
            new User { UserId=3, Email="student@test.local", PasswordHash=hash, Role=UserRole.Student },
            new User { UserId=4, Email="other@test.local", PasswordHash=hash, Role=UserRole.Student },
            new User { UserId=5, Email="staff@test.local", PasswordHash=hash, Role=UserRole.MaintenanceStaff },
            new User { UserId=6, Email="unassigned@test.local", PasswordHash=hash, Role=UserRole.Admin });
        db.Hostels.AddRange(new Hostel {HostelId=1,Name="North",Type=HostelType.Mixed},new Hostel {HostelId=2,Name="South",Type=HostelType.Mixed});
        db.Rooms.AddRange(new Room {RoomId=1,HostelId=1,RoomNumber="101",Capacity=2,CurrentOccupancy=1},
            new Room {RoomId=2,HostelId=1,RoomNumber="102",Capacity=2},new Room {RoomId=3,HostelId=2,RoomNumber="201",Capacity=1});
        db.Students.AddRange(new Student {StudentId=1,UserId=3,FullName="Test Student",RollNumber="T001",HostelId=1,RoomId=1},
            new Student {StudentId=2,UserId=4,FullName="Other Student",RollNumber="T002",HostelId=2});
        db.Wardens.AddRange(new Warden {WardenId=1,UserId=2,FullName="Test Warden",HostelId=1},
            new Warden {WardenId=2,UserId=6,FullName="Unassigned Warden"});
        db.MaintenanceStaff.Add(new MaintenanceStaff {MaintenanceStaffId=1,UserId=5,FullName="Test Staff",HostelId=1,IsActive=true});
        db.Complaints.Add(new Complaint {ComplaintId=1,StudentId=1,Title="Broken light",Description="Room light does not work",Category=ComplaintCategory.Electrical});
        db.MessMenus.Add(new MessMenu {MessMenuId=1,HostelId=1,CreatedByUserId=2,MenuDate=DateOnly.FromDateTime(DateTime.Today.AddDays(2))});
        db.SaveChanges();
    }
    public void Dispose() { Db.Dispose(); Connection.Dispose(); }
}

public sealed class FixedClock(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}
