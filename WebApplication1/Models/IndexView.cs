using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class IndexView
    {
        public string IdUser { get; set; }
        public int IdItem { get; set; }
        public int IdCollection { get; set; }
        public string Name { get; set; }
        public string Tags { get; set; }
        public string DateOfAddition { get; set; }
        public string CollectionName { get; set; }
        public string UserName { get; set; }
        public string CommentText { get; set; }
        public int CountOfItems { get; set; }
    }
}
