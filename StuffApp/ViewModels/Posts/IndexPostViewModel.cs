using StuffApp.Models.Data;

namespace StuffApp.ViewModels.Posts
{
    public class IndexPostViewModel
    {
        public List<PostWithStatus> PostWithStatus { get; set; }
        public List<Category> Categories { get; set; }
        public int? SelectedCategoryId { get; set; }
    }
}
