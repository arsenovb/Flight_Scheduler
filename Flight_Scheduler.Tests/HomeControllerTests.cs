using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Flight_Scheduler.Controllers;
using Flight_Scheduler.Data;
using Flight_Scheduler.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Flight_Scheduler.Tests
{
    [TestFixture]
    public class HomeControllerTests
    {
        private Flight_SchedulerContext _context;
        private HomeController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<Flight_SchedulerContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{System.Guid.NewGuid()}")
                .Options;

            _context = new Flight_SchedulerContext(options);
            SeedTestData();
            _controller = new HomeController(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
            _controller?.Dispose();
        }

        private void SeedTestData()
        {
            var originAirport = new Airport { Id = 1, Name = "JFK" };
            var destinationAirport = new Airport { Id = 2, Name = "LAX" };

            var aircraft = new Aircraft
            {
                Id = 1,
                Model = "Boeing 737",
                CrewCapacity = 5,
                PassengerCapacity = 150,
                FuelTankCapacity = 20000
            };

            var airline = new Airline
            {
                Id = 1,
                Name = "Delta",
                Country = "USA"
            };

            var flight = new Flight
            {
                Id = 1,
                Origin = originAirport,
                Destination = destinationAirport,
                Aircraft = aircraft,
                Airlines = airline,
                DepartureTime = System.DateTime.Now,
                ArrivalTime = System.DateTime.Now.AddHours(5),
                Gate = "A1"
            };

            _context.Flight.Add(flight);
            _context.SaveChanges();
        }

        [Test]
        public async Task IndexShouldReturnViewResultWithListOfFlights()
        {
            var result = await _controller.Index();

            Assert.That(result, Is.TypeOf<ViewResult>());

            var viewResult = result as ViewResult;
            Assert.That(viewResult?.Model, Is.TypeOf<List<Flight>>());

            var model = viewResult.Model as List<Flight>;
            Assert.That(model?.Count, Is.EqualTo(1));
        }

        [Test]
        public void PrivacyShouldReturnViewResult()
        {
            var result = _controller.Privacy();
            Assert.That(result, Is.TypeOf<ViewResult>());
        }

        [Test]
        public void Error_ShouldReturnViewResultWithErrorViewModel()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = _controller.Error();

            Assert.That(result, Is.TypeOf<ViewResult>());

            var viewResult = result as ViewResult;
            Assert.That(viewResult?.Model, Is.TypeOf<ErrorViewModel>());

            var model = viewResult.Model as ErrorViewModel;
            Assert.That(model?.RequestId, Is.Not.Null.And.Not.Empty);
        }
    }
}
