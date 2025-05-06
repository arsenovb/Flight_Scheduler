using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Flight_Scheduler.Data;
using Flight_Scheduler.Models;
using System.Diagnostics;

namespace Flight_Scheduler.Controllers
{
    public class HomeController : Controller
    {
        private readonly Flight_SchedulerContext _context;

        public HomeController(Flight_SchedulerContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var flights = await _context.Flight
                .Include(f => f.Airlines)
                .Include(f => f.Aircraft)
                .ToListAsync();

            return View(flights);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
