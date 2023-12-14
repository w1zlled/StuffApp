using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.CodeAnalysis;

namespace StuffApp.Models.Data
{
    public class Post
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "ИД")]
        public int Id { get; set; }

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

        // Навигационные свойства
        [Display(Name = "Категория товара")]
        [ForeignKey("IdCategory")]
        public Category Category { get; set; }

        [Display(Name = "Автор")]
        [ForeignKey("IdUser")]
        public User User { get; set; }

        public ICollection<Category> Categories { get; set; }
        public ICollection<PostStatusLog> PostStatusLog { get; set; }
    }
}
