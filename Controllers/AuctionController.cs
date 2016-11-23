using System;
// ASP.NET CORE Libraries
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
// Project Models
using beltexam.Models;
// LINQ library
using System.Linq;

namespace beltexam.Controllers
{
    [Route("auction")]
    public class AuctionController : Controller
    {
        // Attach the Database Context to the class
        private AuctionContext _context;
        public AuctionController(AuctionContext context)
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
        // GET: /add/
        [HttpGet]
        [Route("add")]
        public IActionResult Add()
        {
            // Check to make sure user is logged in
            var result = user_check(model:new Auction());
            User user = fetchuser();
            if (user != null)
            {
                // Navbar variables
                ViewBag.email = user.Email;
            }
            // Render the View with the User model
            return result;
        }
        // POST: /add/
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("add")]
        public IActionResult Add(Auction auction)
        {
            // Run the login dynamic method
            return add_auction(auction);
            //return View();
        }
        // GET: {id}/bid/add
        [HttpGet]
        [Route("{auctionid}/bid/add")]
        public IActionResult Bid(int auctionid)
        {
            // Populate the new Bid
            Bid bid = new Bid();
            bid.User = fetchuser();
            // Use the Context method to properly populate auction
            bid.Auction = _context.PopulateAuctionSingle(auctionid);
            // Make sure the auction id is set
            bid.AuctionId = auctionid;
            // Check to make sure user is logged in
            var result = user_check(model:bid);
            // ViewBag details
            ViewBag.topbid = bid.Auction.GetTopBid();
            // Render the view with the User Model
            return result;
        }
        // POST: {id}/bid/add
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{auctionid}/bid/add")]
        public IActionResult Bid(int auctionid, Bid bid)
        {
            // Run the register dynamic method
            return add_bid(bid,auctionid);
        }
        // GET: /delete/{id}
        [HttpGet]
        [Route("delete/{id}")]
        public IActionResult Delete(int id)
        {
            // Run the delete dynamic method
            return delete_auction(id);
        }
        /*  This is the user login check method
            Takes in a view name and a model for prepping the view returned */
        public dynamic user_check(string view = null,object model = null)
        {
            // Grab the user if it exists
            var user = fetchuser();
            // Check to make sure the user exists
            if (user == null)
            {
                // Pull the controller and action out of the HTTP Context
                var currentcontroller = RoutingHttpContextExtensions.GetRouteData(this.HttpContext).Values["controller"];
                var currentaction = RoutingHttpContextExtensions.GetRouteData(this.HttpContext).Values["action"];
                var currentid = RoutingHttpContextExtensions.GetRouteData(this.HttpContext).Values["id"];
                Console.WriteLine(currentid);
                // Redirect to the login page with the returnURL passed as parameters
                return RedirectToAction("Login", "User", new { ReturnNamedRoute = currentaction, ReturnController = currentcontroller, ReturnID = currentid });
            }
            else
            {
                // Return view with specific view name if passed in
                if (view == null)
                {
                    return View(model);
                }
                else
                {
                    return View(view,model);
                }  
            }
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
            // Initialize the model
            var models = _context.PopulateAuctionAllOrderByRemaining();
            // Check to make sure user is logged in
            var result = user_check(model:models);
            User user = fetchuser();
            if (user != null)
            {
                // Navbar ViewBag
                ViewBag.user_name = user.name();
                ViewBag.user_wallet = user.Money;
                ViewBag.userid = user.Id;
            }
            ViewBag.dashboard = true;
            // Handle the expired auctions before we View them (Candidate for async)
            _context.ProcessExpiredAuctions();
            // Return the view of index with list of auctions attached
            return result;
        }
        // This handles the add auction logic
        private dynamic add_auction(Auction auction)
        {
            //Check model validations
            if (ModelState.IsValid)
            {
                // All validations are good. Let's get user details
                User user = fetchuser();
                auction.UserId = user.Id;
                //Save the auction to the table
                _context.Auctions.Add(auction);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            // Return the view of Add Auction
            return View(auction);
        }
        // This handles the add bid logic
        private dynamic add_bid(Bid bid, int auctionid)
        {
            // Ensure the ID is reset to 0 since MVC takes the ID as the Bid ID
            bid.Id = 0;
            // Let's get user details
            User user = fetchuser();
            // Get the auction
            Auction auction = _context.PopulateAuctionSingle(auctionid);
            // Additional manual validations
            bool ManualValidationPassed = true;
            Bid topbid = auction.GetTopBid();
            // When there is no topbid
            if (topbid == null)
            {
                if (bid.UserBid > auction.StartPrice)
                {
                    topbid = bid;
                }
                else
                {
                    ModelState.AddModelError("UserBid", "Your bid must be higher then the start price");
                    ManualValidationPassed = false;
                }
            }
            // When there is a valid topbid
            if (topbid != null)
            {
                // Make sure the new bid is highest than the existing top bid
                if ((topbid.UserBid - bid.UserBid) > 0)
                {
                    ModelState.AddModelError("UserBid", "Your bid must be higher then the highest bid");
                    ManualValidationPassed = false;
                }
                // Let's make sure the user has the funds to bid this
                float cost = user.Money - topbid.UserBid;
                if (cost < 0)
                {
                    ModelState.AddModelError("UserBid", "You have insufficient funds to bid this amount");
                    ManualValidationPassed = false;
                }
            }
            //Check model validations
            if (ModelState.IsValid && ManualValidationPassed)
            {
                // Bind the Foreign ID's
                bid.UserId = user.Id;
                // Add the Bid to the table
                _context.Bids.Add(bid);
                // Save the auction to the table
                _context.SaveChanges();
                // Redirect to Index
                return RedirectToAction("Index");
            }
            // ViewBag details
            ViewBag.topbid = topbid;
            bid.Auction = auction;
            bid.User = user;
            // Return the view of Login
            return View(bid);
        }
        // This handles the add auction logic
        private dynamic delete_auction(int id)
        {
            // Get the auction from the table
            Auction auction = _context.Auctions.Where(x => x.Id == id).SingleOrDefault();
            // Remove the auction
            _context.Remove(auction);
            _context.SaveChanges();
            // Redireect to Index
            return RedirectToAction("Index");
        }
    }
}
