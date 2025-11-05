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
        public async Task<IActionResult> Edit(int id, [Bind("EventId,Title,Description,EventDate,Venue,PhotoFileName,Owner,Created,CategoryId,CategoryName,PhotoFile")] Shows shows)
        {
            if (id != shows.EventId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // If a new photo is uploaded
                if (shows.PhotoFile != null)
                {
                    var newBlobName = Guid.NewGuid() + "_" + shows.PhotoFile.FileName; // Generate a unique blob name
                 
                    shows.PhotoFileName = newBlobName; // Set the new filename

                    var newBlobClient = _containerClient.GetBlobClient(newBlobName); // Create blob client for new file
                  
                    using (var stream = shows.PhotoFile.OpenReadStream())   // Upload new file to Azure Blob Storage
                        await newBlobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = shows.PhotoFile.ContentType });
                    
                    shows.PhotoFileName = newBlobClient.Uri.ToString(); // it updates the model in the URL 

                    // Delete old blob if it exists
                    var existingShow = await _context.Shows.AsNoTracking().FirstOrDefaultAsync(s => s.EventId == id);
                    if (!string.IsNullOrEmpty(existingShow?.PhotoFileName) && existingShow.PhotoFileName.StartsWith("http")) // this will just fecthes the existing show from the Azure Storage
                        if (Uri.TryCreate(existingShow.PhotoFileName, UriKind.Absolute, out var oldUri)) // it check if the existingShow.PhotoFilename is a valid URL
                            await _containerClient.GetBlobClient(Path.GetFileName(oldUri.AbsolutePath)).DeleteIfExistsAsync(); // if it's valid, this will extracts the filename from the URL 
                }
                else
                {
                     // if there is no new photo it will just keep the old photo
                    var existingShow = await _context.Shows.AsNoTracking().FirstOrDefaultAsync(s => s.EventId == id);
                    shows.PhotoFileName = existingShow?.PhotoFileName;
                }

                // Save changes to database
                try
                {
                    _context.Update(shows);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) // this will catch if the another user modified or delete
                                                     // the same shows in the database 
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
                    string blobName;
                    if (Uri.TryCreate(shows.PhotoFileName, UriKind.Absolute, out var uri)) // this will check if the URl is 
                    {
                        blobName = Path.GetFileName(uri.AbsolutePath); // Extracts the fimename to the URL path
                    }
                    else
                    {
                        blobName = shows.PhotoFileName;
                    }
                    var blobClient = _containerClient.GetBlobClient(blobName); // Will create an client object for the blobClient
                                                                              // in the Azure container 
                    await blobClient.DeleteIfExistsAsync(); // this will Asynchronously deletes the blob from the
                                                            // Azure Blob Storage 
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
