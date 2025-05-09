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
using static Flight_Scheduler.Models.FlightCrew;

namespace Flight_Scheduler.Tests
{
    [TestFixture]
    public class FlightCrewsControllerTests
    {
        private Flight_SchedulerContext _context;
        private FlightCrewsController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<Flight_SchedulerContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new Flight_SchedulerContext(options);
            SeedTestData();
            _controller = new FlightCrewsController(_context);
        }

        private void SeedTestData()
        {
            var flightCrew = new FlightCrew
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Age = 35,
                Position = CrewPosition.Captain,
                IsAvailable = true
            };
            _context.FlightCrews.Add(flightCrew);
            _context.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
            _controller?.Dispose();
        }

        private FlightCrew CreateTestCrew(int id = 2)
        {
            return new FlightCrew
            {
                Id = id,
                FirstName = "Test",
                LastName = "Crew",
                Age = 30,
                Position = CrewPosition.FirstOfficer,
                IsAvailable = true
            };
        }

        [Test]
        public async Task Index_ReturnsViewWithFlightCrews()
        {
            var result = await _controller.Index();

            Assert.That(result, Is.TypeOf<ViewResult>());
            var viewResult = result as ViewResult;
            var model = viewResult?.Model as List<FlightCrew>;

            Assert.Multiple(() =>
            {
                Assert.That(model, Is.Not.Null);
                Assert.That(model.Count, Is.EqualTo(1));
                Assert.That(model[0].FirstName, Is.EqualTo("John"));
                Assert.That(model[0].LastName, Is.EqualTo("Doe"));
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
        public async Task Details_WithValidId_ReturnsViewWithCrew()
        {
            var result = await _controller.Details(1);

            Assert.That(result, Is.TypeOf<ViewResult>());
            var viewResult = result as ViewResult;
            var crew = viewResult?.Model as FlightCrew;

            Assert.Multiple(() =>
            {
                Assert.That(crew, Is.Not.Null);
                Assert.That(crew.Id, Is.EqualTo(1));
                Assert.That(crew.FirstName, Is.EqualTo("John"));
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
            var crew = CreateTestCrew();
            _controller.ModelState.Clear();

            var result = await _controller.Create(crew);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult?.ActionName, Is.EqualTo("Index"));

            var savedCrew = await _context.FlightCrews.FindAsync(crew.Id);
            Assert.That(savedCrew, Is.Not.Null);
        }

        [Test]
        public async Task Create_Post_WithInvalidModel_ReturnsView()
        {
            var crew = CreateTestCrew();
            _controller.ModelState.AddModelError("Error", "Test Error");

            var result = await _controller.Create(crew);

            Assert.That(result, Is.TypeOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult?.Model, Is.EqualTo(crew));
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
            var crew = viewResult?.Model as FlightCrew;
            Assert.That(crew?.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task Edit_Post_WithMismatchedId_ReturnsNotFound()
        {
            var crew = CreateTestCrew(1);
            var result = await _controller.Edit(2, crew);
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task Edit_Post_WithValidModel_RedirectsToIndex()
        {
            var crew = await _context.FlightCrews.FindAsync(1);
            crew.FirstName = "Updated";
            _controller.ModelState.Clear();

            var result = await _controller.Edit(1, crew);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult?.ActionName, Is.EqualTo("Index"));

            var updatedCrew = await _context.FlightCrews.FindAsync(1);
            Assert.That(updatedCrew?.FirstName, Is.EqualTo("Updated"));
        }

        [Test]
        public async Task Edit_Post_WithInvalidModel_ReturnsView()
        {
            var crew = await _context.FlightCrews.FindAsync(1);
            _controller.ModelState.AddModelError("Error", "Test Error");

            var result = await _controller.Edit(1, crew);

            Assert.That(result, Is.TypeOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult?.Model, Is.EqualTo(crew));
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
            var crew = viewResult?.Model as FlightCrew;
            Assert.That(crew?.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task DeleteConfirmed_WithValidId_RedirectsToIndex()
        {
            var result = await _controller.DeleteConfirmed(1);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult?.ActionName, Is.EqualTo("Index"));
            Assert.That(await _context.FlightCrews.FindAsync(1), Is.Null);
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
        public void FlightCrewExists_WithExistingId_ReturnsTrue()
        {
            var result = _context.FlightCrews.Any(e => e.Id == 1);
            Assert.That(result, Is.True);
        }

        [Test]
        public void FlightCrewExists_WithNonExistingId_ReturnsFalse()
        {
            var result = _context.FlightCrews.Any(e => e.Id == 999);
            Assert.That(result, Is.False);
        }
    }
}