using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using WebApplication1.ViewModels;

namespace WebApplication1.TagHelpers
{
    public class PageLinkTagHelper : TagHelper
    {
        private IUrlHelperFactory urlHelperFactory;
        public PageLinkTagHelper(IUrlHelperFactory helperFactory)
        {
            urlHelperFactory = helperFactory;
        }
        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }
        public PageViewModel PageModel { get; set; }
        public string PageAction { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            IUrlHelper urlHelper = urlHelperFactory.GetUrlHelper(ViewContext);
            output.TagName = "div";
            TagBuilder tag = new TagBuilder("ul");
            tag.AddCssClass("pagination");
            TagBuilder currentItem = CreateTag(PageModel.PageNumber, urlHelper);
            if (PageModel.HasPreviousPage)
            {
                for (int i = PageModel.PageNumber-1; i >0;i--)
                {
                    TagBuilder prevItem = CreateTag(PageModel.PageNumber - i, urlHelper);
                    tag.InnerHtml.AppendHtml(prevItem);
                }
            }
            tag.InnerHtml.AppendHtml(currentItem);
            if (PageModel.HasNextPage)
            {
                int difference = PageModel.TotalPages - PageModel.PageNumber;
                for (int i = 1; i <= difference; i++)
                {
                    TagBuilder nextItem = CreateTag(PageModel.PageNumber + i, urlHelper);
                    tag.InnerHtml.AppendHtml(nextItem);
                }
            }
            output.Content.AppendHtml(tag);
        }

        TagBuilder CreateTag(int pageNumber, IUrlHelper urlHelper)
        {
            TagBuilder item = new TagBuilder("li");
            TagBuilder link = new TagBuilder("a");
            if (pageNumber == this.PageModel.PageNumber)
            {
                item.AddCssClass("active");
            }
            else
            {
                link.Attributes["href"] = urlHelper.Action(PageAction, new { page = pageNumber });
            }
            item.AddCssClass("page-item");
            link.AddCssClass("page-link");
            link.InnerHtml.Append(pageNumber.ToString());
            item.InnerHtml.AppendHtml(link);
            return item;
        }
    }
}
