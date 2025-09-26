using IdStrategiesDemo.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace IdStrategiesDemo.Data;

public class AppDbContext : DbContext
{
    private readonly IConfiguration _config;
    public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration config) : base(options)
    {
        _config = config;
        Database.SetCommandTimeout(TimeSpan.FromMinutes(30)); // uzun testler için
    }

    public DbSet<RecInt> RecsInt => Set<RecInt>();
    public DbSet<RecGuidV4> RecsGuidV4 => Set<RecGuidV4>();
    public DbSet<RecUlidString> RecsUlidString => Set<RecUlidString>();
    public DbSet<RecUlidBinary> RecsUlidBinary => Set<RecUlidBinary>();
    public DbSet<RecGuidV4_ClusterOnDate> RecsGuidV4_ClusterOnDate => Set<RecGuidV4_ClusterOnDate>();
    public DbSet<RecGuidV7> RecsGuidV7 => Set<RecGuidV7>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // 1) INT IDENTITY, clustered default
        mb.Entity<RecInt>(e =>
        {
            e.ToTable("RecInt");
            e.HasKey(x => x.Id).IsClustered();
            e.Property(x => x.Id).UseIdentityColumn();
            e.Property(x => x.CreatedOn).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        // 2) GUID v4 clustered
        mb.Entity<RecGuidV4>(e =>
        {
            e.ToTable("RecGuidV4");
            e.HasKey(x => x.Id).IsClustered();
            e.Property(x => x.Id).ValueGeneratedNever(); // client-side
            e.Property(x => x.CreatedOn).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        // 3) ULID string(26) clustered
        mb.Entity<RecUlidString>(e =>
        {
            e.ToTable("RecUlidString");
            e.HasKey(x => x.Id).IsClustered();
            e.Property(x => x.Id)
                .HasMaxLength(26)
                .IsFixedLength()
                .IsUnicode(false); // CHAR(26)
            e.Property(x => x.CreatedOn).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        // 4) ULID binary(16) clustered
        mb.Entity<RecUlidBinary>(e =>
        {
            e.ToTable("RecUlidBinary");
            e.HasKey(x => x.Id).IsClustered();
            e.Property(x => x.Id)
                .HasColumnType("BINARY(16)")
                .ValueGeneratedNever();
            e.Property(x => x.CreatedOn).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        // 5) GUID v4 nonclustered PK + clustered CreatedOn
        mb.Entity<RecGuidV4_ClusterOnDate>(e =>
        {
            e.ToTable("RecGuidV4_ClusterOnDate");
            e.HasKey(x => x.Id).IsClustered(false);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.CreatedOn)
                .HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(x => x.CreatedOn).IsClustered();
        });

        // 6) GUID v7 clustered
        mb.Entity<RecGuidV7>(e =>
        {
            e.ToTable("RecGuidV7");
            e.HasKey(x => x.Id).IsClustered();
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.CreatedOn).HasDefaultValueSql("SYSUTCDATETIME()");
        });
    }
}