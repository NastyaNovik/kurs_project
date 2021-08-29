namespace WebApplication1.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string CommentText { get; set; }
        public string IdUser { get; set; }
        public int IdItem { get; set; }
    }
}
