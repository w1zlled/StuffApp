using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace StuffApp.Models.Data
{
    public class PostStatus
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Ид")]
        public short Id { get; set; }
        [Required(ErrorMessage = "Введите статус поста")]
        [Display(Name = "Статус поста")]
        public string StatusName { get; set; }

        public ICollection<PostStatusLog> PostStatusLogs {  get; set; }
    }
}
