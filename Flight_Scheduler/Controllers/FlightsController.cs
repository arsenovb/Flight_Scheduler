using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Flight_Scheduler.Data;
using Flight_Scheduler.Models;

namespace Flight_Scheduler.Controllers
{
    public class FlightsController : Controller
    {
        private readonly Flight_SchedulerContext _context;

        public FlightsController(Flight_SchedulerContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var flights = await _context.Flight
                .Include(f => f.Aircraft)
                .Include(f => f.Airlines)
                .Include(f => f.Origin)
                .Include(f => f.Destination)
                .ToListAsync();

            return View(flights);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var flight = await _context.Flight
                .Include(f => f.Origin)
                .Include(f => f.Destination)
                .Include(f => f.Airlines)
                .Include(f => f.Aircraft)
                .Include(f => f.FlightCrews)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (flight == null) return NotFound();

            return View(flight);
        }

        public async Task<IActionResult> Create()
        {
            if (!await _context.Aircrafts.AnyAsync() ||
                !await _context.Airline.AnyAsync() ||
                !await _context.FlightCrews.AnyAsync())
            {
                TempData["CreateDisabledReason"] = "To create a flight, at least one aircraft, airline, and crewmember must exist.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,OriginId,DestinationId,DepartureTime,ArrivalTime,Gate,AirlineId,AircraftId")] Flight flight,
            List<int> crewIds)
        {
            if (flight.OriginId == flight.DestinationId)
                ModelState.AddModelError("DestinationId", "Origin and destination airports must be different.");

            var aircraft = await _context.Aircrafts.FindAsync(flight.AircraftId);
            if (aircraft == null)
                ModelState.AddModelError("AircraftId", "Aircraft not found.");
            else if (crewIds.Count > aircraft.CrewCapacity)
                ModelState.AddModelError("", $"Too many crew members. Aircraft capacity: {aircraft.CrewCapacity}.");

            if (crewIds.Count != crewIds.Distinct().Count())
                ModelState.AddModelError("", "Duplicate crew members are not allowed.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return View(flight);
            }

            flight.FlightCrews = new List<FlightCrew>();
            foreach (var crewId in crewIds.Distinct())
            {
                var crew = await _context.FlightCrews.FindAsync(crewId);
                if (crew != null && crew.IsAvailable)
                {
                    crew.IsAvailable = false;
                    _context.Entry(crew).State = EntityState.Modified;
                    flight.FlightCrews.Add(crew);
                }
            }

            _context.Add(flight);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var flight = await _context.Flight
                .Include(f => f.FlightCrews)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (flight == null) return NotFound();

            // Temporarily mark current crew as available for re-selection
            foreach (var crew in flight.FlightCrews)
            {
                crew.IsAvailable = true;
                _context.Entry(crew).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();

            var releasedCrewIds = flight.FlightCrews.Select(fc => fc.Id).ToList();
            await PopulateDropdowns(flight, releasedCrewIds);

            return View(flight);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,OriginId,DestinationId,DepartureTime,ArrivalTime,Gate,AirlineId,AircraftId")] Flight flight,
            List<int> crewIds)
        {
            if (id != flight.Id) return NotFound();

            var existingFlight = await _context.Flight
                .Include(f => f.FlightCrews)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (existingFlight == null) return NotFound();

            if (flight.OriginId == flight.DestinationId)
                ModelState.AddModelError("DestinationId", "Origin and destination airports must be different.");

            var aircraft = await _context.Aircrafts.FindAsync(flight.AircraftId);
            if (aircraft == null)
                ModelState.AddModelError("AircraftId", "Aircraft not found.");
            else if (crewIds.Count > aircraft.CrewCapacity)
                ModelState.AddModelError("", $"Too many crew members. Aircraft capacity: {aircraft.CrewCapacity}.");

            if (crewIds.Count != crewIds.Distinct().Count())
                ModelState.AddModelError("", "Duplicate crew members are not allowed.");

            // Reset availability of previously assigned crew
            foreach (var oldCrew in existingFlight.FlightCrews)
            {
                oldCrew.IsAvailable = true;
                _context.Entry(oldCrew).State = EntityState.Modified;
            }

            existingFlight.FlightCrews.Clear();

            foreach (var crewId in crewIds.Distinct())
            {
                var crew = await _context.FlightCrews.FindAsync(crewId);
                if (crew != null && crew.IsAvailable)
                {
                    crew.IsAvailable = false;
                    _context.Entry(crew).State = EntityState.Modified;
                    existingFlight.FlightCrews.Add(crew);
                }
            }

            if (!ModelState.IsValid)
            {
                flight.FlightCrews = existingFlight.FlightCrews;
                await PopulateDropdowns(flight, crewIds);
                return View(flight);
            }

            existingFlight.OriginId = flight.OriginId;
            existingFlight.DestinationId = flight.DestinationId;
            existingFlight.DepartureTime = flight.DepartureTime;
            existingFlight.ArrivalTime = flight.ArrivalTime;
            existingFlight.Gate = flight.Gate;
            existingFlight.AirlineId = flight.AirlineId;
            existingFlight.AircraftId = flight.AircraftId;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var flight = await _context.Flight
                .Include(f => f.Aircraft)
                .Include(f => f.Airlines)
                .Include(f => f.FlightCrews)
                .Include(f => f.Origin)
                .Include(f => f.Destination)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (flight == null) return NotFound();

            return View(flight);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var flight = await _context.Flight
                .Include(f => f.FlightCrews)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (flight != null)
            {
                foreach (var crew in flight.FlightCrews)
                {
                    crew.IsAvailable = true;
                    _context.Entry(crew).State = EntityState.Modified;
                }

                _context.Flight.Remove(flight);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdowns(Flight? flight = null, List<int>? extraCrewIds = null)
        {
            ViewData["AircraftId"] = new SelectList(_context.Aircrafts, "Id", "Model", flight?.AircraftId);
            ViewData["AirlineId"] = new SelectList(_context.Airline, "Id", "Name", flight?.AirlineId);
            ViewData["OriginId"] = new SelectList(_context.Airports, "Id", "Name", flight?.OriginId);
            ViewData["DestinationId"] = new SelectList(_context.Airports, "Id", "Name", flight?.DestinationId);

            var allCrew = await _context.FlightCrews.ToListAsync();
            var aircraftCapacities = await _context.Aircrafts.ToDictionaryAsync(a => a.Id, a => a.CrewCapacity);

            var extraIds = extraCrewIds ?? new List<int>();

            var validCrew = allCrew
                .Where(c => c.IsAvailable || extraIds.Contains(c.Id))
                .ToList();

            ViewBag.Captains = validCrew
                .Where(c => c.Position.ToString() == "Captain")
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.FirstName} {c.LastName} ({c.Position})"
                }).ToList();

            ViewBag.FirstOfficers = validCrew
                .Where(c => c.Position.ToString() == "FirstOfficer")
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.FirstName} {c.LastName} ({c.Position})"
                }).ToList();

            ViewBag.OtherCrew = validCrew
                .Where(c => c.Position.ToString() != "Captain" && c.Position.ToString() != "FirstOfficer")
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.FirstName} {c.LastName} ({c.Position})"
                }).ToList();

            ViewBag.AircraftCrewCapacities = aircraftCapacities;

            if (flight != null)
            {
                ViewBag.SelectedCaptainId = flight.FlightCrews.FirstOrDefault(fc => fc.Position.ToString() == "Captain")?.Id;
                ViewBag.SelectedFirstOfficerId = flight.FlightCrews.FirstOrDefault(fc => fc.Position.ToString() == "FirstOfficer")?.Id;
                ViewBag.SelectedOtherCrewIds = flight.FlightCrews
                    .Where(fc => fc.Position.ToString() != "Captain" && fc.Position.ToString() != "FirstOfficer")
                    .Select(fc => fc.Id).ToList();
            }
        }

        private bool FlightExists(int id)
        {
            return _context.Flight.Any(e => e.Id == id);
        }
    }
}
