using CollegeEventManager.Data;
using CollegeEventManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegeEventManager.Controllers
{
    public class EventController : Controller
    {
        // Controller for managing events
        private readonly AppDbContext _context;

        public EventController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Event/
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events
                .Include(e => e.Registrations)
                .ToListAsync();
            return View(events);
        }

        // GET: /Event/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Event/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventModel ev, IFormFile PosterFile)
        {
            if (ModelState.IsValid)
            {
                // Handle file upload if a poster was provided
                if (PosterFile != null && PosterFile.Length > 0)
                {
                    // Create uploads directory if it doesn't exist
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate unique filename
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + PosterFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await PosterFile.CopyToAsync(fileStream);
                    }

                    // Update the event's PosterPath property
                    ev.PosterPath = uniqueFileName;
                }

                _context.Events.Add(ev);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ev);
        }

        // GET: /Event/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();
            return View(ev);
        }

        // POST: /Event/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventModel eventModel, IFormFile PosterFile)
        {
            if (id != eventModel.EventId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle file upload if a new poster was provided
                    if (PosterFile != null && PosterFile.Length > 0)
                    {
                        // Create uploads directory if it doesn't exist
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Delete old poster file if it exists
                        if (!string.IsNullOrEmpty(eventModel.PosterPath))
                        {
                            var oldFilePath = Path.Combine(uploadsFolder, eventModel.PosterPath);
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Generate unique filename
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + PosterFile.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // Save the file
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await PosterFile.CopyToAsync(fileStream);
                        }

                        // Update the event's PosterPath property
                        eventModel.PosterPath = uniqueFileName;
                    }

                    _context.Update(eventModel);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(eventModel.EventId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(eventModel);
        }

        // GET: /Event/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Registrations)
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (ev == null) return NotFound();
            return View(ev);
        }

        // GET: /Event/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            Console.WriteLine($"Loading delete confirmation for event ID: {id}");
            
            var ev = await _context.Events.FindAsync(id);
            
            if (ev == null)
            {
                Console.WriteLine($"Event with ID {id} not found for deletion confirmation");
                return NotFound();
            }
            
            return View(ev);
        }

        // POST: /Event/Delete/5
        [HttpPost]
        [ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Find the event with related registrations
            var ev = await _context.Events
                .Include(e => e.Registrations)
                .FirstOrDefaultAsync(e => e.EventId == id);
    
            if (ev == null)
            {
                return NotFound();
            }

            try
            {
                // Check for poster file to delete
                if (!string.IsNullOrEmpty(ev.PosterPath))
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    var filePath = Path.Combine(uploadsFolder, ev.PosterPath);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // Remove the event (cascade delete will remove registrations too)
                _context.Events.Remove(ev);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Unable to delete event: {ex.Message}");
                return View(ev);
            }
        }

        // GET: Event/Register/5
        public async Task<IActionResult> Register(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();
            
            // Create a new registration with the event ID pre-populated
            var registration = new RegistrationModel
            {
                EventId = id
            };
            
            ViewBag.EventName = ev.Title;
            return View(registration);
        }

        // POST: Event/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegistrationModel registration)
        {
            if (ModelState.IsValid)
            {
                // Check if event exists
                var eventExists = await _context.Events.AnyAsync(e => e.EventId == registration.EventId);
                if (!eventExists)
                {
                    return NotFound("Event not found");
                }

                // Add registration to database
                _context.Registrations.Add(registration);
                await _context.SaveChangesAsync();
                
                // Redirect to event details
                return RedirectToAction(nameof(Details), new { id = registration.EventId });
            }
            
            // If we got this far, something failed; redisplay form
            var ev = await _context.Events.FindAsync(registration.EventId);
            ViewBag.EventName = ev?.Title ?? "Event";
            return View(registration);
        }

        // GET: Event/EditRegistration/5
        public async Task<IActionResult> EditRegistration(int id)
        {
            var registration = await _context.Registrations
                .Include(r => r.Event)
                .FirstOrDefaultAsync(r => r.RegistrationId == id);
                
            if (registration == null)
            {
                return NotFound();
            }
            
            ViewBag.EventTitle = registration.Event?.Title;
            return View(registration);
        }

        // POST: Event/EditRegistration/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRegistration(RegistrationModel registration)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(registration);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Details), new { id = registration.EventId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RegistrationExists(registration.RegistrationId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            
            var ev = await _context.Events.FindAsync(registration.EventId);
            ViewBag.EventTitle = ev?.Title;
            return View(registration);
        }

        // GET: Event/DeleteRegistration/5
        public async Task<IActionResult> DeleteRegistration(int id)
        {
            var registration = await _context.Registrations
                .Include(r => r.Event)
                .FirstOrDefaultAsync(r => r.RegistrationId == id);
                
            if (registration == null)
            {
                return NotFound();
            }
            
            ViewBag.EventTitle = registration.Event?.Title;
            return View(registration);
        }

        // POST: Event/DeleteRegistrationConfirmed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRegistrationConfirmed(int id, int eventId)
        {
            var registration = await _context.Registrations.FindAsync(id);
            if (registration != null)
            {
                _context.Registrations.Remove(registration);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Details), new { id = eventId });
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }

        private bool RegistrationExists(int id)
        {
            return _context.Registrations.Any(r => r.RegistrationId == id);
        }
    }
}