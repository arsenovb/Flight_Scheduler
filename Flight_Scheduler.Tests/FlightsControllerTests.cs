using Flight_Scheduler.Controllers;
using Flight_Scheduler.Data;
using Flight_Scheduler.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Flight_Scheduler.Tests.Controllers
{
    [TestFixture]
    public class FlightsControllerTests
    {
        private FlightsController _controller;
        private Flight_SchedulerContext _context;
        private ITempDataDictionary _tempData;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<Flight_SchedulerContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new Flight_SchedulerContext(options);
            _tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            SeedTestData();
            _controller = new FlightsController(_context) { TempData = _tempData };
        }

        private void SeedTestData()
        {
            var aircraft = new Aircraft { Id = 1, Model = "Boeing 737", CrewCapacity = 5, PassengerCapacity = 150, FuelTankCapacity = 20000 };
            var airline = new Airline { Id = 1, Name = "TestAir", Country = "TestLand" };
            var airports = new List<Airport>
            {
                new() { Id = 1, Name = "Test Origin" },
                new() { Id = 2, Name = "Test Destination" }
            };
            var crews = new List<FlightCrew>
            {
                new() { Id = 1, FirstName = "John", LastName = "Doe", Age = 35, IsAvailable = true, Position = FlightCrew.CrewPosition.Captain },
                new() { Id = 2, FirstName = "Jane", LastName = "Smith", Age = 30, IsAvailable = true, Position = FlightCrew.CrewPosition.FirstOfficer },
                new() { Id = 3, FirstName = "Mike", LastName = "Johnson", Age = 28, IsAvailable = true, Position = FlightCrew.CrewPosition.FlightAttendant }
            };

            _context.Aircrafts.Add(aircraft);
            _context.Airline.Add(airline);
            _context.Airports.AddRange(airports);
            _context.FlightCrews.AddRange(crews);
            _context.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
            _controller?.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewWithAllFlights()
        {
            var flight = CreateTestFlight();
            _context.Flight.Add(flight);
            await _context.SaveChangesAsync();

            var result = await _controller.Index();

            Assert.That(result, Is.TypeOf<ViewResult>());
            var viewResult = result as ViewResult;
            var flights = viewResult?.Model as List<Flight>;
            Assert.That(flights?.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task Details_WithNullId_ReturnsNotFound()
        {
            var result = await _controller.Details(null);
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task Details_WithInvalidId_ReturnsNotFound()
        {
            var result = await _controller.Details(999);
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task Details_WithValidId_ReturnsViewWithFlight()
        {
            var flight = CreateTestFlight();
            _context.Flight.Add(flight);
            await _context.SaveChangesAsync();

            var result = await _controller.Details(flight.Id);

            Assert.That(result, Is.TypeOf<ViewResult>());
            var viewResult = result as ViewResult;
            var model = viewResult?.Model as Flight;
            Assert.That(model?.Id, Is.EqualTo(flight.Id));
        }

        [Test]
        public async Task Create_Get_WithMissingPrerequisites_RedirectsToIndex()
        {
            _context.Aircrafts.RemoveRange(_context.Aircrafts);
            await _context.SaveChangesAsync();

            var result = await _controller.Create();

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            Assert.That(_tempData["CreateDisabledReason"], Is.Not.Null);
        }

        [Test]
        public async Task Create_Get_WithValidPrerequisites_ReturnsView()
        {
            var result = await _controller.Create();

            Assert.That(result, Is.TypeOf<ViewResult>());
            Assert.Multiple(() =>
            {
                Assert.That(_controller.ViewData["AircraftId"], Is.Not.Null);
                Assert.That(_controller.ViewData["AirlineId"], Is.Not.Null);
                Assert.That(_controller.ViewData["OriginId"], Is.Not.Null);
                Assert.That(_controller.ViewData["DestinationId"], Is.Not.Null);
                Assert.That(_controller.ViewBag.Captains, Is.Not.Null);
                Assert.That(_controller.ViewBag.FirstOfficers, Is.Not.Null);
                Assert.That(_controller.ViewBag.OtherCrew, Is.Not.Null);
            });
        }

        [Test]
        public async Task Create_Post_WithValidModel_RedirectsToIndex()
        {
            var flight = CreateTestFlight();
            var crewIds = new List<int> { 1, 2 };

            var result = await _controller.Create(flight, crewIds);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            var savedFlight = await _context.Flight
                .Include(f => f.FlightCrews)
                .FirstOrDefaultAsync(f => f.Id == flight.Id);

            Assert.Multiple(() =>
            {
                Assert.That(savedFlight, Is.Not.Null);
                Assert.That(savedFlight.FlightCrews.Count, Is.EqualTo(2));
                Assert.That(savedFlight.FlightCrews.Any(c => !c.IsAvailable), Is.True);
            });
        }

        [Test]
        public async Task Create_Post_WithSameOriginDestination_ReturnsViewWithError()
        {
            var flight = CreateTestFlight();
            flight.DestinationId = flight.OriginId;
            var crewIds = new List<int> { 1, 2 };

            var result = await _controller.Create(flight, crewIds);

            Assert.That(result, Is.TypeOf<ViewResult>());
            Assert.That(_controller.ModelState.IsValid, Is.False);
            Assert.That(_controller.ModelState["DestinationId"].Errors[0].ErrorMessage,
                Is.EqualTo("Origin and destination airports must be different."));
        }

        [Test]
        public async Task Create_Post_WithInvalidAircraft_ReturnsViewWithError()
        {
            var flight = CreateTestFlight();
            flight.AircraftId = 999;
            var crewIds = new List<int> { 1, 2 };

            var result = await _controller.Create(flight, crewIds);

            Assert.That(result, Is.TypeOf<ViewResult>());
            Assert.That(_controller.ModelState["AircraftId"].Errors[0].ErrorMessage,
                Is.EqualTo("Aircraft not found."));
        }

        [Test]
        public async Task Edit_Get_WithNullId_ReturnsNotFound()
        {
            var result = await _controller.Edit(null);
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task Edit_Get_WithInvalidId_ReturnsNotFound()
        {
            var result = await _controller.Edit(999);
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task Edit_Get_WithValidId_ReturnsViewAndResetsCrewAvailability()
        {
            var flight = CreateTestFlight();
            var crew = await _context.FlightCrews.FindAsync(1);
            crew.IsAvailable = false;
            flight.FlightCrews.Add(crew);
            _context.Flight.Add(flight);
            await _context.SaveChangesAsync();

            var result = await _controller.Edit(flight.Id);

            Assert.That(result, Is.TypeOf<ViewResult>());
            Assert.That(crew.IsAvailable, Is.True);
        }

        [Test]
        public async Task Edit_Post_WithNonMatchingId_ReturnsNotFound()
        {
            var flight = CreateTestFlight();
            var result = await _controller.Edit(2, flight, new List<int>());
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task Edit_Post_WithValidModel_UpdatesFlightAndRedirects()
        {
            var flight = CreateTestFlight();
            _context.Flight.Add(flight);
            await _context.SaveChangesAsync();

            flight.Gate = "B2";
            var crewIds = new List<int> { 1, 2 };

            var result = await _controller.Edit(flight.Id, flight, crewIds);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            var updatedFlight = await _context.Flight.FindAsync(flight.Id);
            Assert.That(updatedFlight?.Gate, Is.EqualTo("B2"));
        }

        [Test]
        public async Task Delete_Get_WithNullId_ReturnsNotFound()
        {
            var result = await _controller.Delete(null);
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task Delete_Get_WithValidId_ReturnsView()
        {
            var flight = CreateTestFlight();
            _context.Flight.Add(flight);
            await _context.SaveChangesAsync();

            var result = await _controller.Delete(flight.Id);

            Assert.That(result, Is.TypeOf<ViewResult>());
            var model = (result as ViewResult)?.Model as Flight;
            Assert.That(model?.Id, Is.EqualTo(flight.Id));
        }

        [Test]
        public async Task DeleteConfirmed_ResetsCrewAvailabilityAndDeletesFlight()
        {
            var flight = CreateTestFlight();
            var crew = await _context.FlightCrews.FindAsync(1);
            crew.IsAvailable = false;
            flight.FlightCrews.Add(crew);
            _context.Flight.Add(flight);
            await _context.SaveChangesAsync();

            var result = await _controller.DeleteConfirmed(flight.Id);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            Assert.Multiple(async () =>
            {
                Assert.That(await _context.Flight.FindAsync(flight.Id), Is.Null);
                Assert.That(crew.IsAvailable, Is.True);
            });
        }

        private Flight CreateTestFlight()
        {
            return new Flight
            {
                Id = 1,
                OriginId = 1,
                DestinationId = 2,
                DepartureTime = DateTime.Now,
                ArrivalTime = DateTime.Now.AddHours(2),
                AirlineId = 1,
                AircraftId = 1,
                Gate = "A1",
                FlightCrews = new List<FlightCrew>()
            };
        }
    }
}