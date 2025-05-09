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
        private Flight_SchedulerContext _context;
        private AirlinesController _controller;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<Flight_SchedulerContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new Flight_SchedulerContext(options);
            _context.Database.EnsureCreated();

            _context.Airline.AddRange(
                new Airline { Id = 1, Name = "TestAir", Country = "TestLand" },
                new Airline { Id = 2, Name = "SampleAir", Country = "SampleLand" }
            );
            _context.SaveChanges();

            _controller = new AirlinesController(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
            _controller?.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewWithAirlines()
        {
            var result = await _controller.Index();
            Assert.IsInstanceOf<ViewResult>(result);

            var view = result as ViewResult;
            Assert.IsInstanceOf<List<Airline>>(view.Model);
            var model = view.Model as List<Airline>;
            Assert.AreEqual(2, model.Count);
        }

        [Test]
        public async Task Details_ReturnsNotFound_WhenIdIsNull()
        {
            var result = await _controller.Details(null);
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task Details_ReturnsNotFound_WhenAirlineDoesNotExist()
        {
            var result = await _controller.Details(999);
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task Details_ReturnsViewWithAirline_WhenExists()
        {
            var result = await _controller.Details(1);
            Assert.IsInstanceOf<ViewResult>(result);

            var view = result as ViewResult;
            var model = view.Model as Airline;
            Assert.NotNull(model);
            Assert.AreEqual(1, model.Id);
        }

        [Test]
        public void Create_Get_ReturnsView()
        {
            var result = _controller.Create();
            Assert.IsInstanceOf<ViewResult>(result);
        }

        [Test]
        public async Task Create_Post_RedirectsToIndex_WhenModelIsValid()
        {
            var airline = new Airline { Name = "NewAir", Country = "NewLand" };

            var result = await _controller.Create(airline);
            Assert.IsInstanceOf<RedirectToActionResult>(result);

            var redirect = result as RedirectToActionResult;
            Assert.AreEqual("Index", redirect.ActionName);
        }

        [Test]
        public async Task Create_Post_ReturnsView_WhenModelIsInvalid()
        {
            _controller.ModelState.AddModelError("Name", "Required");

            var result = await _controller.Create(new Airline());
            Assert.IsInstanceOf<ViewResult>(result);
        }

        [Test]
        public async Task Edit_Get_ReturnsNotFound_WhenIdIsNull()
        {
            var result = await _controller.Edit(null);
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task Edit_Get_ReturnsNotFound_WhenNotExists()
        {
            var result = await _controller.Edit(999);
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task Edit_Get_ReturnsView_WhenExists()
        {
            var result = await _controller.Edit(1);
            Assert.IsInstanceOf<ViewResult>(result);

            var view = result as ViewResult;
            Assert.IsInstanceOf<Airline>(view.Model);
        }

        [Test]
        public async Task Edit_Post_ReturnsNotFound_WhenIdsMismatch()
        {
            var result = await _controller.Edit(999, new Airline { Id = 1 });
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task Edit_Post_RedirectsToIndex_WhenValid()
        {
            var tracked = await _context.Airline.FirstAsync(a => a.Id == 1);

            _context.Entry(tracked).State = EntityState.Detached;

            var updated = new Airline { Id = 1, Name = "UpdatedAir", Country = "UpdatedLand" };
            var result = await _controller.Edit(1, updated);

            Assert.IsInstanceOf<RedirectToActionResult>(result);
            var redirect = result as RedirectToActionResult;
            Assert.AreEqual("Index", redirect.ActionName);

            var fromDb = await _context.Airline.FindAsync(1);
            Assert.AreEqual("UpdatedAir", fromDb.Name);
            Assert.AreEqual("UpdatedLand", fromDb.Country);
        }

        [Test]
        public async Task Edit_Post_ReturnsView_WhenModelIsInvalid()
        {
            _controller.ModelState.AddModelError("Name", "Required");
            var result = await _controller.Edit(1, new Airline { Id = 1 });
            Assert.IsInstanceOf<ViewResult>(result);
        }

        [Test]
        public async Task Delete_Get_ReturnsNotFound_WhenIdIsNull()
        {
            var result = await _controller.Delete(null);
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task Delete_Get_ReturnsNotFound_WhenAirlineNotFound()
        {
            var result = await _controller.Delete(999);
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task Delete_Get_ReturnsView_WhenAirlineExists()
        {
            var result = await _controller.Delete(1);
            Assert.IsInstanceOf<ViewResult>(result);

            var view = result as ViewResult;
            Assert.IsInstanceOf<Airline>(view.Model);
        }

        [Test]
        public async Task DeleteConfirmed_RemovesAirline_AndRedirectsToIndex()
        {
            var result = await _controller.DeleteConfirmed(1);
            Assert.IsInstanceOf<RedirectToActionResult>(result);

            var redirect = result as RedirectToActionResult;
            Assert.AreEqual("Index", redirect.ActionName);

            Assert.IsNull(await _context.Airline.FindAsync(1));
        }
    }
}
