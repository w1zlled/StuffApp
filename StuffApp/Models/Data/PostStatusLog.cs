using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace StuffApp.Models.Data
{
    public class PostStatusLog
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "ИД")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Статус поста")]
        public short IdStatus {  get; set; }

        [Required]
        [Display(Name = "Пост")]
        public int IdPost { get; set; }

        [Required]
        [Display(Name = "Дата изменения")]
        public DateTime EditDate { get; set; } = DateTime.Now;

        // Навигационные свойства
        [Display(Name = "Статус поста")]
        [ForeignKey("IdStatus")]
        public PostStatus PostStatus { get; set; }

        [Display(Name = "Пост")]
        [ForeignKey("IdPost")]
        public Post Post{ get; set; }

        public static explicit operator PostStatusLog(DateTime v)
        {
            throw new NotImplementedException();
        }
    }
}
