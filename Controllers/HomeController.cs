using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ActivityCenter.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ActivityCenter.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext { get; set; }

        public HomeController(MyContext context)
        {
            dbContext = context;
        }

        public  User GetUser()
        {
            return dbContext.Users.FirstOrDefault(u => u.UserId == HttpContext.Session.GetInt32("UserId"));
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("process/registration")]
        public IActionResult ProcessRegistration(User newUser)
        {
            if (ModelState.IsValid)
            {
                if (dbContext.Users.Any(u => u.Email == newUser.Email))
                {
                    ModelState.AddModelError("Email", "Email already in use!");
                    return View("Index");
                }
                else
                {
                    PasswordHasher<User> Hasher = new PasswordHasher<User>();
                    newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
                    dbContext.Add(newUser);
                    dbContext.SaveChanges();
                    HttpContext.Session.SetInt32("UserId", newUser.UserId);
                    return Redirect("/dashboard");
                }
            }
            else
            {
                return View("Index");
            }
        }

        [HttpPost("process/login")]
        public IActionResult ProcessLogin(Login newLog)
        {
            if (ModelState.IsValid)
            {
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == newLog.LoginEmail);
                if (userInDb == null)
                {
                    ModelState.AddModelError("LoginEmail", "Invalid Email/Password");
                    return View("Index");
                }
                else
                {
                    var hasher = new PasswordHasher<Login>();
                    var result = hasher.VerifyHashedPassword(newLog, userInDb.Password, newLog.LoginPassword);
                    if (result == 0)
                    {
                        ModelState.AddModelError("LoginPassword", "Invalid Email/Password");
                        return View("Index");
                    }
                    else
                    {
                        HttpContext.Session.SetInt32("UserId", userInDb.UserId);
                        return RedirectToAction("Dashboard");
                    }
                }
            }
            else
            {
                return View("Index");
            }
        }

        [HttpGet("dashboard")]
        public IActionResult Dashboard(Login newLog)
        {
            User userInDb = GetUser();
            if (userInDb == null)
            {
                return Redirect("/");
            }
            ViewBag.User = userInDb;
            List<Gathering> AllGatherings = dbContext.Gatherings
                                                .Include(g => g.Planner)
                                                .Include(g => g.Guests)
                                                .ThenInclude(gl => gl.Participants)
                                                .Where(g => g.Date >= DateTime.Now)
                                                .OrderBy(g => g.Date)
                                                .ToList();
            return View(AllGatherings);
        }

        [HttpGet("gathering/new")]
        public IActionResult NewGathering()
        {
            User userInDb = GetUser();
            if (userInDb == null)
            {
                return Redirect ("/");
            }
            return View();
        }

        [HttpPost("gathering/create")]
        public IActionResult CreateGathering(Gathering newGathering)
        {
            User userInDb = GetUser();
            if (userInDb == null)
            {
                return Redirect ("/");
            }
            if(ModelState.IsValid)
            {
                newGathering.UserId = userInDb.UserId;
                dbContext.Gatherings.Add(newGathering);
                dbContext.SaveChanges();
                GuestList g = new GuestList();
                g.UserId = userInDb.UserId;
                g.GatheringId = newGathering.GatheringId;
                dbContext.GuestLists.Add(g);
                dbContext.SaveChanges();
                return Redirect($"/gathering/{newGathering.GatheringId}");
            }
            return View("NewGathering");
        }

        [HttpGet("gathering/{gatheringId}/delete")]
        public IActionResult DeleteGathering(int gatheringId)
        {
            User userInDb = GetUser();
            if (userInDb == null)
            {
                return Redirect ("/");
            }
            Gathering remove = dbContext.Gatherings.FirstOrDefault(w => w.GatheringId == gatheringId);
            dbContext.Gatherings.Remove(remove);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpGet("gathering/{gatheringId}/{status}")]
        public IActionResult ToggleStatus(int gatheringId, string status)
        {
            User userInDb = GetUser();
            if (userInDb == null)
            {
                return Redirect ("/");
            }
            if(status == "join")
            {
                GuestList newStatus = new GuestList();
                newStatus.UserId = userInDb.UserId;
                newStatus.GatheringId = gatheringId;
                dbContext.GuestLists.Add(newStatus);
            }
            else if(status == "leave")
            {
                GuestList backout = dbContext.GuestLists.FirstOrDefault(gl => gl.UserId == userInDb.UserId && gl.GatheringId == gatheringId);
                dbContext.GuestLists.Remove(backout);
            }
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpGet("gathering/{gatheringId}")]
        public IActionResult DisplayGathering(int gatheringId)
        {
            User userInDb = GetUser();
            if (userInDb == null)
            {
                return Redirect ("/");
            }
            ViewBag.User = userInDb;
            Gathering displaying = dbContext.Gatherings
                                    .Include(w => w.Guests)
                                    .ThenInclude(gl => gl.Participants)
                                    .Include(w => w.Planner)
                                    .FirstOrDefault(w => w.GatheringId == gatheringId );
            return View(displaying);
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return View("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
