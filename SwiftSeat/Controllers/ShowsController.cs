using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SwiftSeat.Models;

namespace SwiftSeat.Controllers
{
    [Authorize]
    public class ShowsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly BlobContainerClient _containerClient;

        public ShowsController(ApplicationDbContext context, IConfiguration configuration)
        {
           _configuration = configuration;
            _context = context;

            var connectionString = _configuration["SwiftSeat_Storage"];
            var cotainerName = "swiftseat-uploads";
            _containerClient = new BlobContainerClient(connectionString, cotainerName);
        }

        // GET: Shows
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Shows.Include(s => s.Category);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Shows/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shows = await _context.Shows
                .Include(s => s.Category)
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Shows/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,EventDate,Venue,Owner,CategoryId,PhotoFile,Created")] Shows shows)
        {
            if (ModelState.IsValid)
            {
                // Set Created to now
                shows.Created = DateTime.Now;

                // Handle photo upload
                if (shows.PhotoFile != null)
                {
                    var uniqueFileName = Guid.NewGuid() + "_" + shows.PhotoFile.FileName;
                    var blobClient = _containerClient.GetBlobClient(uniqueFileName);

                    using (var stream = shows.PhotoFile.OpenReadStream())
                        await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = shows.PhotoFile.ContentType });

                    shows.PhotoFileName = blobClient.Uri.ToString();
                }
                else
                {
                    shows.PhotoFileName = null;
                }

                _context.Add(shows);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", shows.CategoryId);
            return View(shows);
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", shows.CategoryId);
            return View(shows);
        }

        // POST: Shows/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventId,Title,Description,EventDate,Venue,PhotoFileName,Owner,CategoryId,CategoryName,PhotoFile")] Shows shows)
        {
            if (id != shows.EventId)
            {
                return NotFound();
            }

            // Fetch the original show from the database
            var originalShow = await _context.Shows.AsNoTracking().FirstOrDefaultAsync(s => s.EventId == id);
            if (originalShow == null)
            {
                return NotFound();
            }

            // Always preserve the original Created date
            shows.Created = originalShow.Created;

            // If a new photo is uploaded
            if (shows.PhotoFile != null)
            {
                var newBlobName = Guid.NewGuid() + "_" + shows.PhotoFile.FileName;
                shows.PhotoFileName = newBlobName;

                var newBlobClient = _containerClient.GetBlobClient(newBlobName);

                using (var stream = shows.PhotoFile.OpenReadStream())
                    await newBlobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = shows.PhotoFile.ContentType });

                shows.PhotoFileName = newBlobClient.Uri.ToString();

                // Delete old blob if it exists
                if (!string.IsNullOrEmpty(originalShow.PhotoFileName) && originalShow.PhotoFileName.StartsWith("http"))
                {
                    // Extract the blob name from the URL using string operations
                    var lastSlashIndex = originalShow.PhotoFileName.LastIndexOf('/');
                    var blobName = lastSlashIndex >= 0
                        ? originalShow.PhotoFileName.Substring(lastSlashIndex + 1)
                        : originalShow.PhotoFileName;

                    var oldBlobClient = _containerClient.GetBlobClient(blobName);
                    await oldBlobClient.DeleteIfExistsAsync();
                }
            }
            else
            {
                shows.PhotoFileName = originalShow.PhotoFileName;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(shows);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShowsExists(shows.EventId))
                    {
                        return NotFound();
                    }
                    throw;
                }

                return RedirectToAction("Index", "Home");
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", shows.CategoryId);
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
                .Include(s => s.Category)
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
                // Delete image from Azure Blob Storage if it exists
                if (!string.IsNullOrEmpty(shows.PhotoFileName))
                {
                    // Extract the blob name from the URL using string operations
                    var lastSlashIndex = shows.PhotoFileName.LastIndexOf('/');
                    var blobName = lastSlashIndex >= 0
                        ? shows.PhotoFileName.Substring(lastSlashIndex + 1)
                        : shows.PhotoFileName;

                    var blobClient = _containerClient.GetBlobClient(blobName);
                    await blobClient.DeleteIfExistsAsync();
                }

                _context.Shows.Remove(shows);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", "Home");
        }

        private bool ShowsExists(int id)
        {
            return _context.Shows.Any(e => e.EventId == id);
        }
    }
}
