using System.Diagnostics;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using HWM.WebApp.Models;


namespace HWM.WebApp.Controllers
{
    public class LeaderController : Controller
    {
        private readonly ILogger<LeaderController> _logger;

        public LeaderController(ILogger<LeaderController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index(int ownerId = 0)
        {
            // Should be moved to configuration section
            string path = @"D:\Database\HWM\Leader\LGCreatures_ext.json";
            string json = System.IO.File.ReadAllText(path, System.Text.Encoding.UTF8);

            IList<FollowerModel> followers =
                JsonConvert.DeserializeObject<IList<FollowerModel>>(json) ??
                throw new ArgumentException();

            IList<FollowerModel> followerList = new List<FollowerModel>();

            if (ownerId > 0)
            {   
                foreach (var follower in followers.Where(f => f.Tier != 4))
                {
                    var pool = follower.Pools.FirstOrDefault(p => p.OwnerId == ownerId);

                    if (pool != null)
                    {
                        follower.ActivePoolId = follower.Pools.IndexOf(pool);
                        followerList.Add(follower);
                    }
                }

                ViewData["HasOwner"] = true;
            }

            else
            {
                followerList = followers;

                ViewData["HasOwner"] = false;
            }

            return View(followerList);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel 
            { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
            });
        }
    }
}
