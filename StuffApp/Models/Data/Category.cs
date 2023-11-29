using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StuffApp.Models.Data
{
    public class Category
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Ид")]
        public short Id { get; set; }
        [Required(ErrorMessage = "Введите категорию")]
        [Display(Name = "Категория")]
        public string CategoryName { get; set; }
        [Display(Name = "Родительская категория")]
        public short ParentCategoryId { get; set; }

    }
}
