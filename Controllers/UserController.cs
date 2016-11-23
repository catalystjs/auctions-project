// ASP.NET CORE Libraries
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
// Project Models
using beltexam.ViewModels;
using beltexam.Models;
// LINQ library
using System.Linq;


namespace beltexam.Controllers
{
    [Route("user")]
    public class UserController : Controller
    {
        // Attach the Database Context to the class
        private AuctionContext _context;
        public UserController(AuctionContext context)
        {
            // This attaches the Quote Database Context to the controller
            _context = context;
        }
        // GET: /Home/
        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            // Run the index dynamic method
            return index();
        }
        // GET: /login/
        [HttpGet]
        [Route("login")]
        public IActionResult Login(string ReturnNamedRoute=null, string ReturnController=null, int? ReturnID=null)
        {
            Login user = new Login();
            // Pull the controller and action out of the HTTP Context
            var currentaction = RoutingHttpContextExtensions.GetRouteData(this.HttpContext).Values["action"];
            string caller = (string)currentaction;
            // Set the Session variables for caller method
            HttpContext.Session.SetString("login_route",caller);
            if (ReturnNamedRoute != null && ReturnController != null)
            {
                // Set the View data information for Redirect
                HttpContext.Session.SetString("source_action", ReturnNamedRoute);
                HttpContext.Session.SetString("source_controller", ReturnController);
            }
            if (ReturnID != null)
            {
                // Set the View data information for the ID
                HttpContext.Session.SetInt32("source_id", (int)ReturnID);
            }
            User dbuser = fetchuser();
            if (dbuser != null)
            {
                // Navbar variables
                ViewBag.email = dbuser.Email;
            }
            // Variable for the caller in the method
            ViewBag.caller = caller;
            // Render the View with the User model
            return View(user);
        }
        // POST: /login/
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("login")]
        public IActionResult Login(Login user)
        {
            // Run the login dynamic method
            return login(user);
        }

        // GET: /register/
        [HttpGet]
        [Route("register")]
        public IActionResult Register()
        {
            User user = new User();
            // Set the View data information for Redirect
            HttpContext.Session.SetString("register_route", "Register");
            // Render the view with the User Model
            return View(user);
        }
        // POST: /login/
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("register")]
        public IActionResult Register(User user)
        {
            // Run the register dynamic method
            return register(user);
        }
        // GET: /logout/
        [HttpGet]
        [Route("logout")]
        public IActionResult Logout()
        {
            // Reset the Sessions by clearing them all
            HttpContext.Session.Clear();
            // Redirect to the Index page
            return RedirectToAction("Index");
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
        // This handles the index logic
        private dynamic index()
        {
            User user = fetchuser();
            if (user != null)
            {
                // Navbar variables
                ViewBag.email = user.Email;
            }
            // Set the View data information for Redirect
            HttpContext.Session.SetString("login_route", "Index");
            // Variable for the caller in the method
            ViewBag.caller = null;
            // Create the wrapper for the Index page
            Wrapper wrapper = new Wrapper(new User(), new Login());
            // Return the view of index with list attached
            return View(wrapper);
        }
        // This handles the login logic
        private dynamic login(Login user)
        {
            // Get the Session variables for caller method
            string caller = HttpContext.Session.GetString("login_route");
            //Check model validations
            if (ModelState.IsValid)
            {
                // All validations are good. Let's check if user exists
                User dbuser = _context.Users.Where(item => item.Email == user.Email_field).SingleOrDefault();
                // If the user exists we do our checks
                if (dbuser != null)
                {
                    // User exists. Let's check password
                    if (dbuser.check_password(user.Password_field))
                    {
                        // Set the session variables
                        HttpContext.Session.SetString("authenticated","Yes");
                        HttpContext.Session.SetInt32("user",dbuser.Id);
                        // Get the Session variables for Return URL if they exist
                        string route = HttpContext.Session.GetString("source_action");
                        string controller = HttpContext.Session.GetString("source_controller");
                        int? id = HttpContext.Session.GetInt32("source_id");
                        // Remove the existing Session keys for the Return URL
                        HttpContext.Session.Remove("source_controller");
                        HttpContext.Session.Remove("source_action");
                        // Set the ID if it is present
                        if (id != null)
                        {
                            HttpContext.Session.Remove("source_id");
                        }
                        // Set the default action and controller for redirect
                        if (route == null && controller == null)
                        {
                            route = "Index";
                            controller = "Auction";
                        }
                        //Password is good. Redirect to route and controller specified
                        return RedirectToAction(route, controller, new {id = id});
                    }

                }
                else
                {
                    ModelState.AddModelError("Email_field", "This email does not exist");
                }
            }
            // This was the short hand login View
            if (caller == "Login")
            {
                return View(user);
            }
            // This is the Index page if no other caller was set
            Wrapper wrapper = new Wrapper(new User(), user);
            return View("Index",wrapper);
        }
        // This handles the register logic
        private dynamic register(User user)
        {
            // Check the form validations
            if (ModelState.IsValid)
            {
                // Let's hash the password
                user.hash_password();
                // Add the user to the database and save changes
                _context.Users.Add(user);
                _context.SaveChanges();
                // Get the dbuser to grab all the details, including ID
                User dbuser = _context.Users.Where(item => item.Email == user.Email).SingleOrDefault();
                // Store information in Session
                HttpContext.Session.SetString("registered","Yes");
                HttpContext.Session.SetInt32("user",dbuser.Id);
                // Redirect to the main users page
                return RedirectToAction("Index", "Auction");
            }
            Wrapper wrapper = new Wrapper(user, new Login());
            // Return the view of Register
            return View("Index",wrapper);
        }
    }
}
