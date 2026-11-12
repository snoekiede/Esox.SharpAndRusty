using Microsoft.AspNetCore.Mvc;

namespace Esox.SharpAndRusty.Samples.LibraryAPI.Controllers
{
    public class BookController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
