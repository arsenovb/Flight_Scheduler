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

        // GET: Flights
        public async Task<IActionResult> Index()
        {
            var flights = await _context.Flight
                .Include(f => f.Aircraft)
                .Include(f => f.Airlines)
                .Include(f => f.FlightCrews)
                .ToListAsync();
            return View(flights);
        }

        // GET: Flights/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var flight = await _context.Flight
                .Include(f => f.Aircraft)
                .Include(f => f.Airlines)
                .Include(f => f.FlightCrews)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (flight == null) return NotFound();

            return View(flight);
        }

        // GET: Flights/Create
        public async Task<IActionResult> Create()
        {
            var hasAircrafts = await _context.Aircrafts.AnyAsync();
            var hasAirlines = await _context.Airline.AnyAsync();
            var hasCrew = await _context.FlightCrews.AnyAsync();

            if (!hasAircrafts || !hasAirlines || !hasCrew)
            {
                TempData["CreateDisabledReason"] = "To create a flight, at least one aircraft, airline, and crewmember must exist.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns();
            return View();
        }

        // POST: Flights/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Origin,Destination,DepartureTime,ArrivalTime,Gate,AirlineId,AircraftId")] Flight flight,
            int? CaptainId,
            int? FirstOfficerId,
            List<int> OtherCrewIds)
        {
            var allCrewIds = new List<int>();
            if (CaptainId.HasValue) allCrewIds.Add(CaptainId.Value);
            if (FirstOfficerId.HasValue) allCrewIds.Add(FirstOfficerId.Value);
            if (OtherCrewIds != null) allCrewIds.AddRange(OtherCrewIds);

            if (allCrewIds.Count != allCrewIds.Distinct().Count())
                ModelState.AddModelError("", "Duplicate crew members are not allowed.");

            var aircraft = await _context.Aircrafts.FindAsync(flight.AircraftId);
            if (aircraft == null)
                ModelState.AddModelError("AircraftId", "Aircraft not found.");
            else if (allCrewIds.Count > aircraft.CrewCapacity)
                ModelState.AddModelError("", $"Too many crew members. Aircraft capacity: {aircraft.CrewCapacity}.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return View(flight);
            }

            flight.FlightCrews = new List<FlightCrew>();
            foreach (var crewId in allCrewIds.Distinct())
            {
                var crew = await _context.FlightCrews.FindAsync(crewId);
                if (crew != null && crew.IsAvailable)
                {
                    crew.IsAvailable = false;
                    flight.FlightCrews.Add(crew);
                }
            }

            _context.Flight.Add(flight);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Flights/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var flight = await _context.Flight.FindAsync(id);
            if (flight == null) return NotFound();

            ViewData["AircraftId"] = new SelectList(_context.Aircrafts, "Id", "Model", flight.AircraftId);
            ViewData["AirlineId"] = new SelectList(_context.Airline, "Id", "Name", flight.AirlineId);

            return View(flight);
        }

        // POST: Flights/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Origin,Destination,DepartureTime,ArrivalTime,Gate,AirlineId,AircraftId")] Flight flight)
        {
            if (id != flight.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(flight);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FlightExists(flight.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["AircraftId"] = new SelectList(_context.Aircrafts, "Id", "Model", flight.AircraftId);
            ViewData["AirlineId"] = new SelectList(_context.Airline, "Id", "Name", flight.AirlineId);

            return View(flight);
        }

        // GET: Flights/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var flight = await _context.Flight
                .Include(f => f.Aircraft)
                .Include(f => f.Airlines)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (flight == null) return NotFound();

            return View(flight);
        }

        // POST: Flights/Delete/5
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
                }

                _context.Flight.Remove(flight);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool FlightExists(int id)
        {
            return _context.Flight.Any(e => e.Id == id);
        }

        private async Task PopulateDropdowns()
        {
            ViewData["AircraftId"] = new SelectList(_context.Aircrafts, "Id", "Model");
            ViewData["AirlineId"] = new SelectList(_context.Airline, "Id", "Name");

            ViewBag.Captains = await _context.FlightCrews
                .Where(fc => fc.IsAvailable && fc.Position == FlightCrew.CrewPosition.Captain)
                .Select(fc => new SelectListItem { Value = fc.Id.ToString(), Text = $"{fc.FirstName} {fc.LastName}" })
                .ToListAsync();

            ViewBag.FirstOfficers = await _context.FlightCrews
                .Where(fc => fc.IsAvailable && fc.Position == FlightCrew.CrewPosition.FirstOfficer)
                .Select(fc => new SelectListItem { Value = fc.Id.ToString(), Text = $"{fc.FirstName} {fc.LastName}" })
                .ToListAsync();

            ViewBag.OtherCrew = await _context.FlightCrews
                .Where(fc => fc.IsAvailable &&
                             fc.Position != FlightCrew.CrewPosition.Captain &&
                             fc.Position != FlightCrew.CrewPosition.FirstOfficer)
                .Select(fc => new SelectListItem { Value = fc.Id.ToString(), Text = $"{fc.FirstName} {fc.LastName}" })
                .ToListAsync();

            ViewBag.AircraftCrewCapacities = await _context.Aircrafts
                .ToDictionaryAsync(a => a.Id, a => a.CrewCapacity);
        }
    }
}
