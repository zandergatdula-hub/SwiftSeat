using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SwiftSeat.Models;

namespace SwiftSeat.Controllers
{
    public class ShowsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShowsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Shows
        public async Task<IActionResult> Index()
        {
            return View(await _context.Shows.ToListAsync());
        }

        // GET: Shows/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shows = await _context.Shows
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (shows == null)
            {
                return NotFound();
            }

            return View(shows);
        }

        // GET: Shows/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Shows/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Shows show)
        {
            if (ModelState.IsValid)
            {
                if (show.PhotoFile != null)
                {
                    // Get the file name
                    var fileName = Path.GetFileName(show.PhotoFile.FileName);

                    // Set the path to wwwroot/photos
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/photos", fileName);

                    // Save the file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await show.PhotoFile.CopyToAsync(stream);
                    }

                    // Set the PhotoFileName property
                    show.PhotoFileName = fileName;
                }
                  
                // Save to database (add and save changes)
                _context.Add(show);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(show);
        }

        // GET: Shows/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shows = await _context.Shows.FindAsync(id);
            if (shows == null)
            {
                return NotFound();
            }
            return View(shows);
        }

        // POST: Shows/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventId,Title,Description,EventDate,Venue,PhotoFile,PhotoFileName")] Shows shows)
        {
            if (id != shows.EventId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle file upload
                    if (shows.PhotoFile != null)
                    {
                        var fileName = Path.GetFileName(shows.PhotoFile.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/photos", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await shows.PhotoFile.CopyToAsync(stream);
                        }

                        shows.PhotoFileName = fileName;
                    }

                    _context.Update(shows);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShowsExists(shows.EventId))
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
            return View(shows);
        }

        // GET: Shows/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shows = await _context.Shows
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (shows == null)
            {
                return NotFound();
            }

            return View(shows);
        }

        // POST: Shows/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var shows = await _context.Shows.FindAsync(id);
            if (shows != null)
            {
                _context.Shows.Remove(shows);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ShowsExists(int id)
        {
            return _context.Shows.Any(e => e.EventId == id);
        }
    }
}
