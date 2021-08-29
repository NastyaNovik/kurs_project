using Microsoft.AspNetCore.SignalR;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Controllers;
using WebApplication1.Models;

namespace WebApplication1.SignalR
{
    public class ItemCommentsLikes : Hub
    {
        private ApplicationContext db;
        public ItemCommentsLikes(ApplicationContext context)
        {
            db = context;
        }
        public async Task Send(string message, int IdItem,string UserId, string UserName)
        {
            Comment comment = new Comment
            {
                CommentText = message,
                IdUser = UserId,
                IdItem = IdItem
            };
            db.Comments.Add(comment);
            await db.SaveChangesAsync();
            User user = db.Users.Find(UserId);
            UserName = user.UserName;
            await Clients.All.SendAsync("Send", message, IdItem, UserId, UserName);
        }
        public async Task Like(int IdItem, string UserId)
        {
            int countUserLikes= db.Likes.Where(i => i.IdItem == IdItem).Where(u => u.IdUser == UserId).Count();
            if (countUserLikes == 0)
            {
                Like like = new Like
                {
                    like = true,
                    IdUser = UserId,
                    IdItem = IdItem
                };
                db.Likes.Add(like);
            }
            else
            {
                Like oldLike = db.Likes.Where(i => i.IdItem == IdItem).Where(u => u.IdUser == UserId).First();
                oldLike.like = true;
            }            
            await db.SaveChangesAsync();
            await Clients.All.SendAsync("Like",IdItem, UserId);
        }
        public async Task DisLike(int IdItem, string UserId)
        {
            Like dislike = db.Likes.Where(i => i.IdItem == IdItem).Where(u => u.IdUser == UserId).First();
            dislike.like = false;
            await db.SaveChangesAsync();
            await Clients.All.SendAsync("DisLike", IdItem, UserId);
        }
    }

}
