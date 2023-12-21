using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StuffApp.Models;
using StuffApp.ViewModels.Users;
using System.Data;

namespace StuffApp.Controllers
{
    [Authorize(Roles = "admin")]
    public class UsersController : Controller
    {
        UserManager<User> _userManager;
        RoleManager<IdentityRole> _roleManager;
        public UsersController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        // отображение списка пользователей
        // действия для начальной страницы Index
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

        /*public async Task<IActionResult> Details(int? id)
        {
            // получаем пользователя
            User user = await _userManager.FindByIdAsync(id);
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
        }*/
    }
}
