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
            string path = @"D:\Database\HWM\Leader\LGCreatures_ext.json";
            string json = System.IO.File.ReadAllText(path, System.Text.Encoding.UTF8);

            IEnumerable<FollowerModel> followerList =
                JsonConvert.DeserializeObject<IEnumerable<FollowerModel>>(json) ??
                throw new ArgumentException();

            if (ownerId > 0)
            {
                followerList = 
                    followerList.Where(f => f.Owners.Contains(ownerId) && f.Tier != 4);
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
