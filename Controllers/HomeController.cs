using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WeddingPlanner.Context;
using WeddingPlanner.Models;

namespace WeddingPlanner.Controllers
{
    public class HomeController : Controller
    {
        private static MyContext _context;

        public HomeController(MyContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("register")]
        public IActionResult RegisterUser(User user)
        {
            if (ModelState.IsValid)
            {
                // Initializing a PasswordHasher object, providing our User class as its type
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                user.Password = Hasher.HashPassword(user, user.Password);
                user.CreatedAt = DateTime.Now;
                user.UpdatedAt = DateTime.Now;
                //Save your user object to the database
                _context.Users.Add(user);
                _context.SaveChanges();
                HttpContext.Session.SetInt32("UserID", user.UserID);
                return RedirectToAction("Success");
            } 
            else 
            {
                Console.WriteLine("----------------------Action Aborted----------------------");
                return View("Index");
            }
        }


        [HttpPost("auth")]
        public IActionResult Login(LoginUser user)
        {
            if (ModelState.IsValid)
            {
                User pulledUser = _context.Users.FirstOrDefault(p => p.Email.Contains(user.LoginEmail));
                if (pulledUser == null) 
                {
                    ModelState.AddModelError("LoginEmail", "Email/Password Invalid");
                    ModelState.AddModelError("LoginPassword", "Email/Password Invalid");
                    Console.WriteLine("----------------------Action Aborted----------------------");
                    return View("Index");
                }
                // Initialize hasher object
                var hasher = new PasswordHasher<LoginUser>();
                // verify provided password against hash stored in db
                var result = hasher.VerifyHashedPassword(user, pulledUser.Password, user.LoginPassword);
                // result can be compared to 0 for failure
                if(result == 0)
                {
                    // handle failure (this should be similar to how "existing email" is handled)
                    ModelState.AddModelError("LoginEmail", "Email/password Invalid");
                    ModelState.AddModelError("LoginPassword", "Email/Password Invalid");
                    Console.WriteLine("----------------------Action Aborted----------------------");
                    return View("Index");
                }
                else {
                    HttpContext.Session.SetInt32("UserID", pulledUser.UserID);
                    return RedirectToAction("Success");
                }
            }
            else
            {
                Console.WriteLine("----------------------Action Aborted----------------------");
                return View("Index");
            }
        }

        [HttpGet("success")]
        public IActionResult Success()
        {
            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                return RedirectToAction("NotAuthorized");
            }
            return Redirect("Dashboard");
        }

        [HttpGet("Dashboard")]
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                return RedirectToAction("NotAuthorized");
            }
            ViewBag.User = _context.Users
                .Include(u => u.WeddingsCreated)
                    .ThenInclude(wc => wc.Wedding)
                .FirstOrDefault(u => u.UserID == (int)HttpContext.Session.GetInt32("UserID"));

            List<Wedding> Weddings = _context.Weddings
                .Include(w => w.Guests)
                .Include(w => w.Owner)
                .ToList();

            foreach (Wedding wedding in Weddings)
            {
                if (wedding.Date <= DateTime.Now)
                {
                    _context.Remove(wedding);
                    _context.SaveChanges();
                }
            }

            ViewBag.WeddingsAttending = _context.Associations
                .Include(g => g.Wedding)
                .Include(g => g.User)
                .Where(g => g.User.UserID == (int)HttpContext.Session.GetInt32("UserID"))
                .ToList();
            return View(Weddings);
        }

        [HttpGet("Weddings/{WeddingID}")]
        public IActionResult Profile(int WeddingID)
        {
            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                return RedirectToAction("NotAuthorized");
            }
            ViewBag.Guests = _context.Associations
                .Include(an => an.User)
                .Include(an => an.Wedding)
                .Where(an => an.WeddingId == WeddingID)
                .ToList();

            Wedding Wedding = _context.Weddings
                .FirstOrDefault(w => w.WeddingID == WeddingID);

            // Set the address string for the API request
            string address = Wedding.Address;
            address.Replace(" ", "+");
            ViewBag.Address = address;

            return View(Wedding);
        }

        [HttpGet("Weddings/Plan")]
        public IActionResult PlanWedding()
        {
            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                return RedirectToAction("NotAuthorized");
            }
            ViewBag.User = _context.Users
                .Include(u => u.WeddingsCreated)
                    .ThenInclude(wc => wc.Wedding)
                .FirstOrDefault(u => u.UserID == (int)HttpContext.Session.GetInt32("UserID"));
            return View();
        }

        [HttpPost("Weddings/Create")]
        public IActionResult CreateWedding(Wedding wedding)
        {
            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                return RedirectToAction("NotAuthorized");
            }
            if (ModelState.IsValid)
            {
                wedding.Owner = _context.Users
                    .FirstOrDefault(u => u.UserID == (int)HttpContext.Session.GetInt32("UserID"));
                wedding.CreatedAt = DateTime.Now;
                wedding.UpdatedAt = DateTime.Now;
                _context.Weddings.Add(wedding);
                // Create a dummy Association object
                ManyToMany PulledUser = _context.Associations
                    .Include(an => an.Wedding)
                    .Include(an => an.User)
                    .Where(an => an.Wedding == wedding)
                    .FirstOrDefault(an => an.User == wedding.Owner);
                
                // Add the new Association to the list
                if (PulledUser == null)
                {   
                    ManyToMany AddUser = new ManyToMany() {Wedding = wedding, WeddingId = wedding.WeddingID, User = wedding.Owner, UserId = wedding.Owner.UserID};
                    _context.Add(AddUser);
                    _context.SaveChanges();
                }
                else
                {
                    Console.WriteLine("--------------------Already RSVP'd!--------------------");
                }
                // Redirect from a successful post request
                return RedirectToAction("Dashboard");
            }
            else
            {
                Console.WriteLine("----------------------Action Aborted----------------------");
                return View("PlanWedding", wedding);
            }
        }

        [HttpPost("Weddings/{WeddingID}/Delete")]
        public IActionResult DeleteWedding(Wedding wedding)
        {  
            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                return RedirectToAction("NotAuthorized");
            }
            _context.Weddings.Remove(wedding);
            _context.SaveChanges();
            return RedirectToAction("Dashboard");
        }
        public IActionResult NotAuthorized(LoginUser user)
        {
                return View("Index");
        }

        [HttpPost("Weddings/{WeddingID}/RSVP")]
        public IActionResult RSVP(int WeddingID)
        {
            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                return RedirectToAction("NotAuthorized");
            }
            // Get the Current Wedding
            Wedding wedding = _context.Weddings
                .Include(w => w.Guests)
                    .ThenInclude(g => g.User)
                .FirstOrDefault(w => w.WeddingID == WeddingID);
            // Get the current User.
            User CurrUser = _context.Users
                .FirstOrDefault(u => u.UserID == (int)HttpContext.Session.GetInt32("UserID"));

            // Create a dummy Association object
            ManyToMany PulledUser = _context.Associations
                .Include(an => an.Wedding)
                .Include(an => an.User)
                .Where(an => an.Wedding == wedding)
                .FirstOrDefault(an => an.User == CurrUser);
            
            // Add the new Association to the list
            if (PulledUser == null)
            {   
                ManyToMany AddUser = new ManyToMany() {Wedding = wedding, WeddingId = WeddingID, User = CurrUser, UserId = CurrUser.UserID};
                _context.Add(AddUser);
                _context.SaveChanges();
            }
            else
            {
                Console.WriteLine("--------------------Already RSVP'd!--------------------");
            }
            // Redirect from a successful post request
            return RedirectToAction("Dashboard");
        }

        [HttpPost("Weddings/{WeddingID}/UNRSVP")]
        public IActionResult UNRSVP(int WeddingID)
        {
            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                return RedirectToAction("NotAuthorized");
            }
            // Get the Current Wedding
            Wedding wedding = _context.Weddings
                .Include(w => w.Guests)
                    .ThenInclude(g => g.User)
                .FirstOrDefault(w => w.WeddingID == WeddingID);
            // Get the current User.
            User CurrUser = _context.Users
                .FirstOrDefault(u => u.UserID == (int)HttpContext.Session.GetInt32("UserID"));

            // Get the User Association Object
            ManyToMany PulledUser = _context.Associations
                .Include(an => an.Wedding)
                .Include(an => an.User)
                .Where(an => an.Wedding == wedding)
                .FirstOrDefault(an => an.User == CurrUser);

            // Add the new Association to the list
            if (PulledUser != null)
            {   
                _context.Remove(PulledUser);
                _context.SaveChanges();
            }
            else
            {
                Console.WriteLine("--------------------Already RSVP'd!--------------------");
            }
            // Redirect from a successful post request
            return RedirectToAction("Dashboard");
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return View("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
