using Microsoft.AspNetCore.Mvc;
using VinylShop.Api.Service;

namespace VinylShop.Api.Controllers
{
    public class AlbumIteration : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
