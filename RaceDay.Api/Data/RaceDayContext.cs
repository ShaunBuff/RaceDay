using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data
{
    public class RaceDayContext : DbContext
    {
        public RaceDayContext(DbContextOptions<RaceDayContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<EventType> EventTypes { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users", "dbo");
                entity.HasKey(u => u.UserID);
                entity.Property(u => u.UserID).UseIdentityColumn();
                entity.Property(u => u.FirstName).HasMaxLength(50).IsRequired();
                entity.Property(u => u.LastName).HasMaxLength(50).IsRequired();
                entity.Property(u => u.Email).HasMaxLength(255).IsRequired();
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
                entity.Property(u => u.Role).HasMaxLength(20).IsRequired();
                entity.Property(u => u.CreatedAt)
                    .HasColumnType("datetime2")
                    .HasDefaultValueSql("SYSUTCDATETIME()");
            });

            modelBuilder.Entity<EventType>(entity =>
            {
                entity.ToTable("EventTypes", "dbo");
                entity.HasKey(e => e.EventTypeID);
                entity.Property(e => e.EventTypeID).UseIdentityColumn();
                entity.Property(e => e.TypeName).HasMaxLength(50).IsRequired();
                entity.HasIndex(e => e.TypeName).IsUnique();
                entity.Property(e => e.Description).HasMaxLength(250);
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.ToTable("Locations", "dbo");
                entity.HasKey(l => l.LocationID);
                entity.Property(l => l.LocationID).UseIdentityColumn();
                entity.Property(l => l.LocationName).HasMaxLength(100).IsRequired();
                entity.Property(l => l.Address).HasMaxLength(200);
                entity.Property(l => l.City).HasMaxLength(100).IsRequired();
                entity.Property(l => l.Province).HasMaxLength(100).IsRequired();
            });

            modelBuilder.Entity<Event>(entity =>
            {
                entity.ToTable("Events", "dbo");
                entity.HasKey(e => e.EventID);
                entity.Property(e => e.EventID).UseIdentityColumn();
                entity.Property(e => e.EventName).HasMaxLength(150).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.EventDate).HasColumnType("date");
                entity.Property(e => e.DistanceKm).HasColumnType("decimal(7,2)");
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime2")
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasOne<User>().WithMany()
                    .HasForeignKey(e => e.OrganiserID)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<EventType>().WithMany()
                    .HasForeignKey(e => e.EventTypeID)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Location>().WithMany()
                    .HasForeignKey(e => e.LocationID)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories", "dbo");
                entity.HasKey(c => c.CategoryID);
                entity.Property(c => c.CategoryID).UseIdentityColumn();
                entity.Property(c => c.CategoryName).HasMaxLength(100).IsRequired();
                entity.Property(c => c.DistanceKm).HasColumnType("decimal(7,2)");
                entity.HasIndex(c => new { c.EventID, c.CategoryName }).IsUnique();

                entity.HasOne<Event>().WithMany()
                    .HasForeignKey(c => c.EventID)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Enrolment>(entity =>
            {
                entity.ToTable("Enrolments", "dbo");
                entity.HasKey(e => e.EnrolmentID);
                entity.Property(e => e.EnrolmentID).UseIdentityColumn();
                entity.Property(e => e.EnrolmentDate)
                    .HasColumnType("datetime2")
                    .HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .HasDefaultValue("Registered")
                    .IsRequired();
                entity.HasIndex(e => new { e.ParticipantID, e.EventID }).IsUnique();

                entity.HasOne<User>().WithMany()
                    .HasForeignKey(e => e.ParticipantID)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Event>().WithMany()
                    .HasForeignKey(e => e.EventID)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Category>().WithMany()
                    .HasForeignKey(e => e.CategoryID)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Result>(entity =>
            {
                entity.ToTable("Results", "dbo");
                entity.HasKey(r => r.ResultID);
                entity.Property(r => r.ResultID).UseIdentityColumn();
                entity.Property(r => r.FinishTime).HasColumnType("time(0)");
                entity.Property(r => r.FinishingPosition);
                entity.Property(r => r.RecordedAt)
                    .HasColumnType("datetime2")
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasIndex(r => r.EnrolmentID).IsUnique();

                entity.HasOne<Enrolment>().WithMany()
                    .HasForeignKey(r => r.EnrolmentID)
                    .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}