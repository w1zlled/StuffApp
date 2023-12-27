using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StuffApp.Models;
using StuffApp.Models.Data;
using StuffApp.ViewModels.Users;
using System.Data;

namespace StuffApp.Controllers
{
    /*[Authorize(Roles = "admin")]*/
    public class UsersController : Controller
    {
        private readonly AppCtx _context;
        UserManager<User> _userManager;
        RoleManager<IdentityRole> _roleManager;
        public UsersController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, AppCtx context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }
        // отображение списка пользователей
        // действия для начальной страницы Index
        [Authorize(Roles = "admin")]
        public IActionResult Index() => View(_userManager.Users.ToList());

       
        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                // Обработка ошибки, если идентификатор пользователя не передан
                return BadRequest("Invalid user ID");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                // Обработка ошибки, если пользователь не найден
                return NotFound("User not found");
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                // Пользователь успешно удален
                return RedirectToAction("Index"); // Перенаправление на страницу после успешного удаления
            }
            else
            {
                return View("Error");
            }
        }

        // так как роли задаются для каждого пользователя системы отдельно,
        // то можно перенести методы работы с ними в контроллер Users, где мы можеи получить доступ ко всем пользователям системы
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> EditRoles(string userId)
        {
            // получаем пользователя
            User user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                // получем список ролей пользователя
                var userRoles = await _userManager.GetRolesAsync(user);
                var allRoles = _roleManager.Roles.ToList();
                ChangeRoleViewModel model = new ChangeRoleViewModel
                {
                    UserId = user.Id,
                    UserEmail = user.Email,
                    UserRoles = userRoles,
                    AllRoles = allRoles
                };
                return View(model);
            }

            return NotFound();
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> EditRoles(string userId, List<string> roles)
        {
            // получаем пользователя
            User user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                // получем список ролей пользователя
                var userRoles = await _userManager.GetRolesAsync(user);
                // получаем все роли
                var allRoles = _roleManager.Roles.ToList();
                // получаем список ролей, которые были добавлены
                var addedRoles = roles.Except(userRoles);
                // получаем роли, которые были удалены
                var removedRoles = userRoles.Except(roles);

                await _userManager.AddToRolesAsync(user, addedRoles);

                await _userManager.RemoveFromRolesAsync(user, removedRoles);

                return RedirectToAction(nameof(Index));
            }

            return NotFound();
        }

        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                /*return RedirectToAction("Index");*/
                return NotFound();
            }
            /*string userId = id.ToString();*/
            // получаем пользователя
            /*User user = await _userManager.FindByIdAsync(id);*/

   

            var currentUserId = _userManager.GetUserId(User);
            bool isSubscribed = await _context.Subscribe
                .AnyAsync(s => s.IdSubscriber == currentUserId && s.IdSeller == id);

            var userWithPosts = await _context.Users
                .Where(user => user.Id == id)
                .Include(user => user.Posts)
                    .ThenInclude(post => post.Category)
                .Include(user => user.Posts)
                    .ThenInclude(post => post.PostImage)
                .FirstOrDefaultAsync();

            if (userWithPosts != null)
            {
                var postsWithImages = userWithPosts.Posts.Select(post => new PostWithStatus
                {
                    Post = post,
                    FirstImage = post.PostImage
                        .OrderBy(img => img.Id)
                        .FirstOrDefault()?.ImgUrl
                });

                var viewModel = new DetailsUserViewModel
                {
                    User = userWithPosts,
                    Posts = postsWithImages.ToList(),
                    IsCurrentUserSubscribed = isSubscribed
                };

                return View(viewModel);
            }


            return NotFound();
        }

        public async Task<IActionResult> Subscribe(string? id)
        {
            var currentUserId = _userManager.GetUserId(User);
            // получаем пользователя
            if (id != null && currentUserId != null)
            {
                Subscribe subscribe = new Subscribe { IdSubscriber = currentUserId, IdSeller = id };
                _context.Add(subscribe);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = id });
            }

            return NotFound();
        }

        public async Task<IActionResult> Unsubscribe(string? id)
        {
            var currentUserId = _userManager.GetUserId(User);
            // получаем пользователя
            if (id != null && currentUserId != null)
            {
                // Ищем подписку пользователя на продавца
                var subscription = await _context.Subscribe
                    .Where(s => s.IdSubscriber == currentUserId && s.IdSeller == id)
                    .FirstOrDefaultAsync();

                // Если подписка найдена, удаляем ее и сохраняем изменения
                if (subscription != null)
                {
                    _context.Subscribe.Remove(subscription);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Details), new { id = id });
            }

            return NotFound();
        }

        public async Task<IActionResult> SubscribeIndex(string? searchString)
        {
            ViewData["CurrentFilter"] = searchString;
            var currentUserId = _userManager.GetUserId(User);
            if (!String.IsNullOrEmpty(searchString))
            {
                /*appCtx = appCtx.Where(s => s.Post.Title.Contains(searchString));*/
                var subscribes = _context.Subscribe
                .Join(
                    _context.Users,
                    subscribe => subscribe.IdSeller,
                    user => user.Id,
                    (subscribe, user) => new { Subscribe = subscribe, User = user }
                )
                .Where(s => s.Subscribe.IdSubscriber == currentUserId)
                .AsEnumerable() // Переход к выполнению запроса на стороне клиента
                .Where(s => s.User.Fullname.Contains(searchString))
                .Select(s => new IndexSubscribeViewModel
                {
                    Seller = s.User,
                    SubscribeDate = s.Subscribe.SubscribeDate
                })
                .ToList();

                return View(subscribes);
            } else
            {
                var subscribes = _context.Subscribe
                .Join(
                    _context.Users,
                    subscribe => subscribe.IdSeller,
                    user => user.Id,
                    (subscribe, user) => new { Subscribe = subscribe, User = user }
                )
                .Where(s => s.Subscribe.IdSubscriber == currentUserId)
                .Select(s => new IndexSubscribeViewModel
                {
                    Seller = s.User,
                    SubscribeDate = s.Subscribe.SubscribeDate
                })
                .ToList();

                return View(subscribes);
            }


            return NotFound();
        }
    }
}
