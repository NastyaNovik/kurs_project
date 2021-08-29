using System;

namespace WebApplication1.Models
{
    public class Collection
    {
        public int Id { get; set; }
        public string Name { get; set; } 
        public string Description { get; set; }
        public string Topic { get; set; }
        public string ImageUrl { get; set; }
        public string FirstFieldName { get; set; }
        public string SecondFieldName { get; set; }
        public string ThirdFieldName { get; set; }
        public int CountOfItems { get; set; }
        public string IdUser { get; set; }
    }
}
