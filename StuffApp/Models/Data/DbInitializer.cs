using System.Diagnostics;

namespace StuffApp.Models.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppCtx context)
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

        }
    }
}
