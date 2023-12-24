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

        /*public async Task<IActionResult> Delete(int? id)
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
        }*/
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
            var userWithPosts = await _context.Users
            .Where(user => user.Id == id)
            .Include(user => user.Posts)
            .ThenInclude(post => post.Category)
            .FirstOrDefaultAsync();

            if (userWithPosts != null)
            {
                // Создание объекта DetailsUserViewModel с данными пользователя и постов
                var viewModel = new DetailsUserViewModel
                {
                    User = userWithPosts,
                    Posts = userWithPosts.Posts
                };

                return View(viewModel);
            }

            /*if (user != null)
            {
                *//*User model = new User
                {
                    UserId = user.Id,
                    UserEmail = user.Email,
                    UserRoles = userRoles,
                    AllRoles = allRoles
                };*/
                /*return RedirectToAction("Index");*/
                /*return View(appCtx);*//*
            }*/
            /*return RedirectToAction("Index");*/
            /*return View(model);*/
            return NotFound();
        }
    }
}
