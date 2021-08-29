using System;

namespace WebApplication1.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Tags { get; set; }
        public string FirstFieldValue { get; set; }
        public string SecondFieldValue { get; set; }
        public string ThirdFieldValue { get; set; }
        public int IdCollection { get; set; }
        public DateTime DateTheItemWasAdded { get; set; }
        public string ImageUrl { get; set; }
    }
}
