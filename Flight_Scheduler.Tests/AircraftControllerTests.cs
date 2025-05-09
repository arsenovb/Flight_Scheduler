using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Flight_Scheduler.Controllers;
using Flight_Scheduler.Data;
using Flight_Scheduler.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Flight_Scheduler.Tests
{
    [TestFixture]
    public class AircraftControllerTests
    {
        private Flight_SchedulerContext _context;
        private AircraftController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<Flight_SchedulerContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{System.Guid.NewGuid()}")
                .Options;

            _context = new Flight_SchedulerContext(options);
            SeedTestData();
            _controller = new AircraftController(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
            _controller?.Dispose();
        }

        private void SeedTestData()
        {
            var aircraft = new Aircraft
            {
                Id = 1,
                Model = "Airbus A320",
                CrewCapacity = 6,
                PassengerCapacity = 180,
                FuelTankCapacity = 24210
            };
            _context.Aircrafts.Add(aircraft);
            _context.SaveChanges();
        }

        [Test]
        public async Task IndexReturnsViewWithAircraftList()
        {
            var result = await _controller.Index();

            Assert.That(result, Is.TypeOf<ViewResult>());
            var viewResult = result as ViewResult;

            Assert.That(viewResult?.Model, Is.TypeOf<List<Aircraft>>());
            var model = viewResult?.Model as List<Aircraft>;

            Assert.That(model?.Count, Is.EqualTo(1));
            Assert.That(model?[0].Model, Is.EqualTo("Airbus A320"));
        }

        [Test]
        public async Task DetailsReturnsAircraftWhenIdIsValid()
        {
            var result = await _controller.Details(1);

            Assert.That(result, Is.TypeOf<ViewResult>());
            var viewResult = result as ViewResult;

            var aircraft = viewResult?.Model as Aircraft;
            Assert.That(aircraft, Is.Not.Null);
            Assert.That(aircraft?.Id, Is.EqualTo(1));
            Assert.That(aircraft?.Model, Is.EqualTo("Airbus A320"));
        }

        [Test]
        public async Task DetailsReturnsNotFoundWhenIdIsNull()
        {
            var result = await _controller.Details(null);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task DetailsReturnsNotFoundWhenAircraftDoesNotExist()
        {
            var result = await _controller.Details(999);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task CreatePostValidModelRedirectsToIndex()
        {
            var newAircraft = new Aircraft
            {
                Id = 2,
                Model = "Boeing 777",
                CrewCapacity = 10,
                PassengerCapacity = 300,
                FuelTankCapacity = 47000
            };

            var result = await _controller.Create(newAircraft);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            var redirect = result as RedirectToActionResult;

            Assert.That(redirect?.ActionName, Is.EqualTo("Index"));
            Assert.That(_context.Aircrafts.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task EditPostValidDataUpdatesAircraft()
        {
            var aircraft = _context.Aircrafts.First();
            aircraft.Model = "Updated Model";

            var result = await _controller.Edit(aircraft.Id, aircraft);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());

            var updatedAircraft = _context.Aircrafts.Find(aircraft.Id);
            Assert.That(updatedAircraft?.Model, Is.EqualTo("Updated Model"));
        }

        [Test]
        public async Task DeleteConfirmedRemovesAircraft()
        {
            var result = await _controller.DeleteConfirmed(1);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            Assert.That(_context.Aircrafts.Count(), Is.EqualTo(0));
        }
    }
}
