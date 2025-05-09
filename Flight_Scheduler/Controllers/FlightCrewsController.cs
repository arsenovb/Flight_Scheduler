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
    public class FlightCrewsController : Controller
    {
        private readonly Flight_SchedulerContext _context;

        public FlightCrewsController(Flight_SchedulerContext context)
        {
            _context = context;
        }

        // GET: FlightCrews
        public async Task<IActionResult> Index()
        {
            return View(await _context.FlightCrews.ToListAsync());
        }

        // GET: FlightCrews/Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var flightCrew = await _context.FlightCrews
                .FirstOrDefaultAsync(m => m.Id == id);
            if (flightCrew == null)
            {
                return NotFound();
            }

            return View(flightCrew);
        }

        // GET: FlightCrews/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: FlightCrews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,Age,Position,IsAvailable")] FlightCrew flightCrew)
        {
            if (ModelState.IsValid)
            {
                _context.Add(flightCrew);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(flightCrew);
        }

        // GET: FlightCrews/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var flightCrew = await _context.FlightCrews.FindAsync(id);
            if (flightCrew == null)
            {
                return NotFound();
            }
            return View(flightCrew);
        }

        // POST: FlightCrews/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Age,Position,IsAvailable")] FlightCrew flightCrew)
        {
            if (id != flightCrew.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(flightCrew);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FlightCrewExists(flightCrew.Id))
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
            return View(flightCrew);
        }

        // GET: FlightCrews/Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var flightCrew = await _context.FlightCrews
                .FirstOrDefaultAsync(m => m.Id == id);
            if (flightCrew == null)
            {
                return NotFound();
            }

            return View(flightCrew);
        }

        // POST: FlightCrews/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var flightCrew = await _context.FlightCrews.FindAsync(id);
            if (flightCrew != null)
            {
                _context.FlightCrews.Remove(flightCrew);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FlightCrewExists(int id)
        {
            return _context.FlightCrews.Any(e => e.Id == id);
        }
    }
}
