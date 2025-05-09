using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Flight_Scheduler.Controllers;
using Flight_Scheduler.Data;
using Flight_Scheduler.Models;
using System;
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
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
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
                Model = "Boeing 737",
                CrewCapacity = 6,
                PassengerCapacity = 180,
                FuelTankCapacity = 24210
            };
            _context.Aircrafts.Add(aircraft);
            _context.SaveChanges();
        }

        private Aircraft CreateTestAircraft(int id = 2)
        {
            return new Aircraft
            {
                Id = id,
                Model = "Test Aircraft",
                CrewCapacity = 5,
                PassengerCapacity = 150,
                FuelTankCapacity = 20000
            };
        }

        [Test]
        public async Task Index_ReturnsViewWithAircraftList()
        {
            var result = await _controller.Index();

            Assert.That(result, Is.TypeOf<ViewResult>());
            var viewResult = result as ViewResult;
            var model = viewResult?.Model as List<Aircraft>;

            Assert.Multiple(() =>
            {
                Assert.That(model, Is.Not.Null);
                Assert.That(model.Count, Is.EqualTo(1));
                Assert.That(model[0].Model, Is.EqualTo("Boeing 737"));
            });
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
        public async Task Details_WithValidId_ReturnsView()
        {
            var result = await _controller.Details(1);

            Assert.That(result, Is.TypeOf<ViewResult>());
            var viewResult = result as ViewResult;
            var aircraft = viewResult?.Model as Aircraft;

            Assert.Multiple(() =>
            {
                Assert.That(aircraft, Is.Not.Null);
                Assert.That(aircraft.Id, Is.EqualTo(1));
                Assert.That(aircraft.Model, Is.EqualTo("Boeing 737"));
            });
        }

        [Test]
        public void Create_Get_ReturnsView()
        {
            var result = _controller.Create();
            Assert.That(result, Is.TypeOf<ViewResult>());
        }

        [Test]
        public async Task Create_Post_WithValidModel_RedirectsToIndex()
        {
            var aircraft = CreateTestAircraft();
            _controller.ModelState.Clear();

            var result = await _controller.Create(aircraft);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult?.ActionName, Is.EqualTo("Index"));

            var savedAircraft = await _context.Aircrafts.FindAsync(aircraft.Id);
            Assert.That(savedAircraft, Is.Not.Null);
        }

        [Test]
        public async Task Create_Post_WithInvalidModel_ReturnsView()
        {
            var aircraft = CreateTestAircraft();
            _controller.ModelState.AddModelError("Error", "Test Error");

            var result = await _controller.Create(aircraft);

            Assert.That(result, Is.TypeOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult?.Model, Is.EqualTo(aircraft));
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
        public async Task Edit_Get_WithValidId_ReturnsView()
        {
            var result = await _controller.Edit(1);

            Assert.That(result, Is.TypeOf<ViewResult>());
            var viewResult = result as ViewResult;
            var aircraft = viewResult?.Model as Aircraft;
            Assert.That(aircraft?.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task Edit_Post_WithMismatchedId_ReturnsNotFound()
        {
            var aircraft = CreateTestAircraft(1);
            var result = await _controller.Edit(2, aircraft);
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task Edit_Post_WithValidModel_RedirectsToIndex()
        {
            var aircraft = await _context.Aircrafts.FindAsync(1);
            aircraft.Model = "Updated Model";
            _controller.ModelState.Clear();

            var result = await _controller.Edit(1, aircraft);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult?.ActionName, Is.EqualTo("Index"));

            var updatedAircraft = await _context.Aircrafts.FindAsync(1);
            Assert.That(updatedAircraft?.Model, Is.EqualTo("Updated Model"));
        }

        [Test]
        public async Task Edit_Post_WithInvalidModel_ReturnsView()
        {
            var aircraft = await _context.Aircrafts.FindAsync(1);
            _controller.ModelState.AddModelError("Error", "Test Error");

            var result = await _controller.Edit(1, aircraft);

            Assert.That(result, Is.TypeOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult?.Model, Is.EqualTo(aircraft));
        }

        [Test]
        public async Task Delete_Get_WithNullId_ReturnsNotFound()
        {
            var result = await _controller.Delete(null);
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task Delete_Get_WithInvalidId_ReturnsNotFound()
        {
            var result = await _controller.Delete(999);
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task Delete_Get_WithValidId_ReturnsView()
        {
            var result = await _controller.Delete(1);

            Assert.That(result, Is.TypeOf<ViewResult>());
            var viewResult = result as ViewResult;
            var aircraft = viewResult?.Model as Aircraft;
            Assert.That(aircraft?.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task DeleteConfirmed_WithValidId_RedirectsToIndex()
        {
            var result = await _controller.DeleteConfirmed(1);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult?.ActionName, Is.EqualTo("Index"));
            Assert.That(await _context.Aircrafts.FindAsync(1), Is.Null);
        }

        [Test]
        public async Task DeleteConfirmed_WithInvalidId_RedirectsToIndex()
        {
            var result = await _controller.DeleteConfirmed(999);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult?.ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public void AircraftExists_WithExistingId_ReturnsTrue()
        {
            var result = _context.Aircrafts.Any(e => e.Id == 1);
            Assert.That(result, Is.True);
        }

        [Test]
        public void AircraftExists_WithNonExistingId_ReturnsFalse()
        {
            var result = _context.Aircrafts.Any(e => e.Id == 999);
            Assert.That(result, Is.False);
        }
    }
}