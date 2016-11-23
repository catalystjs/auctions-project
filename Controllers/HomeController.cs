// ASP.NET CORE Libraries
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
// Project Models
using beltexam.Models;
// LINQ library
using System.Linq;

namespace beltexam.Controllers
{
    public class HomeController : Controller
    {
        // Attach the Database Context to the class
        private AuctionContext _context;
        public HomeController(AuctionContext context)
        {
            // This attaches the Quote Database Context to the controller
            _context = context;
        }
        // GET: /Home/
        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            User user = fetchuser();
            if (user != null)
            {
                // Navbar variables
                ViewBag.email = user.Email;
            }
            return RedirectToAction("Index", "User");
        }
        // Fetch the user object
        public User fetchuser()
        {
            // Get the user id from the Session if it exists
            int? user_id = HttpContext.Session.GetInt32("user");
            // Return the user or null
            if (user_id != null)
            {
                return _context.Users.Where(item => item.Id == user_id).SingleOrDefault();
            }
            else
            {
                return null;
            }
        }
    }
}
