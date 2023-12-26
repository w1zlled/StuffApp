using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace StuffApp.Models.Data
{
    public class PostImage
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "ИД")]
        public int Id { get; set; }

        [Display(Name = "Изображение")]
        public string ImgUrl { get; set; }

        [Required(ErrorMessage = "Поле 'Изображение' обязательно для заполнения.")]
        [NotMapped]
        [Display(Name = "Загрузить изображение")]
        public IFormFile ImageFile { get; set; }

        [Required]
        [Display(Name = "Пост")]
        public int IdPost { get; set; }

        [Display(Name = "Пост")]
        [ForeignKey("IdPost")]
        public Post Post{ get; set; }
    }
}
