using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace StuffApp.ViewModels.Posts
{
    public class EditPostViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Введите заголовок для объявления")]
        [Display(Name = "Заголовок")]
        public string Title { get; set; }

        [Display(Name = "Описание")]
        public string? Descr { get; set; }

        [Display(Name = "Адрес")]
        public string? Address { get; set; }

        [Display(Name = "Изображение")]
        public string? ImgUrl { get; set; }

        [Display(Name = "Цена")]
        public int? Price { get; set; }

        [Required]
        [Display(Name = "Категория товара")]
        public short IdCategory { get; set; }

        [Required]
        [Display(Name = "Автор")]
        public string IdUser { get; set; }
    }
}
