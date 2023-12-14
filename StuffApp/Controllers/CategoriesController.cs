using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StuffApp.Models;
using StuffApp.Models.Data;
using StuffApp.ViewModels.Categories;

namespace StuffApp.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly AppCtx _context;
        /*private readonly UserManager<User> _userManager;*/

        public CategoriesController(AppCtx context/*, UserManager<User> user*/)
        {
            _context = context;
            /* _userManager = user;*/
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            /*IdentityUser user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);*/
            if (_context.Categories != null)
            {
                var appCtx = _context.Categories
                    /*.Include(s => s.Posts)*/
                    /*.Where(w => w.Posts.IdUser == user.Id)*/
                    .OrderBy(f => f.CategoryName)
                .Select(c => new CategoryWithParent
                {
                    Category = c,
                    ParentCategoryName = _context.Categories
                        .Where(pc => pc.Id == c.ParentCategoryId)
                        .Select(pc => pc.CategoryName)
                        .FirstOrDefault()
                }

                );
                return View(await appCtx.ToListAsync());
                return View(await _context.Categories.OrderBy(f => f.CategoryName).ToListAsync());
            }
            else
            {
                return Problem("Entity set 'AppCtx.Categories'  is null.");
            }
            /*return _context.Categories != null ?
                        View(await _context.Categories.OrderBy(f => f.CategoryName).ToListAsync()) :
                        Problem("Entity set 'AppCtx.Categories'  is null.");*/
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(short? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = _context.Categories
                .Where(m => m.Id == id)
                .Select(c => new CategoryWithParent
                {
                    Category = c,
                    ParentCategoryName = _context.Categories
                            .Where(pc => pc.Id == c.ParentCategoryId)
                            .Select(pc => pc.CategoryName)
                            .FirstOrDefault()
                })
                .FirstOrDefault();
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            ViewData["Categories"] = new SelectList(_context.Categories.OrderBy(o => o.CategoryName), "Id", "CategoryName", 1);
            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCategoryViewModel model)
        {

            if (_context.Categories
                .Where(f => f.CategoryName == model.CategoryName)
                .FirstOrDefault() != null
                )
            {
                ModelState.AddModelError("", "Введеная категория уже существует");
            }

            if (ModelState.IsValid)
            {
                Category category = new()
                {
                    CategoryName = model.CategoryName,
                    ParentCategoryId = (short)((model.ParentCategoryId != null) ? model.ParentCategoryId : 0)
                };
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            /*ViewData["IdFormOfStudy"] = new SelectList(_context.Categories
               .Where(w => w != null));*/
            return View(model);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(short? id)
        {
            ViewBag.Categories = _context.Categories.ToList();
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            ViewData["ParentCategoryId"] = category.ParentCategoryId;
            EditCategoryViewModel model = new()
            {
                Id = category.Id,
                CategoryName = category.CategoryName
            };
            return View(model);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, EditCategoryViewModel model)
        {
            Category category = await _context.Categories.FindAsync(id);
            if (id != category.Id)
            {
                return NotFound();
            }

            if (_context.Categories
                .Where(f => f.CategoryName == model.CategoryName)
                .Where(f => f.ParentCategoryId == model.ParentCategoryId)
                .FirstOrDefault() != null
                )
            {
                ModelState.AddModelError("", "Введеная категория уже существует");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    category.CategoryName = model.CategoryName;
                    category.ParentCategoryId = model.ParentCategoryId;
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
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
            return View(model);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(short? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            /*var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);*/
            var category = _context.Categories
                .Where(m => m.Id == id)
                .Select(c => new CategoryWithParent
                {
                    Category = c,
                    ParentCategoryName = _context.Categories
                            .Where(pc => pc.Id == c.ParentCategoryId)
                            .Select(pc => pc.CategoryName)
                            .FirstOrDefault()
                })
                .FirstOrDefault();
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            if (_context.Categories == null)
            {
                return Problem("Entity set 'AppCtx.Categories'  is null.");
            }
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(short id)
        {
            return (_context.Categories?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
