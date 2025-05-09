using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Flight_Scheduler.Controllers;
using Flight_Scheduler.Data;
using Flight_Scheduler.Models;
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
                .UseInMemoryDatabase(databaseName: $"TestDb_{System.Guid.NewGuid()}")
                .Options;

            _context = new Flight_SchedulerContext(options);
            SeedTestData();
            _controller = new FlightCrewsController(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
            _controller?.Dispose();
        }
        private void SeedTestData()
        {
            _context.FlightCrews.Add(new FlightCrew
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Age = 35,
                Position = CrewPosition.Captain,
                IsAvailable = true
            }) ;
            _context.SaveChanges();
        }

        [Test]
        public async Task IndexReturnsViewWithFlightCrewList()
        {
            var result = await _controller.Index();

            Assert.That(result, Is.TypeOf<ViewResult>());
            var viewResult = result as ViewResult;

            Assert.That(viewResult?.Model, Is.TypeOf<List<FlightCrew>>());
            var model = viewResult?.Model as List<FlightCrew>;

            Assert.That(model?.Count, Is.EqualTo(1));
            Assert.That(model[0].FirstName, Is.EqualTo("John"));
        }

        [Test]
        public async Task DetailsValidIdReturnsViewWithFlightCrew()
        {
            var result = await _controller.Details(1);

            Assert.That(result, Is.TypeOf<ViewResult>());
            var viewResult = result as ViewResult;

            Assert.That(viewResult?.Model, Is.TypeOf<FlightCrew>());
            var model = viewResult?.Model as FlightCrew;

            Assert.That(model?.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task DetailsNullIdReturnsNotFound()
        {
            var result = await _controller.Details(null);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task DetailsInvalidIdReturnsNotFound()
        {
            var result = await _controller.Details(999);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public void CreateGetReturnsView()
        {
            var result = _controller.Create();

            Assert.That(result, Is.TypeOf<ViewResult>());
        }

        [Test]
        public async Task CreatePostValidDataRedirectsToIndex()
        {
            var newCrew = new FlightCrew
            {
                Id = 2,
                FirstName = "Alice",
                LastName = "Smith",
                Age = 29,
                Position = CrewPosition.FlightAttendant,
                IsAvailable = true
            };

            var result = await _controller.Create(newCrew);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            Assert.That(_context.FlightCrews.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task CreatePostInvalidModelReturnsView()
        {
            _controller.ModelState.AddModelError("FirstName", "Required");

            var invalidCrew = new FlightCrew();

            var result = await _controller.Create(invalidCrew);

            Assert.That(result, Is.TypeOf<ViewResult>());
        }

        [Test]
        public async Task EditGetValidIdReturnsViewWithModel()
        {
            var result = await _controller.Edit(1);

            Assert.That(result, Is.TypeOf<ViewResult>());
            var model = (result as ViewResult)?.Model as FlightCrew;

            Assert.That(model?.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task EditGetInvalidIdReturnsNotFound()
        {
            var result = await _controller.Edit(999);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task EditPostValidDataUpdatesCrewAndRedirects()
        {
            var crew = _context.FlightCrews.First();
            crew.FirstName = "Updated";

            var result = await _controller.Edit(crew.Id, crew);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            var updated = _context.FlightCrews.Find(crew.Id);
            Assert.That(updated?.FirstName, Is.EqualTo("Updated"));
        }

        [Test]
        public async Task EditPostInvalidIdReturnsNotFound()
        {
            var crew = new FlightCrew { Id = 5 };

            var result = await _controller.Edit(1, crew);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task EditPostInvalidModelReturnsView()
        {
            var crew = _context.FlightCrews.First();
            _controller.ModelState.AddModelError("LastName", "Required");

            var result = await _controller.Edit(crew.Id, crew);

            Assert.That(result, Is.TypeOf<ViewResult>());
        }

        [Test]
        public async Task DeleteGetValidIdReturnsView()
        {
            var result = await _controller.Delete(1);

            Assert.That(result, Is.TypeOf<ViewResult>());
            var model = (result as ViewResult)?.Model as FlightCrew;
            Assert.That(model?.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task DeleteGetInvalidIdReturnsNotFound()
        {
            var result = await _controller.Delete(999);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task DeleteConfirmedRemovesCrewAndRedirects()
        {
            var result = await _controller.DeleteConfirmed(1);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            Assert.That(_context.FlightCrews.Count(), Is.EqualTo(0));
        }
    }
}
