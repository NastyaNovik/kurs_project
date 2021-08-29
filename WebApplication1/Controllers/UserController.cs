using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebApplication1.Models;
using WebApplication1.ViewModels;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "admin")]
    public class UserController:Controller
    {
        private ApplicationContext db;
        RoleManager<IdentityRole> _roleManager;
        UserManager<User> _userManager;
        SignInManager<User> _signInManager;
        public UserController(ApplicationContext context,RoleManager<IdentityRole> roleManager, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            db=context;
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public List<Item> findItems(string id)
        {
            var finditems = from i in db.Items
                            join c in db.Collection
                            on i.IdCollection equals c.Id
                            where c.IdUser == id
                            select new Item
                            {
                                Id = i.Id,
                                Tags = i.Tags,
                                FirstFieldValue = i.FirstFieldValue,
                                SecondFieldValue = i.SecondFieldValue,
                                ThirdFieldValue = i.ThirdFieldValue,
                                Name = i.Name,
                                ImageUrl = i.ImageUrl,
                                IdCollection = i.IdCollection,
                                DateTheItemWasAdded = i.DateTheItemWasAdded
                            };
            List<Item> items = new List<Item>();
            foreach (var i in finditems)
            {
                items.Add(i);
            }
            return items;
        }
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            IQueryable<Collection> collections = db.Collection.Where(a=>a.IdUser==id);
            IQueryable<Like> likes = db.Likes.Where(a => a.IdUser == id);
            IQueryable<Comment> comments = db.Comments.Where(a => a.IdUser == id);
            User user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                IEnumerable<string> userRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, userRoles);
                if (collections.Count() != 0)
                {
                    db.Items.RemoveRange(findItems(id));
                }
                db.Collection.RemoveRange(collections);
                db.Likes.RemoveRange(likes);
                db.Comments.RemoveRange(comments);
                db.SaveChanges();
                IdentityResult result = await _userManager.DeleteAsync(user);
                if (User.Identity.Name == user.UserName)
                {
                    await _signInManager.SignOutAsync().ConfigureAwait(false);
                }
            }
            return RedirectToAction("UserList","User");
        }
        public async Task<IActionResult> Edit(string id)
        {
            User user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var allRoles = _roleManager.Roles.ToList();
                ChangeRoleViewModel model = new ChangeRoleViewModel
                {
                    UserId = user.Id,
                    UserEmail = user.Email,
                    UserRoles = userRoles,
                    AllRoles = allRoles
                };
                return View("Edit", model);
            }
            return NotFound();
        }
        [HttpPost]
        public async Task<IActionResult> Edit(string userId, List<string> roles)
        {
            User user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var addedRoles = roles.Except(userRoles);
                var removedRoles = userRoles.Except(roles);
                await _userManager.AddToRolesAsync(user, addedRoles);
                await _userManager.RemoveFromRolesAsync(user, removedRoles);
                return RedirectToAction("UserList","User");
            }
            return NotFound();
        }
        public ActionResult UserList()
        {
            return View(_userManager.Users.ToList());
        }
    }
}
