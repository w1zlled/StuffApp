using System.ComponentModel.DataAnnotations;

namespace StuffApp.ViewModels.Account
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Введите E-mail")]
        [Display(Name = "E-mail")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [Display(Name = "Пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Запомнить?")]
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }
    }
}
