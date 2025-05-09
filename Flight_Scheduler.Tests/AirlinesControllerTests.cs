using Flight_Scheduler.Controllers;
using Flight_Scheduler.Data;
using Flight_Scheduler.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Flight_Scheduler.Tests.Controllers
{
    [TestFixture]
    public class AirlinesControllerTests
    {
        private AirlinesController _controller;
        private Flight_SchedulerContext _context;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<Flight_SchedulerContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new Flight_SchedulerContext(options);
            SeedTestData();
            _controller = new AirlinesController(_context);
        }

        private void SeedTestData()
        {
            var airline = new Airline
            {
                Id = 1,
                Name = "Test Airlines",
                Country = "Test Country"
            };
            _context.Airline.Add(airline);
            _context.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
            _controller?.Dispose();
        }

        private Airline CreateTestAirline(int id = 2)
        {
            return new Airline
            {
                Id = id,
                Name = "New Test Airlines",
                Country = "New Test Country"
            };
        }

        [Test]
        public async Task Index_ReturnsViewWithAllAirlines()
        {
            var result = await _controller.Index();

            Assert.That(result, Is.TypeOf<ViewResult>());
            var viewResult = result as ViewResult;
            var model = viewResult?.Model as List<Airline>;

            Assert.Multiple(() =>
            {
                Assert.That(model, Is.Not.Null);
                Assert.That(model.Count, Is.EqualTo(1));
                Assert.That(model[0].Name, Is.EqualTo("Test Airlines"));
                Assert.That(model[0].Country, Is.EqualTo("Test Country"));
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
        public async Task Details_WithValidId_ReturnsViewWithAirline()
        {
            var result = await _controller.Details(1);

            Assert.That(result, Is.TypeOf<ViewResult>());
            var viewResult = result as ViewResult;
            var airline = viewResult?.Model as Airline;

            Assert.Multiple(() =>
            {
                Assert.That(airline, Is.Not.Null);
                Assert.That(airline.Id, Is.EqualTo(1));
                Assert.That(airline.Name, Is.EqualTo("Test Airlines"));
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
            var airline = CreateTestAirline();
            _controller.ModelState.Clear();

            var result = await _controller.Create(airline);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult?.ActionName, Is.EqualTo("Index"));

            var savedAirline = await _context.Airline.FindAsync(airline.Id);
            Assert.That(savedAirline, Is.Not.Null);
        }

        [Test]
        public async Task Create_Post_WithInvalidModel_ReturnsView()
        {
            var airline = CreateTestAirline();
            _controller.ModelState.AddModelError("Error", "Test Error");

            var result = await _controller.Create(airline);

            Assert.That(result, Is.TypeOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult?.Model, Is.EqualTo(airline));
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
            var airline = viewResult?.Model as Airline;
            Assert.That(airline?.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task Edit_Post_WithMismatchedId_ReturnsNotFound()
        {
            var airline = CreateTestAirline(1);
            var result = await _controller.Edit(2, airline);
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task Edit_Post_WithValidModel_RedirectsToIndex()
        {
            var airline = await _context.Airline.FindAsync(1);
            airline.Name = "Updated Name";
            _controller.ModelState.Clear();

            var result = await _controller.Edit(1, airline);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult?.ActionName, Is.EqualTo("Index"));

            var updatedAirline = await _context.Airline.FindAsync(1);
            Assert.That(updatedAirline?.Name, Is.EqualTo("Updated Name"));
        }

        [Test]
        public async Task Edit_Post_WithInvalidModel_ReturnsView()
        {
            var airline = await _context.Airline.FindAsync(1);
            _controller.ModelState.AddModelError("Error", "Test Error");

            var result = await _controller.Edit(1, airline);

            Assert.That(result, Is.TypeOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult?.Model, Is.EqualTo(airline));
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
            var airline = viewResult?.Model as Airline;
            Assert.That(airline?.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task DeleteConfirmed_WithValidId_RedirectsToIndex()
        {
            var result = await _controller.DeleteConfirmed(1);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult?.ActionName, Is.EqualTo("Index"));
            Assert.That(await _context.Airline.FindAsync(1), Is.Null);
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
        public void AirlineExists_WithExistingId_ReturnsTrue()
        {
            var result = _context.Airline.Any(e => e.Id == 1);
            Assert.That(result, Is.True);
        }

        [Test]
        public void AirlineExists_WithNonExistingId_ReturnsFalse()
        {
            var result = _context.Airline.Any(e => e.Id == 999);
            Assert.That(result, Is.False);
        }
    }
}