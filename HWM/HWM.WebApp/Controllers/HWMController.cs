using HWM.WebApp.Models;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;
using System.Diagnostics;

namespace HWM.WebApp.Controllers
{
    public class HWMController : Controller
    {
        private readonly ILogger<HWMController> _logger;

        public HWMController(ILogger<HWMController> logger)
        {
            _logger = logger;
        }
        
        public IActionResult Leader()
        {
            string path = @"D:\Database\HWM\Leader\LGCreatures_ext.json";
            string json = System.IO.File.ReadAllText(path, System.Text.Encoding.UTF8);
            IEnumerable<FollowerModel> followerList =
                JsonConvert.DeserializeObject<IEnumerable<FollowerModel>>(json) ??
                throw new ArgumentException();

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
