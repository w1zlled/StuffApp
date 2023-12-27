using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace StuffApp.Models.Data
{
    public class Subscribe
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "ИД")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Автор")]
        public string IdSeller { get; set; }

        /*[Display(Name = "Автор")]
        [ForeignKey("IdSeller")]
        public User Seller { get; set; }*/

        [Required]
        [Display(Name = "Подписчик")]
        public string IdSubscriber { get; set; }

        [Required]
        [Display(Name = "Дата подписки")]
        public DateTime SubscribeDate {  get; set; } = DateTime.Now;

        /*[Display(Name = "Автор")]
        [ForeignKey("IdSubscriber")]
        public User Subscriber { get; set; }*/
    }
}
