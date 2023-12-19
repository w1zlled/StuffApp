using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StuffApp.Models;
using StuffApp.Models.Data;

namespace StuffApp.Controllers
{
    [Authorize(Roles = "admin")]
    public class PostStatusesController : Controller
    {
        private readonly AppCtx _context;

        public PostStatusesController(AppCtx context)
        {
            _context = context;
        }

        // GET: PostStatuses
        public async Task<IActionResult> Index()
        {
            return _context.PostStatus != null ? 
                          View(await _context.PostStatus.OrderBy(f => f.Id).ToListAsync()) :
                          Problem("Entity set 'AppCtx.PostStatus'  is null.");
        }

        // GET: PostStatuses/Details/5
        public async Task<IActionResult> Details(short? id)
        {
            if (id == null || _context.PostStatus == null)
            {
                return NotFound();
            }

            var postStatus = await _context.PostStatus
                .FirstOrDefaultAsync(m => m.Id == id);
            if (postStatus == null)
            {
                return NotFound();
            }

            return View(postStatus);
        }

        // GET: PostStatuses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PostStatuses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StatusName")] PostStatus postStatus)
        {
            if (_context.PostStatus
                .Where(f => f.StatusName == postStatus.StatusName)
                .FirstOrDefault() != null
                )
            {
                ModelState.AddModelError("", "Введеная категория уже существует");
            }
            if (ModelState.IsValid)
            {
                _context.Add(postStatus);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(postStatus);
        }

        // GET: PostStatuses/Edit/5
        public async Task<IActionResult> Edit(short? id)
        {
            if (id == null || _context.PostStatus == null)
            {
                return NotFound();
            }

            var postStatus = await _context.PostStatus.FindAsync(id);
            if (postStatus == null)
            {
                return NotFound();
            }
            return View(postStatus);
        }

        // POST: PostStatuses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, [Bind("Id,StatusName")] PostStatus postStatus)
        {
            if (id != postStatus.Id)
            {
                return NotFound();
            }

            if (_context.PostStatus
                .Where(f => f.StatusName == postStatus.StatusName)
                .FirstOrDefault() != null
                )
            {
                ModelState.AddModelError("", "Введеная категория уже существует");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(postStatus);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostStatusExists(postStatus.Id))
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
            return View(postStatus);
        }

        // GET: PostStatuses/Delete/5
        public async Task<IActionResult> Delete(short? id)
        {
            if (id == null || _context.PostStatus == null)
            {
                return NotFound();
            }

            var postStatus = await _context.PostStatus
                .FirstOrDefaultAsync(m => m.Id == id);
            if (postStatus == null)
            {
                return NotFound();
            }

            return View(postStatus);
        }

        // POST: PostStatuses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            if (_context.PostStatus == null)
            {
                return Problem("Entity set 'AppCtx.PostStatus'  is null.");
            }
            var postStatus = await _context.PostStatus.FindAsync(id);
            if (postStatus != null)
            {
                _context.PostStatus.Remove(postStatus);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostStatusExists(short id)
        {
          return (_context.PostStatus?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
