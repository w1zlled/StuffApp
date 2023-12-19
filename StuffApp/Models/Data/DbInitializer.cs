using Microsoft.AspNetCore.Identity;
using System.Diagnostics;

namespace StuffApp.Models.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(AppCtx context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            //context.Database.EnsureCreated();

            // Look for any students.
            if (context.PostStatus.Any())
            {
                return;   // DB has been seeded
            }

            var postStatuses = new PostStatus[]
            {
                new PostStatus { StatusName = "Черновик" },
                new PostStatus { StatusName = "На модерации" },
                new PostStatus { StatusName = "Опубликовано" },
            };
            foreach (PostStatus s in postStatuses)
            {
                context.PostStatus.Add(s);
            }
            context.SaveChanges();
            var mainCategory = new Category
            {
                CategoryName = "Главная",
                ParentCategoryId = 1
            };
            context.Categories.Add(mainCategory);
            context.SaveChanges();


            string adminEmail = "admin@mail.ru";
            string password = "123Vv.";

            if (await roleManager.FindByNameAsync("admin") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("admin"));
            }
            if (await roleManager.FindByNameAsync("registeredUser") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("registeredUser"));
            }

            if (await userManager.FindByNameAsync(adminEmail) == null)
            {
                User admin = new User
                {
                    Email = adminEmail,
                    UserName = adminEmail,
                    LastName = "Филин",
                    FirstName = "Виталий",
                    EmailConfirmed = true
                };

                IdentityResult result = await userManager.CreateAsync(admin, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "admin");
                }
            }
        }
    }
}
