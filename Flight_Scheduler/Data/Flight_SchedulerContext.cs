using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Flight_Scheduler.Models;

namespace Flight_Scheduler.Data
{
    public class Flight_SchedulerContext : DbContext
    {
        public Flight_SchedulerContext (DbContextOptions<Flight_SchedulerContext> options)
            : base(options)
        {
        }

        public DbSet<Flight_Scheduler.Models.Airline> Airline { get; set; } = default!;
        public DbSet<Flight_Scheduler.Models.Flight> Flight { get; set; } = default!;

        public DbSet<Aircraft> Aircrafts { get; set; } = default!;
        public DbSet<FlightCrew> FlightCrews { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FlightCrew>()
                .Property(fc => fc.Position)
                .HasConversion<string>();

            modelBuilder.Entity<Flight>()
                .HasMany(f => f.FlightCrews)
                .WithMany(fc => fc.Flights);
        }
    }
}
