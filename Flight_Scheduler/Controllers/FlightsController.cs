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
            var flight_SchedulerContext = _context.Flight.Include(f => f.Aircraft).Include(f => f.Airlines).Include(f => f.FlightCrew);
            return View(await flight_SchedulerContext.ToListAsync());
        }

        // GET: Flights/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var flight = await _context.Flight
                .Include(f => f.Aircraft)
                .Include(f => f.Airlines)
                .Include(f => f.FlightCrew)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (flight == null)
            {
                return NotFound();
            }

            return View(flight);
        }

        // GET: Flights/Create
        public IActionResult Create()
        {
            ViewData["AircraftId"] = new SelectList(_context.Aircrafts, "Id", "Model");
            ViewData["AirlineId"] = new SelectList(_context.Airline, "Id", "Name");
            ViewData["FlightCrewId"] = new SelectList(_context.FlightCrews, "Id", "FirstName");
            return View();
        }

        // POST: Flights/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Origin,Destination,DepartureTime,ArrivalTime,Gate,AirlineId,AircraftId,FlightCrewId")] Flight flight)
        {
            if (ModelState.IsValid)
            {
                _context.Add(flight);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AircraftId"] = new SelectList(_context.Aircrafts, "Id", "Id", flight.AircraftId);
            ViewData["AirlineId"] = new SelectList(_context.Airline, "Id", "Id", flight.AirlineId);
            ViewData["FlightCrewId"] = new SelectList(_context.FlightCrews, "Id", "Id", flight.FlightCrewId);
            return View(flight);
        }

        // GET: Flights/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var flight = await _context.Flight.FindAsync(id);
            if (flight == null)
            {
                return NotFound();
            }
            ViewData["AircraftId"] = new SelectList(_context.Aircrafts, "Id", "Model", flight.AircraftId);
            ViewData["AirlineId"] = new SelectList(_context.Airline, "Id", "Name", flight.AirlineId);
            ViewData["FlightCrewId"] = new SelectList(_context.FlightCrews, "Id", "FirstName", flight.FlightCrewId);
            return View(flight);
        }

        // POST: Flights/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Origin,Destination,DepartureTime,ArrivalTime,Gate,AirlineId,AircraftId,FlightCrewId")] Flight flight)
        {
            if (id != flight.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(flight);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FlightExists(flight.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AircraftId"] = new SelectList(_context.Aircrafts, "Id", "Id", flight.AircraftId);
            ViewData["AirlineId"] = new SelectList(_context.Airline, "Id", "Id", flight.AirlineId);
            ViewData["FlightCrewId"] = new SelectList(_context.FlightCrews, "Id", "Id", flight.FlightCrewId);
            return View(flight);
        }

        // GET: Flights/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var flight = await _context.Flight
                .Include(f => f.Aircraft)
                .Include(f => f.Airlines)
                .Include(f => f.FlightCrew)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (flight == null)
            {
                return NotFound();
            }

            return View(flight);
        }

        // POST: Flights/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var flight = await _context.Flight.FindAsync(id);
            if (flight != null)
            {
                _context.Flight.Remove(flight);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FlightExists(int id)
        {
            return _context.Flight.Any(e => e.Id == id);
        }
    }
}
