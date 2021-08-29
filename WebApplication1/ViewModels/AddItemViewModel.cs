using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class AddItemViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Item name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Tags")]
        public string Tags { get; set; }

        [Required]
        public string FirstFieldValue { get; set; }

        [Required]
        public string SecondFieldValue { get; set; }

        [Required]
        public string ThirdFieldValue { get; set; }

        public string ImageUrl { get; set; }
    }
}
