using System.Collections.Generic;
using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    public class IndexViewModel
    {
        public IEnumerable<Collection> Collections{ get; set; }
        public IEnumerable<Item> Items { get; set; }
        public IEnumerable<Comment> Comments { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public IEnumerable<IndexView> IndexView { get; set; }
        public int CountOfLIkes { get; set; }
    }
}
