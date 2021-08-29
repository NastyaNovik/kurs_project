namespace WebApplication1.Models
{
    public class Like
    {
        public int Id { get; set; }
        public bool like { get; set; }
        public string IdUser { get; set; }
        public int IdItem { get; set; }
    }
}
