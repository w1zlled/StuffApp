using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StuffApp.Models;
using StuffApp.Models.Data;

namespace StuffApp.Controllers
{
    [Authorize(Roles = "admin")]
    public class PostStatusLogsController : Controller
    {
        private readonly AppCtx _context;

        public PostStatusLogsController(AppCtx context)
        {
            _context = context;
        }

        // GET: PostStatusLogs
        public async Task<IActionResult> Index()
        {
              return _context.PostStatusLog != null ? 
                          View(await _context.PostStatusLog.ToListAsync()) :
                          Problem("Entity set 'AppCtx.PostStatusLog'  is null.");
        }

        // GET: PostStatusLogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.PostStatusLog == null)
            {
                return NotFound();
            }

            var postStatusLog = await _context.PostStatusLog
                .FirstOrDefaultAsync(m => m.Id == id);
            if (postStatusLog == null)
            {
                return NotFound();
            }

            return View(postStatusLog);
        }

        // GET: PostStatusLogs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PostStatusLogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdStatus,IdPost,RegDate")] PostStatusLog postStatusLog)
        {
            if (ModelState.IsValid)
            {
                _context.Add(postStatusLog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(postStatusLog);
        }

        // GET: PostStatusLogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.PostStatusLog == null)
            {
                return NotFound();
            }

            var postStatusLog = await _context.PostStatusLog.FindAsync(id);
            if (postStatusLog == null)
            {
                return NotFound();
            }
            return View(postStatusLog);
        }

        // POST: PostStatusLogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdStatus,IdPost,RegDate")] PostStatusLog postStatusLog)
        {
            if (id != postStatusLog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(postStatusLog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostStatusLogExists(postStatusLog.Id))
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
            return View(postStatusLog);
        }

        // GET: PostStatusLogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.PostStatusLog == null)
            {
                return NotFound();
            }

            var postStatusLog = await _context.PostStatusLog
                .FirstOrDefaultAsync(m => m.Id == id);
            if (postStatusLog == null)
            {
                return NotFound();
            }

            return View(postStatusLog);
        }

        // POST: PostStatusLogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.PostStatusLog == null)
            {
                return Problem("Entity set 'AppCtx.PostStatusLog'  is null.");
            }
            var postStatusLog = await _context.PostStatusLog.FindAsync(id);
            if (postStatusLog != null)
            {
                _context.PostStatusLog.Remove(postStatusLog);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostStatusLogExists(int id)
        {
          return (_context.PostStatusLog?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
