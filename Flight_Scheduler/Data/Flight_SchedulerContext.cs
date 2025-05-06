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

        public DbSet<Destination> Destinations { get; set; } = default;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FlightCrew>()
                .Property(fc => fc.Position)
                .HasConversion<string>();

            modelBuilder.Entity<Flight>()
                .HasMany(f => f.FlightCrews)
                .WithMany(fc => fc.Flights);

            modelBuilder.Entity<Flight>()
               .HasOne(f => f.Destination)
               .WithMany(d => d.Flights)
               .HasForeignKey(f => f.DestinationId);

            modelBuilder.Entity<Destination>().HasData(
                new Destination { Id = 1, Name = "John F. Kennedy (JFK), New York" },
                new Destination { Id = 2, Name = "Los Angeles International (LAX), Los Angeles" },
                new Destination { Id = 3, Name = "Hartsfield-Jackson Atlanta (ATL), Atlanta" },
                new Destination { Id = 4, Name = "Heathrow Airport (LHR), London" },
                new Destination { Id = 5, Name = "Charles de Gaulle (CDG), Paris" },
                new Destination { Id = 6, Name = "Tokyo Haneda (HND), Tokyo" },
                new Destination { Id = 7, Name = "Dubai International (DXB), Dubai" },
                new Destination { Id = 8, Name = "Sydney Airport (SYD), Sydney" },
                new Destination { Id = 9, Name = "Changi Airport (SIN), Singapore" },
                new Destination { Id = 10, Name = "Hong Kong International (HKG), Hong Kong" }
            );
        }
    }
}
