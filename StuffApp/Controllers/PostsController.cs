using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using StuffApp.Models;
using StuffApp.Models.Data;
using StuffApp.ViewModels.Posts;
using System.Data;

namespace StuffApp.Controllers
{
    public class PostsController : Controller
    {
        private readonly AppCtx _context;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PostsController(AppCtx context, UserManager<User> user, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = user;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Posts
        public async Task<IActionResult> Index(string sortOrder, string searchString, string currentFilter, int? pageNumber, int? categoryId)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["CurrentFilter"] = searchString;
            ViewData["PageNumber"] = pageNumber != null ? pageNumber : 1;
            ViewData["CategoryId"] = categoryId;
            ViewBag.Categories = _context.Categories.ToList();
            /*var orderList = new List<string> { "По названию", "По цене" };
            ViewBag.OrderBy = new SelectList(orderList);*/
            ViewData["sortOrder"] = sortOrder;
            List<SelectListItem> orderByList = new List<SelectListItem>
            {
                new SelectListItem { Value = "name", Text = "По названию" },
                new SelectListItem { Value = "nameDesc", Text = "По названию (по убыванию)" },
                new SelectListItem { Value = "price", Text = "По цене" },
                new SelectListItem { Value = "priceDesc", Text = "По цене (по убыванию)" }
            };
            ViewBag.OrderBy = new SelectList(orderByList, "Value", "Text");

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            /*var appCtx = _context.Posts
               .Include(post => post.Category)
               .Include(post => post.User)
               .Include(post => post.PostStatusLog.MaxBy(PostStatusLog => PostStatusLog.EditDate))
               .ThenInclude(PostStatusLog => PostStatusLog.PostStatus.StatusName);*/

            var appCtx = _context.Posts
            .Include(post => post.Category)
            .Include(post => post.User)
            .Select(post => new PostWithStatus
            {
                Post = post,
                LatestStatus = post.PostStatusLog
                    .OrderByDescending(log => log.EditDate)
                    .FirstOrDefault().PostStatus,
                EditDate = post.PostStatusLog
                    .OrderByDescending(log => log.EditDate)
                    .FirstOrDefault().EditDate
            })
            .Where(postWithStatus => postWithStatus.LatestStatus.Id == 3); ;
            if (categoryId.HasValue)
            {
                appCtx = appCtx.Where(post => post.Post.IdCategory == categoryId.Value);
            }
            if (!String.IsNullOrEmpty(searchString))
            {
                appCtx = appCtx.Where(s => s.Post.Title.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "nameDesc":
                    //students = students.OrderByDescending(s => s.LastName);
                    appCtx = appCtx.OrderByDescending(s => s.Post.Title);
                    break;
                case "price":
                    //students = students.OrderBy(s => s.EnrollmentDate);
                    appCtx = appCtx.OrderBy(s => s.Post.Price);
                    break;
                case "priceDesc":
                    //students = students.OrderByDescending(s => s.EnrollmentDate);
                    appCtx = appCtx.OrderByDescending(s => s.Post.Price);
                    break;
                default:
                    //students = students.OrderBy(s => s.LastName);
                    appCtx = appCtx.OrderBy(s => s.Post.Title);
                    break;
            }

            /*appCtx.OrderBy(f => f.Title);*/
            /*var appCtx = _context.*/
            /*return View(await appCtx.ToListAsync());*/
            int pageSize = 8;
            return View(await PaginatedList<PostWithStatus>.CreateAsync(appCtx.AsNoTracking(), pageNumber ?? 1, pageSize));
        }


        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            /*var post = await _context.Posts
                .Include(p => p.Category)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);*/

            var post = _context.Posts
            .Include(post => post.Category)
            .Include(post => post.User)
            .Where(post => post.Id == id)
            .Select(post => new PostWithStatus
            {
                Post = post,
                LatestStatus = post.PostStatusLog
                    .OrderByDescending(log => log.EditDate)
                    .FirstOrDefault().PostStatus,
                EditDate = post.PostStatusLog
                    .OrderByDescending(log => log.EditDate)
                    .FirstOrDefault().EditDate
            })
            .FirstOrDefault();
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DetailsModerate(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            /*var post = await _context.Posts
                .Include(p => p.Category)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);*/

            var post = _context.Posts
            .Include(post => post.Category)
            .Include(post => post.User)
            .Where(post => post.Id == id)
            .Select(post => new PostWithStatus
            {
                Post = post,
                LatestStatus = post.PostStatusLog
                    .OrderByDescending(log => log.EditDate)
                    .FirstOrDefault().PostStatus,
                EditDate = post.PostStatusLog
                    .OrderByDescending(log => log.EditDate)
                    .FirstOrDefault().EditDate
            })
            .FirstOrDefault();
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Posts/Create
        public async Task<IActionResult> CreateAsync()
        {
            ViewData["IdCategory"] = new SelectList(_context.Categories, "Id", "CategoryName");
            string userEmail = User.Identity.Name;
            // Получаем объект пользователя по email
            var user = await _userManager.FindByEmailAsync(userEmail);
            var userId = user.Id;
            ViewData["IdUser"] = new SelectList(_context.Users, "Id", "Fullname", userId);
            return View();
        }

        // POST: Posts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePostViewModel model)
        {
            /*if (ModelState.IsValid)
            {*/
            /*_context.Add(post);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));*/
            /*}
            else
            {
                return NotFound(ModelState);
            }*/
            if (ModelState.IsValid)
            {
                if (model.ImageFile != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    model.ImgUrl = "/images/" + uniqueFileName;

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }
                }
                Post post = new()
                {
                    Title = model.Title,
                    Descr = model.Descr,
                    Address = model.Address,
                    ImgUrl = model.ImgUrl,
                    /*Price = model.Price,*/
                    Price = (model.Price != null ? model.Price : 0),
                    IdCategory = model.IdCategory,
                    IdUser = model.IdUser
                    /*CategoryName = model.CategoryName,
                    ParentCategoryId = (short)((model.ParentCategoryId != null) ? model.ParentCategoryId : 0)*/
                };
                _context.Add(post);


                await _context.SaveChangesAsync();
                int postId = post.Id;
                var log = new PostStatusLog { IdPost = postId, IdStatus = 2, EditDate = DateTime.Now };
                _context.Add(log);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return NotFound(ModelState);
            }
            ViewData["IdCategory"] = new SelectList(_context.Categories, "Id", "CategoryName", model.IdCategory);
            ViewData["IdUser"] = new SelectList(_context.Users, "Id", "Fullname", model.IdUser);
            return View(model);
        }

        // GET: Posts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            EditPostViewModel model = new()
            {
                Id = post.Id,
                Title = post.Title,
                Descr = post.Descr,
                Address = post.Address,
                ImgUrl = post.ImgUrl,
                Price = post.Price,
                IdCategory = post.IdCategory,
                IdUser = post.IdUser
            };
            ViewData["IdCategory"] = new SelectList(_context.Categories, "Id", "CategoryName", post.IdCategory);
            ViewData["IdUser"] = new SelectList(_context.Users, "Id", "Fullname", post.IdUser);
            ViewBag.CurrentImgUrl = post.ImgUrl;
            return View(model);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditPostViewModel model)
        {
            Post post = await _context.Posts.FindAsync(id);
            if (id != post.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (model.ImageFile != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    model.ImgUrl = "/images/" + uniqueFileName;

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }
                } else
                {
                    Post postTemp = _context.Posts
                        .Where(p => p.Id == id)
                        .FirstOrDefault();
                    model.ImgUrl = postTemp.ImgUrl;
                }
                try
                {
                    post.Title = model.Title;
                    post.Descr = model.Descr;
                    post.Address = model.Address;
                    post.ImgUrl = model.ImgUrl;
                    post.Price = model.Price;
                    post.IdCategory = model.IdCategory;
                    post.IdUser = model.IdUser;
                    _context.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Posts", new { id = id });
                /*return RedirectToAction(nameof(Index));*/
            }
            ViewData["IdCategory"] = new SelectList(_context.Categories, "Id", "CategoryName", post.IdCategory);
            ViewData["IdUser"] = new SelectList(_context.Users, "Id", "Id", post.IdUser);
            return View(post);
            
        }

        // GET: Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Category)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Posts == null)
            {
                return Problem("Entity set 'AppCtx.Post'  is null.");
            }
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return (_context.Posts?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        // GET: Posts
        public async Task<IActionResult> IndexModerate(string sortOrder, string searchString, string currentFilter, int? pageNumber, int? categoryId, short? statusId)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["CurrentFilter"] = searchString;
            ViewData["PageNumber"] = pageNumber != null ? pageNumber : 1;
            ViewData["CategoryId"] = categoryId;
            ViewBag.Categories = _context.Categories.ToList();
            /*var orderList = new List<string> { "По названию", "По цене" };
            ViewBag.OrderBy = new SelectList(orderList);*/
            ViewData["sortOrder"] = sortOrder;
            List<SelectListItem> orderByList = new List<SelectListItem>
            {
                new SelectListItem { Value = "name", Text = "По названию" },
                new SelectListItem { Value = "nameDesc", Text = "По названию (по убыванию)" },
                new SelectListItem { Value = "price", Text = "По цене" },
                new SelectListItem { Value = "priceDesc", Text = "По цене (по убыванию)" }
            };
            ViewBag.OrderBy = new SelectList(orderByList, "Value", "Text");
            ViewData["StatusId"] = statusId;
            List<SelectListItem> statusesList = new List<SelectListItem>
            {
                new SelectListItem { Value = "3", Text = "Опубликованные" },
                new SelectListItem { Value = "2", Text = "На модерации" },
            };
            ViewBag.StatusesList = new SelectList(statusesList, "Value", "Text");

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            /*var appCtx = _context.Posts
               .Include(post => post.Category)
               .Include(post => post.User)
               .Include(post => post.PostStatusLog.MaxBy(PostStatusLog => PostStatusLog.EditDate))
               .ThenInclude(PostStatusLog => PostStatusLog.PostStatus.StatusName);*/

            var appCtx = _context.Posts
            .Include(post => post.Category)
            .Include(post => post.User)
            .Select(post => new PostWithStatus
            {
                Post = post,
                LatestStatus = post.PostStatusLog
                    .OrderByDescending(log => log.EditDate)
                    .FirstOrDefault().PostStatus,
                EditDate = post.PostStatusLog
                    .OrderByDescending(log => log.EditDate)
                    .FirstOrDefault().EditDate
            });
            if (categoryId.HasValue)
            {
                appCtx = appCtx.Where(post => post.Post.IdCategory == categoryId.Value);
            }
            if (!String.IsNullOrEmpty(searchString))
            {
                appCtx = appCtx.Where(s => s.Post.Title.Contains(searchString));
            }
            if (statusId.HasValue)
            {
                appCtx = appCtx.Where(s => s.LatestStatus.Id == statusId.Value);
            } else
            {
                appCtx = appCtx.Where(s => s.LatestStatus.Id == 2);
                ViewData["StatusId"] = 2;
            }
            switch (sortOrder)
            {
                case "nameDesc":
                    //students = students.OrderByDescending(s => s.LastName);
                    appCtx = appCtx.OrderByDescending(s => s.Post.Title);
                    break;
                case "price":
                    //students = students.OrderBy(s => s.EnrollmentDate);
                    appCtx = appCtx.OrderBy(s => s.Post.Price);
                    break;
                case "priceDesc":
                    //students = students.OrderByDescending(s => s.EnrollmentDate);
                    appCtx = appCtx.OrderByDescending(s => s.Post.Price);
                    break;
                default:
                    //students = students.OrderBy(s => s.LastName);
                    appCtx = appCtx.OrderBy(s => s.Post.Title);
                    break;
            }

            /*appCtx.OrderBy(f => f.Title);*/
            /*var appCtx = _context.*/
            /*return View(await appCtx.ToListAsync());*/
            int pageSize = 4;
            return View(await PaginatedList<PostWithStatus>.CreateAsync(appCtx.AsNoTracking(), pageNumber ?? 1, pageSize));
        }
        public async Task<IActionResult> EditStatus(int postId, short statusId)
        {
            /*var post = await _context.Posts.FindAsync(id);*/
            /*if (post == null)
            {
                return NotFound();
            }*/
            try
            {
                PostStatusLog postStatusLog = new PostStatusLog
                {
                    IdPost = postId,
                    IdStatus = statusId,
                    EditDate = DateTime.Now,
                };
                _context.Add(postStatusLog);
                await _context.SaveChangesAsync();
                ViewData["oleg"] = "govno";
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
            }
            /*return View(new PostWithStatus { Post = post });*/
            return RedirectToAction(nameof(Details), new { id = postId });
        }
    }
}
