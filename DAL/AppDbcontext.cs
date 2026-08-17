using DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace DAL
{
    public class AppDbcontext
     : IdentityDbContext<App_User, IdentityRole<Guid>, Guid>
    {
        public AppDbcontext(DbContextOptions<AppDbcontext> options)
            : base(options)
        {
        }


        public DbSet<Car> Cars { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Payout> Payouts { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<TripStatusLog> TripStatusLogs { get; set; }
        public DbSet<DriverDocument> DriverDocuments { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<DriverDocument>()
       .HasIndex(d => d.DriverId)
       .IsUnique();

            modelBuilder.HasPostgresExtension("postgis");

            modelBuilder.Entity<App_User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<Driver>()
                .Property(d => d.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Trip>()
                 .Property(t => t.Status)
                 .HasConversion<string>();

            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Passenger)
                .WithMany()
                .HasForeignKey(t => t.PassengerId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Driver)
                .WithMany()
                .HasForeignKey(t => t.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Car)
                .WithMany()
                .HasForeignKey(t => t.CarId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TripStatusLog>()
                .Property(tsl => tsl.Status)
                .HasConversion<string>();

            modelBuilder.Entity<TripStatusLog>()
                .HasOne(tsl => tsl.Trip)
                .WithMany(t => t.TripStatusLogs)
                .HasForeignKey(tsl => tsl.TripId)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<Payment>()
                .Property(p => p.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Trip)
                .WithOne()

                .HasForeignKey<Payment>(p => p.TripId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payout>()
                .Property(p => p.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Payout>()
                .HasOne(p => p.Driver)
                .WithMany()
                .HasForeignKey(p => p.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Trip)
                .WithMany()
                .HasForeignKey(r => r.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Offer>()
                .Property(o => o.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Offer>()
                .HasOne(o => o.Trip)
                .WithMany() 
                .HasForeignKey(o => o.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Offer>()
                .HasOne(o => o.Driver)
                .WithMany()
                .HasForeignKey(o => o.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Driver>()
    .HasOne(u => u.DriverDocument)
    .WithOne(d => d.Driver)
    .HasForeignKey<DriverDocument>(d => d.DriverId)
    .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Notification>()
    .HasOne(n => n.User)
    .WithMany(u => u.notifications)
    .HasForeignKey(n => n.UserId)
    .OnDelete(DeleteBehavior.Cascade);
        }
       

    }
}