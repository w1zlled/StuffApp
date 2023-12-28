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
            ViewData["CurrentFilter"] = searchString;
            ViewData["PageNumber"] = pageNumber != null ? pageNumber : 1;
            ViewData["CategoryId"] = categoryId;
            ViewBag.Categories = _context.Categories.ToList();
            ViewData["sortOrder"] = sortOrder;
            List<SelectListItem> orderByList = new List<SelectListItem>
            {
                new SelectListItem { Value = "name", Text = "По названию" },
                new SelectListItem { Value = "nameDesc", Text = "По названию (по убыванию)" },
                new SelectListItem { Value = "price", Text = "По цене" },
                new SelectListItem { Value = "priceDesc", Text = "По цене (по убыванию)" },
                new SelectListItem { Value = "date", Text = "Сначала старые" },
                new SelectListItem { Value = "dateDesc", Text = "Сначала новые" }
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
                    .FirstOrDefault().EditDate,
                FirstImage = post.PostImage
                .OrderBy(img => img.Id)
                .FirstOrDefault().ImgUrl
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
                    appCtx = appCtx.OrderByDescending(s => s.Post.Title);
                    break;
                case "price":
                    appCtx = appCtx.OrderBy(s => s.Post.Price);
                    break;
                case "priceDesc":
                    appCtx = appCtx.OrderByDescending(s => s.Post.Price);
                    break;
                case "date":
                    appCtx = appCtx.OrderBy(s => s.EditDate);
                    break;
                case "dateDesc":
                    appCtx = appCtx.OrderByDescending(s => s.EditDate);
                    break;
                default:
                    appCtx = appCtx.OrderBy(s => s.Post.Title);
                    break;
            }

            int pageSize = 4;
            return View(await PaginatedList<PostWithStatus>.CreateAsync(appCtx.AsNoTracking(), pageNumber ?? 1, pageSize));
        }
        [Authorize(Roles = "registeredUser, admin")]
        public async Task<IActionResult> IndexMy(string sortOrder, string searchString, string currentFilter, int? pageNumber, int? categoryId, short? statusId)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentFilter"] = searchString;
            ViewData["PageNumber"] = pageNumber != null ? pageNumber : 1;
            ViewData["CategoryId"] = categoryId;
            ViewBag.Categories = _context.Categories.ToList();
            ViewData["sortOrder"] = sortOrder;
            List<SelectListItem> orderByList = new List<SelectListItem>
            {
                new SelectListItem { Value = "name", Text = "По названию" },
                new SelectListItem { Value = "nameDesc", Text = "По названию (по убыванию)" },
                new SelectListItem { Value = "price", Text = "По цене" },
                new SelectListItem { Value = "priceDesc", Text = "По цене (по убыванию)" },
                new SelectListItem { Value = "date", Text = "Сначала старые" },
                new SelectListItem { Value = "dateDesc", Text = "Сначала новые" }
            };
            ViewBag.OrderBy = new SelectList(orderByList, "Value", "Text");
            ViewData["StatusId"] = statusId;
            List<SelectListItem> statusesList = new List<SelectListItem>
            {
                new SelectListItem { Value = "3", Text = "Опубликованные" },
                new SelectListItem { Value = "2", Text = "На модерации" },
                new SelectListItem { Value = "1", Text = "Черновики" },
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
            var currentUserId = _userManager.GetUserId(User);
            var appCtx = _context.Posts
            .Include(post => post.Category)
            .Include(post => post.User)
            .Where(post => post.IdUser == currentUserId)
            .Select(post => new PostWithStatus
            {
                Post = post,
                LatestStatus = post.PostStatusLog
                    .OrderByDescending(log => log.EditDate)
                    .FirstOrDefault().PostStatus,
                EditDate = post.PostStatusLog
                    .OrderByDescending(log => log.EditDate)
                    .FirstOrDefault().EditDate,
                FirstImage = post.PostImage
                .OrderBy(img => img.Id)
                .FirstOrDefault().ImgUrl
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
            }
            else
            {
                appCtx = appCtx.Where(s => s.LatestStatus.Id == 3);
                ViewData["StatusId"] = 3;
            }
            switch (sortOrder)
            {
                case "nameDesc":
                    appCtx = appCtx.OrderByDescending(s => s.Post.Title);
                    break;
                case "price":
                    appCtx = appCtx.OrderBy(s => s.Post.Price);
                    break;
                case "priceDesc":
                    appCtx = appCtx.OrderByDescending(s => s.Post.Price);
                    break;
                case "date":
                    appCtx = appCtx.OrderBy(s => s.EditDate);
                    break;
                case "dateDesc":
                    appCtx = appCtx.OrderByDescending(s => s.EditDate);
                    break;
                default:
                    appCtx = appCtx.OrderBy(s => s.Post.Title);
                    break;
            }


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



            var post = _context.Posts
            .Include(post => post.Category)
            .Include(post => post.User)
            .Include(post => post.PostImage)
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
        [Authorize(Roles = "registeredUser, admin")]
        public async Task<IActionResult> DetailsModerate(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

   

            var post = _context.Posts
            .Include(post => post.Category)
            .Include(post => post.User)
            .Include(post => post.PostImage)
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

        [Authorize(Roles = "registeredUser, admin")]
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

        [Authorize(Roles = "registeredUser, admin")]
        // POST: Posts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePostViewModel model)
        {
            if (ModelState.IsValid)
            {
                Post post = new()
                {
                    Title = model.Title,
                    Descr = model.Descr,
                    Address = model.Address,
                    Price = (model.Price != null ? model.Price : 0), // Price = model.Price ?? 0
                    IdCategory = model.IdCategory,
                    IdUser = model.IdUser,
                    PostImage = new List<PostImage>()
                };
                _context.Add(post);
                await _context.SaveChangesAsync();
                // Обработка загрузки изображений
                foreach (var imageFile in model.ImageFile)
                {
                    if (imageFile != null)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                        string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        var postImage = new PostImage
                        {
                            ImgUrl = "/images/" + uniqueFileName,
                            IdPost = post.Id
                        };

                        post.PostImage.Add(postImage);
                    }
                }

                /*_context.Add(post);*/
                await _context.SaveChangesAsync();
                /*int postId = post.Id;*/
                var log = new PostStatusLog { IdPost = post.Id, IdStatus = 2, EditDate = DateTime.Now };
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

        [Authorize(Roles = "registeredUser, admin")]
        // GET: Posts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.PostImage)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (post == null)
            {
                return NotFound();
            }
            if (post.Id == null)
            {
                return NotFound();
            }
            EditPostViewModel model = new()
            {
                Id = post.Id,
                Title = post.Title,
                Descr = post.Descr,
                Address = post.Address,
                /*ImgUrl = post.ImgUrl,*/
                Price = post.Price,
                IdCategory = post.IdCategory,
                IdUser = post.IdUser,
                ExistingImages = post.PostImage != null
                    ? post.PostImage
                        .Where(pi => pi.ImgUrl != null)
                        .Select(pi => pi.ImgUrl)
                        .ToList()
                    : new List<string>()
                /*ExistingImages = post.PostImage.Select(pi => pi.ImgUrl).ToList()*/
            };
            ViewData["IdCategory"] = new SelectList(_context.Categories, "Id", "CategoryName", post.IdCategory);
            ViewData["IdUser"] = new SelectList(_context.Users, "Id", "Fullname", post.IdUser);
            /*ViewBag.CurrentImgUrl = post.ImgUrl;*/
            return View(model);
        }

        [Authorize(Roles = "registeredUser, admin")]
        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditPostViewModel model)
        {
            Post post = await _context.Posts
                .Include(post => post.PostImage)
                .FirstOrDefaultAsync(post => post.Id == id);
            if (id != post.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    post.Title = model.Title;
                    post.Descr = model.Descr;
                    post.Address = model.Address;
                    post.Price = (model.Price != null ? model.Price : 0);
                    post.IdCategory = model.IdCategory;
                    post.IdUser = model.IdUser;

          
                    if (model.ImageFile != null)
                    {
                        // Обработка загрузки изображений
                        foreach (var imageFile in model.ImageFile)
                        {
                            if (imageFile != null)
                            {
                                string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", uniqueFileName);

                                using (var fileStream = new FileStream(filePath, FileMode.Create))
                                {
                                    await imageFile.CopyToAsync(fileStream);
                                }

                                var postImage = new PostImage
                                {
                                    ImgUrl = "/images/" + uniqueFileName,
                                    IdPost = post.Id
                                };

                                post.PostImage.Add(postImage);
                            }
                        }
                    }
                    

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

                /*await _context.SaveChangesAsync();*/
                var log = new PostStatusLog { IdPost = post.Id, IdStatus = 2, EditDate = DateTime.Now };
                _context.Add(log);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return NotFound(ModelState);
            }
            ViewData["IdCategory"] = new SelectList(_context.Categories, "Id", "CategoryName", post.IdCategory);
            ViewData["IdUser"] = new SelectList(_context.Users, "Id", "Id", post.IdUser);
            return View(post);
            
        }

        [Authorize(Roles = "registeredUser, admin")]
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

        [Authorize(Roles = "registeredUser, admin")]
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

        [Authorize(Roles = "admin")]
        // GET: Posts
        public async Task<IActionResult> IndexModerate(string sortOrder, string searchString, string currentFilter, int? pageNumber, int? categoryId, short? statusId)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentFilter"] = searchString;
            ViewData["PageNumber"] = pageNumber != null ? pageNumber : 1;
            ViewData["CategoryId"] = categoryId;
            ViewBag.Categories = _context.Categories.ToList();
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
                    .FirstOrDefault().EditDate,
                FirstImage = post.PostImage
                .OrderBy(img => img.Id)
                .FirstOrDefault().ImgUrl
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
                case "date":
                    //students = students.OrderByDescending(s => s.EnrollmentDate);
                    appCtx = appCtx.OrderBy(s => s.EditDate);
                    break;
                case "dateDesc":
                    //students = students.OrderByDescending(s => s.EnrollmentDate);
                    appCtx = appCtx.OrderByDescending(s => s.EditDate);
                    break;
                default:
                    //students = students.OrderBy(s => s.LastName);
                    appCtx = appCtx.OrderBy(s => s.Post.Title);
                    break;
            }

            int pageSize = 4;
            return View(await PaginatedList<PostWithStatus>.CreateAsync(appCtx.AsNoTracking(), pageNumber ?? 1, pageSize));
        }
        public async Task<IActionResult> EditStatus(int postId, short statusId)
        {
           
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

        [HttpPost]
        public IActionResult DeleteImage([FromBody] DeleteImageViewModel model)
        {
            try
            {
                // Удаляем изображение с сервера
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, model.ImageUrl.TrimStart('/'));
                System.IO.File.Delete(filePath);

                // Удаляем изображение из БД
                var postImage = _context.PostImage.FirstOrDefault(pi => pi.ImgUrl == model.ImageUrl);
                if (postImage != null)
                {
                    _context.PostImage.Remove(postImage);
                    _context.SaveChanges();
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Обработка ошибок
                return Json(new { success = false, error = ex.Message });
            }
        }

        public class DeleteImageViewModel
        {
            public string ImageUrl { get; set; }
        }
    }
}
