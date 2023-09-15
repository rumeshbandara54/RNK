using Microsoft.AspNetCore.Mvc;

namespace RnkApi.Controllers
{
    public class employeeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
