using Flight_Scheduler.Controllers;
using Flight_Scheduler.Data;
using Flight_Scheduler.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Flight_Scheduler.Tests.Controllers
{
    [TestFixture]
    public class FlightsControllerTests
    {
        private FlightsController controller;
        private Flight_SchedulerContext context;
        private ITempDataDictionary tempData;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<Flight_SchedulerContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            context = new Flight_SchedulerContext(options);

            // Initialize TempData
            tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            SeedTestData();

            controller = new FlightsController(context)
            {
                TempData = tempData
            };
        }

        private void SeedTestData()
        {
            var aircraft = new Aircraft { Id = 1, Model = "Boeing 737", CrewCapacity = 5, PassengerCapacity = 150, FuelTankCapacity = 20000 };
            var airline = new Airline { Id = 1, Name = "TestAir", Country = "TestLand" };
            var airport1 = new Airport { Id = 1, Name = "Test Origin" };
            var airport2 = new Airport { Id = 2, Name = "Test Destination" };
            var crew1 = new FlightCrew { Id = 1, FirstName = "John", LastName = "Doe", Age = 35, IsAvailable = true, Position = FlightCrew.CrewPosition.Captain };
            var crew2 = new FlightCrew { Id = 2, FirstName = "Jane", LastName = "Smith", Age = 30, IsAvailable = true, Position = FlightCrew.CrewPosition.FirstOfficer };

            context.Aircrafts.Add(aircraft);
            context.Airline.Add(airline);
            context.Airports.AddRange(airport1, airport2);
            context.FlightCrews.AddRange(crew1, crew2);
            context.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            context?.Dispose();
            controller?.Dispose();
        }

        [Test]
        public async Task IndexShouldReturnViewWithFlights()
        {
            var result = await controller.Index();

            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public async Task DetailsShouldReturnNotFoundWhenIdIsNull()
        {
            var result = await controller.Details(null);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task DetailsShouldReturnNotFoundWhenFlightDoesNotExist()
        {
            var result = await controller.Details(-1);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task DetailsShouldReturnViewWhenFlightExists()
        {
            var flight = new Flight
            {
                Id = 10,
                OriginId = 1,
                DestinationId = 2,
                DepartureTime = DateTime.Now,
                ArrivalTime = DateTime.Now.AddHours(2),
                AirlineId = 1,
                AircraftId = 1,
                Gate = "A1"
            };

            context.Flight.Add(flight);
            await context.SaveChangesAsync();

            var result = await controller.Details(flight.Id);

            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public async Task CreateShouldRedirectToIndexWhenPrerequisiteDataMissing()
        {
            // Remove all prerequisite data
            context.Aircrafts.RemoveRange(context.Aircrafts);
            context.Airline.RemoveRange(context.Airline);
            context.FlightCrews.RemoveRange(context.FlightCrews);
            await context.SaveChangesAsync();

            var result = await controller.Create();

            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirect = result as RedirectToActionResult;
            Assert.That(redirect.ActionName, Is.EqualTo("Index"));
            Assert.That(controller.TempData["CreateDisabledReason"]?.ToString(), Is.EqualTo("To create a flight, at least one aircraft, airline, and crewmember must exist."));
        }

        [Test]
        public async Task CreateShouldReturnViewWhenDataIsValid()
        {
            // Adding necessary prerequisite data for a valid creation
            var aircraft = new Aircraft { Id = 2, Model = "Boeing 747", CrewCapacity = 10, PassengerCapacity = 200, FuelTankCapacity = 30000 };
            var airline = new Airline { Id = 2, Name = "TestAirline2", Country = "TestCountry2" };
            var crew1 = new FlightCrew { Id = 3, FirstName = "Mike", LastName = "Johnson", Age = 40, IsAvailable = true, Position = FlightCrew.CrewPosition.Captain };

            context.Aircrafts.Add(aircraft);
            context.Airline.Add(airline);
            context.FlightCrews.Add(crew1);
            await context.SaveChangesAsync();

            var result = await controller.Create();

            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public async Task EditShouldReturnNotFoundWhenIdIsNull()
        {
            var result = await controller.Edit(null);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task EditShouldReturnViewWhenFlightExists()
        {
            var flight = new Flight
            {
                Id = 1,
                OriginId = 1,
                DestinationId = 2,
                DepartureTime = DateTime.Now,
                ArrivalTime = DateTime.Now.AddHours(2),
                AirlineId = 1,
                AircraftId = 1,
                Gate = "A1"
            };

            context.Flight.Add(flight);
            await context.SaveChangesAsync();

            var result = await controller.Edit(flight.Id);

            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public async Task DeleteShouldReturnNotFoundWhenIdIsInvalid()
        {
            var result = await controller.Delete(-1);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task DeleteShouldReturnViewWhenFlightExists()
        {
            var flight = new Flight
            {
                Id = 10,
                OriginId = 1,
                DestinationId = 2,
                DepartureTime = DateTime.Now,
                ArrivalTime = DateTime.Now.AddHours(2),
                AirlineId = 1,
                AircraftId = 1,
                Gate = "A1"
            };

            context.Flight.Add(flight);
            await context.SaveChangesAsync();

            var result = await controller.Delete(flight.Id);

            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public async Task DeleteConfirmedShouldRedirectToIndexWhenFlightExists()
        {
            var flight = new Flight
            {
                Id = 10,
                OriginId = 1,
                DestinationId = 2,
                DepartureTime = DateTime.Now,
                ArrivalTime = DateTime.Now.AddHours(2),
                AirlineId = 1,
                AircraftId = 1,
                Gate = "A1"
            };

            context.Flight.Add(flight);
            await context.SaveChangesAsync();

            var result = await controller.DeleteConfirmed(flight.Id);

            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirect = result as RedirectToActionResult;
            Assert.That(redirect.ActionName, Is.EqualTo("Index"));
        }
    }
}
