using Microsoft.AspNetCore.Identity;
using StuffApp.Models.Data;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Security.Claims;

namespace StuffApp.Models.Data
{
    public class User : IdentityUser
    {
        [Required(ErrorMessage = "Введите фамилию")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Введите имя")]
        [Display(Name = "Имя")]
        public string FirstName { get; set; }
        public DateTime RegDate { get; set; }
        public string Fullname => string.Format("{0} {1}", LastName, FirstName);
        //навигационные свойства
        public ICollection<Post> Posts { get; set; }
        /*public ICollection<Subscribe> Subscribe { get; set; }*/
        public static implicit operator User(ClaimsPrincipal v)
        {
            throw new NotImplementedException();
        }
    }
} 
