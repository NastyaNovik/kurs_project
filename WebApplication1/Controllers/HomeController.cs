using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using WebApplication1.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using WebApplication1.ViewModels;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.Security.Claims;
using WebApplication1.AppData;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        IWebHostEnvironment _appEnvironment;
        private ApplicationContext db;
        private static string cloudName;
        private static string ApiKey;
        private static string ApiSecret;
        private static string DefaultImageUrl;
        public int countOfItems=0;
        public HomeController(ApplicationContext context, IWebHostEnvironment appEnvironment, IOptions<AppDataOptions> settings)
        {
            db = context;
            _appEnvironment = appEnvironment;
            cloudName = settings.Value.CloudName;
            ApiKey = settings.Value.ApiKey;
            ApiSecret = settings.Value.ApiSecret;
            DefaultImageUrl = settings.Value.DefaultImageUrl;
        }
        Account account = new Account(cloudName, ApiKey, ApiSecret);

        public List<IndexView> listFromQuery(IQueryable<IndexView> query)
        {
            List<IndexView> items = new List<IndexView>();
            foreach (var i in query)
            {
                items.Add(i);
            }
            countOfItems = items.Count();
            return items;
        }
        public string getCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
        public bool checkHasTheCurrentUserCreatedTheCollection(int Id)
        {
            return Equals(getCurrentUserId(), db.Collection.Find(Id).IdUser);
        }
        public bool checkIsItPersonalAreaOfCurrentUser(string Id)
        {
            return Equals(getCurrentUserId(), db.Users.Find(Id).Id);
        }
        public List<IndexView> getLastAddedItems()
        {
            var items = (from item in db.Items
                         join collection in db.Collection on item.IdCollection equals collection.Id
                         join user in db.Users on collection.IdUser equals user.Id
                         orderby item.DateTheItemWasAdded descending
                         select new IndexView()
                         {
                             IdUser = user.Id,
                             IdItem = item.Id,
                             Name = item.Name,
                             DateOfAddition = item.DateTheItemWasAdded.ToShortDateString(),
                             Tags = item.Tags,
                             CollectionName = collection.Name,
                             UserName = user.UserName
                         }).Take(10);
            return listFromQuery(items);
        }
        public List<IndexView> getLargestCollections()
        {
            var collections = (from collection in db.Collection
                               join user in db.Users on collection.IdUser equals user.Id
                               orderby collection.CountOfItems descending
                               select new IndexView()
                               {
                                   IdUser = user.Id,
                                   IdCollection = collection.Id,
                                   CollectionName = collection.Name,
                                   UserName = user.UserName,
                                   CountOfItems = collection.CountOfItems
                               }).Take(5);
            return listFromQuery(collections);
        }
        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
                ViewBag.UserId = getCurrentUserId();
            ViewBag.items = await Task.Run(() => getLastAddedItems());
            ViewBag.collect = await Task.Run(() => getLargestCollections());
            return View("Index");
        }
        public async Task<IActionResult> PersonalArea(string Id, int page = 1)
        {
            if (User.Identity.IsAuthenticated)
            {
                ViewBag.check = checkIsItPersonalAreaOfCurrentUser(Id);
            }
            int pageSize = 3;
            ViewBag.UserId = Id;
            IQueryable<Collection> source = db.Collection.Where(a => a.IdUser == Id);
            var collections = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return View(collectionViewModel(source.Count(), page, pageSize, collections));
        }
        public string LoadImage(IFormFile uploadedFile)
        {
            Cloudinary cloudinary = new Cloudinary(account);
            if (uploadedFile != null)
            {
                var uploadParams = new ImageUploadParams()
                {

                    Folder = "images",
                    File = new FileDescription(uploadedFile.FileName, uploadedFile.OpenReadStream()),
                };
                var uploadResult = cloudinary.Upload(uploadParams);
                return Convert.ToString(uploadResult.Url);
            }
            else return DefaultImageUrl;
        }
        public async Task<IActionResult> AddCollection(IFormFile uploadedFile, AddCollectionViewModel model, string Id)
        {
            Collection collection = new Collection
            {
                Name = model.Name,
                Description = model.Description,
                Topic = model.Topic,
                ImageUrl = LoadImage(uploadedFile),
                FirstFieldName = model.FirstFieldName,
                SecondFieldName = model.SecondFieldName,
                ThirdFieldName = model.ThirdFieldName,
                IdUser = Id
            };
            db.Collection.Add(collection);
            await db.SaveChangesAsync();
            return RedirectToAction("PersonalArea", new { id = Id });
        }
        public async Task<IActionResult> GoToAddCollection(string Id)
        {
            ViewBag.UserId = Id;
            List<TopicForCollection> topicList = await db.Topics.OrderBy(a=>a.Topic).ToListAsync();
            ViewBag.topics = topicList;
            return View("AddCollection");
        }
        public void itemAttributesNames(Collection collection)
        {
            ViewBag.FirstFieldName = collection.FirstFieldName;
            ViewBag.SecondFieldName = collection.SecondFieldName;
            ViewBag.ThirdFieldName = collection.ThirdFieldName;
        }
        public IndexViewModel itemViewModel(int count, int page, int pageSize, IEnumerable<Item> items)
        {
            IndexViewModel viewModel = new IndexViewModel
            {
                PageViewModel = new PageViewModel(count, page, pageSize),
                Items = items
            };
            return viewModel;
        }
        public IndexViewModel itemView(IEnumerable<IndexView> items,int count, int page, int pageSize)
        {
            IndexViewModel viewModel = new IndexViewModel
            {
                PageViewModel = new PageViewModel(count, page, pageSize),
                IndexView = items
            };
            return viewModel;
        }
        public IndexViewModel collectionViewModel(int count, int page, int pageSize, IEnumerable<Collection> collections)
        {
            IndexViewModel viewModel = new IndexViewModel
            {
                PageViewModel = new PageViewModel(count, page, pageSize),
                Collections = collections
            };
            return viewModel;
        }
        public async Task<IActionResult> CollectionPage(int Id, string selectedSort, int page = 1, int pageSize = 3)
        {
            if (User.Identity.IsAuthenticated)
            {
                ViewBag.check = checkHasTheCurrentUserCreatedTheCollection(Id);
            }
            Collection collection = await db.Collection.FindAsync(Id);
            ViewBag.Id = collection.Id;
            itemAttributesNames(collection);
            IQueryable<Item> source = db.Items;
            switch (selectedSort)
            {
                case "Date of addition the item \u21D1":
                    source = db.Items.OrderBy(a => a.DateTheItemWasAdded);
                    break;
                case "Date of addition the item \u21D3":
                    source = db.Items.OrderByDescending(a => a.DateTheItemWasAdded);
                    break;
                case "Name":
                    source = db.Items.OrderBy(a => a.Name);
                    break;
                default:
                    source = db.Items;
                    break;
            }
            var items = source.Where(p => p.IdCollection == collection.Id).ToList().Skip((page - 1) * pageSize).Take(pageSize);
            int count = source.Where(p => p.IdCollection == collection.Id).Count();
            return View(itemViewModel(count, page, pageSize, items));
        }
        public async Task<IActionResult> GoToAddItem(int Id)
        {
            Collection collection = await db.Collection.FindAsync(Id);
            await Task.Run(() => itemAttributesNames(collection));
            ViewBag.Id = Id;
            return View("AddItem");
        }
        public DateTime getCurrentTime()
        {
            DateTime utcTime = DateTime.UtcNow;
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Belarus Standard Time");
            DateTime localTime = TimeZoneInfo.ConvertTime(utcTime, tz);
            return localTime;
        }
        public async Task<IActionResult> AddItemIntoCollection(IFormFile uploadedFile, AddItemViewModel model, int Id)
        {
            Item item = new Item
            {
                Name = model.Name,
                Tags = model.Tags,
                FirstFieldValue = model.FirstFieldValue,
                SecondFieldValue = model.SecondFieldValue,
                ThirdFieldValue = model.ThirdFieldValue,
                ImageUrl = LoadImage(uploadedFile),
                IdCollection = Id,
                DateTheItemWasAdded = getCurrentTime()
            };
            db.Items.Add(item);
            Collection collection = await db.Collection.FindAsync(Id);
            collection.CountOfItems++;
            await db.SaveChangesAsync();
            return RedirectToAction("CollectionPage", new { id = Id });           
        }
        public List<Like> findLikes(int id)
        {
            var findlikes = from like in db.Likes
                            join item in db.Items
                            on like.IdItem equals item.Id
                            join collection in db.Collection
                            on item.IdCollection equals collection.Id
                            where collection.Id == id
                            select new Like
                            {
                                Id = like.Id,
                                like = like.like,
                                IdItem = like.IdItem,
                                IdUser =like.IdUser
                            };
            List<Like> likes = new List<Like>();
            foreach (var like in findlikes)
            {
                likes.Add(like);
            }
            return likes;
        }
        public List<Comment> findComments(int id)
        {
            var findcomments = from comment in db.Comments
                            join item in db.Items
                            on comment.IdItem equals item.Id
                            join collection in db.Collection
                            on item.IdCollection equals collection.Id
                            where collection.Id == id
                            select new Comment
                            {
                                Id = comment.Id,
                                CommentText = comment.CommentText,
                                IdItem = comment.IdItem,
                                IdUser = comment.IdUser
                            };
            List<Comment> comments = new List<Comment>();
            foreach (var comment in findcomments)
            {
                comments.Add(comment);
            }
            return comments;
        }
        public async Task<IActionResult> DeleteCollection(string[] selectedCollections, string Id)
        {
            for (int i = 0; i < selectedCollections.Length; i++)
            {
                Collection collection = await db.Collection.FindAsync(Convert.ToInt32(selectedCollections[i]));
                IQueryable<Item> items = db.Items.Where(i => i.IdCollection == collection.Id);                  
                if (collection != null)
                {
                    if (items.Count() != 0)
                    {
                        db.Likes.RemoveRange(findLikes(Convert.ToInt32(selectedCollections[i])));
                        db.Comments.RemoveRange(findComments(Convert.ToInt32(selectedCollections[i])));
                    }
                    db.Items.RemoveRange(items);
                    db.Collection.Remove(collection);
                    await db.SaveChangesAsync();
                }
            }
            return RedirectToAction("PersonalArea", new { id = Id });
        }
        public async Task<IActionResult> DeleteItem(string[] selectedItems, int Id)
        {            
            Collection collection = db.Collection.Find(Id);
            for (int i = 0; i < selectedItems.Length; i++)
            {
                Item item = await db.Items.FindAsync(Convert.ToInt32(selectedItems[i])); ;
                IQueryable<Like> likes = db.Likes.Where(a => a.IdItem==item.Id);
                IQueryable<Comment> comments = db.Comments.Where(a => a.IdItem == item.Id);
                if (item != null)
                {
                    db.Likes.RemoveRange(likes);
                    db.Comments.RemoveRange(comments);
                    db.Items.Remove(item);
                    collection.CountOfItems--;
                    await db.SaveChangesAsync();
                }
            }
            return RedirectToAction("CollectionPage", new { id = Id });
        }
        public async Task<IActionResult> GoToEditCollection(int Id, string IdUser)
        {
            Collection collection = await db.Collection.FindAsync(Id);
            AddCollectionViewModel model = new AddCollectionViewModel();
            model.Name = collection.Name;
            model.Description = collection.Description;
            model.ImageUrl = collection.ImageUrl;
            model.Topic = collection.Topic;
            ViewBag.Id = Id;
            ViewBag.IdUser = IdUser;
            return View("EditCollection", model);
        }
        public async Task<IActionResult> Edit(AddCollectionViewModel model, int Id, string IdUser)
        {
            Collection collection = await db.Collection.FindAsync(Id);
            collection.Name = model.Name;
            collection.Description = model.Description;
            await db.SaveChangesAsync();
            return RedirectToAction("PersonalArea", new { id = IdUser });
        }
        public async Task<IActionResult> GoToEditItem(int Id, int IdCollection)
        {
            Collection collection = await db.Collection.FindAsync(IdCollection);
            await Task.Run(()=>itemAttributesNames(collection));
            Item item = await db.Items.FindAsync(Id);
            AddItemViewModel model = new AddItemViewModel();
            model.Name = item.Name;
            model.Tags = item.Tags;
            model.FirstFieldValue = item.FirstFieldValue;
            model.SecondFieldValue = item.SecondFieldValue;
            model.ThirdFieldValue = item.ThirdFieldValue;
            ViewBag.Id = IdCollection;
            ViewBag.IdItem = Id;
            model.Id = item.Id;
            return View("EditItem", model);
        }
        public async Task<IActionResult> EditItem(AddItemViewModel model, int Id, int IdItem)
        {            
            Item item = await db.Items.FindAsync(IdItem);
            item.Name = model.Name;
            item.Tags = model.Tags;
            item.FirstFieldValue = model.FirstFieldValue;
            item.SecondFieldValue = model.SecondFieldValue;
            item.ThirdFieldValue = model.ThirdFieldValue;
            await db.SaveChangesAsync();
            return RedirectToAction("CollectionPage", new { id = Id });
        }
        public List<IndexView> getCommentsAndAuthors(int Id)
        {
            var com = (from comment in db.Comments.Where(a => a.IdItem == Id)
                       join user in db.Users on comment.IdUser equals user.Id
                       select new IndexView()
                       {
                           UserName = user.UserName,
                           CommentText = comment.CommentText,
                           IdUser = user.Id,
                       });
            return listFromQuery(com);
        }
        public async Task<IActionResult> ItemPage(int Id)
        {
            ViewBag.Comments = await Task.Run(()=>getCommentsAndAuthors(Id));
            ViewBag.IdItem = Id;
            if (User.Identity.IsAuthenticated)
            {
                ViewBag.UserId = await Task.Run(() => getCurrentUserId());
                ViewBag.username = User.Identity.Name;
                int countOfLikesUser = db.Likes.Where(i => i.IdItem == Id).Where(u => u.IdUser == getCurrentUserId()).
                Where(val => val.like == true).Count();
                ViewBag.countOfLikesUser = countOfLikesUser;
            }
            IEnumerable<Item> item = db.Items.Where(i => i.Id == Id);
            IndexViewModel viewModel = new IndexViewModel
            {
                Items = item,
                CountOfLIkes = db.Likes.Where(i => i.IdItem == Id).Where(val => val.like == true).Count(),
            };
            return View("ItemPage", viewModel);
        }
       
        public List<IndexView> getItemsFromSearching(string search)
        {
            string s = $"{search}*";
            var searchingitems = (from item in db.Items
                                  join collection in db.Collection
                                  on item.IdCollection equals collection.Id
                                  join user in db.Users on collection.IdUser equals user.Id
                                  where EF.Functions.Contains(collection.Description, $"\"{s}\"") ||
                                  EF.Functions.Contains(collection.Topic, $"\"{s}\"") ||
                                  EF.Functions.Contains(item.Name, $"\"{s}\"") ||
                                  EF.Functions.Contains(item.Tags, $"\"{s}\"") ||
                                  EF.Functions.Contains(item.FirstFieldValue, $"\"{s}\"") ||
                                  EF.Functions.Contains(item.SecondFieldValue, $"\"{s}\"") ||
                                  EF.Functions.Contains(item.ThirdFieldValue, $"\"{s}\"")
                                  select new IndexView()
                                  {
                                      UserName = user.UserName,
                                      Name = item.Name,
                                      IdItem = item.Id,
                                      IdUser = user.Id,
                                      CollectionName = collection.Name,
                                  });
            return listFromQuery(searchingitems);
        }
        public IEnumerable<IndexView> getAllItems()
        {
            var searchingitems = (from item in db.Items
                                  join collection in db.Collection
                                  on item.IdCollection equals collection.Id
                                  join user in db.Users on collection.IdUser equals user.Id
                                  select new IndexView()
                                  {
                                      UserName = user.UserName,
                                      Name = item.Name,
                                      IdItem = item.Id,
                                      IdUser = user.Id,
                                      CollectionName = collection.Name,
                                  });
            return searchingitems;
        }
        public IEnumerable<IndexView> getAllCollections()
        {
            var collections =  from collection in db.Collection
                               join user in db.Users on collection.IdUser equals user.Id
                               orderby collection.CountOfItems descending
                               select new IndexView()
                               {
                                   IdUser = user.Id,
                                   IdCollection = collection.Id,
                                   CollectionName = collection.Name,
                                   UserName = user.UserName,
                                   CountOfItems = collection.CountOfItems
                               };
            return collections;
        }
        public async Task<IActionResult> SearchResult(string user, string search, int page = 1, int pageSize = 10)
        {
            ViewBag.search=search;
            List<User> userList = await db.Users.ToListAsync();
            userList.Insert(0, new User{ UserName = "Not Selected"});
            ViewBag.users = userList;
            if (user != null && user != "Not Selected")
            {
                if (search == null)
                {
                    countOfItems = getAllItems().Where(p => p.UserName == user).Count();
                    return View(itemView(getAllItems().Where(p => p.UserName == user).Skip((page - 1) * pageSize).Take(pageSize), countOfItems, page, pageSize));
                }
                else
                {
                    countOfItems = getItemsFromSearching(search).Where(p => p.UserName == user).Count();
                    return View(itemView(getItemsFromSearching(search).Where(p => p.UserName == user).Skip((page - 1) * pageSize).Take(pageSize), countOfItems, page, pageSize));
                }
            }
            else
            {
                if (search == null)
                {
                    countOfItems = getAllItems().Count();
                    return View(itemView(getAllItems().Skip((page - 1) * pageSize).Take(pageSize), countOfItems, page, pageSize));
                }
                else
                {
                    countOfItems = getItemsFromSearching(search).Count();
                    return View(itemView(getItemsFromSearching(search).Skip((page - 1) * pageSize).Take(pageSize), countOfItems, page, pageSize));
                }
            }
        }
        public async Task<IActionResult> AllCollections(string user, int page = 1, int pageSize = 10)
        {
            List<User> userList = await db.Users.ToListAsync();
            userList.Insert(0, new User { UserName = "Not Selected" });
            ViewBag.users = userList;
            if (user != null && user != "Not Selected")
            {
                countOfItems = getAllCollections().Where(p => p.UserName == user).Count();
                return View(itemView(getAllCollections().Where(p => p.UserName == user).Skip((page - 1) * pageSize).Take(pageSize), countOfItems, page, pageSize));
            }
            else
            {
                countOfItems = getAllCollections().Count();
                return View(itemView(getAllCollections().Skip((page - 1) * pageSize).Take(pageSize), countOfItems, page, pageSize));
            }
        }
    }
}
