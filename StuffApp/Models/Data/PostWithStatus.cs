namespace StuffApp.Models.Data
{
    public class PostWithStatus
    {
        public Post Post { get; set; }
        public PostStatus LatestStatus { get; set; }
        public DateTime EditDate { get; set; }
    }
}
