using System.ComponentModel.DataAnnotations;

namespace StuffApp.ViewModels.Categories
{
    public class EditCategoryViewModel
    {
        public short? Id { get; set; }
        [Required(ErrorMessage = "Введите название категории")]
        [Display(Name = "Категория")]
        public string CategoryName { get; set; }
        [Display(Name = "Родительская категория")]
        public short ParentCategoryId { get; set; }
    }
}
