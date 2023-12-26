using StuffApp.Models.Data;

namespace StuffApp.ViewModels.Users
{
    public class DetailsUserViewModel
    {
        public User User { get; set; }
        public ICollection<PostWithStatus> Posts { get; set; }
    }
}
