using StuffApp.Models.Data;

namespace StuffApp.ViewModels.Users
{
    public class DetailsUserViewModel
    {
        public User User { get; set; }
        public ICollection<Post> Posts { get; set; }
    }
}
