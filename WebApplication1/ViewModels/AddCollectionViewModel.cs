using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class AddCollectionViewModel
    {
        [Required]
        [Display(Name = "Name for collection")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Topic")]
        public string Topic { get; set; }

        public string ImageUrl { get; set; }

        [Required]
        [Display(Name = "Attribute for collection elements")]
        public string FirstFieldName { get; set; }

        [Required]
        [Display(Name = "Attribute for collection elements")]
        public string SecondFieldName { get; set; }

        [Required]
        [Display(Name = "Attribute for collection elements")]
        public string ThirdFieldName { get; set; }

        [Required]
        public int CountOfItems { get; set; }
    }
}
