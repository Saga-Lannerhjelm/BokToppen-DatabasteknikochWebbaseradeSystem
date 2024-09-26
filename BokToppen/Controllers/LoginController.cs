
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BokToppen.Models;

namespace BokToppen.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserId") != null)
            {
                return RedirectToAction("Index", "Books");
            }

            return View("Index");
        }

        [HttpPost]
        public IActionResult Login(UserModel user)
        {
            if (ModelState.IsValid)
            {
                UserMethod um = new UserMethod();
                int existingUserId = um.FindUser(user, out string error);
                if(existingUserId != 0){
                    // Sätter session variabel som visar att användaren är "inloggad"
                    HttpContext.Session.SetString("UserId", existingUserId.ToString());
                    return RedirectToAction("Index", "Books");
                }
                else
                {
                    // Lägger till ett fel meddelande om till exempel användarnamnet eller lösenordet är fel 
                    ModelState.AddModelError(nameof(user.Password), error);
                }
            }
            return View("Index", user);
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateUser(UserModel user)
        {
            string error = "";
            if (ModelState.IsValid)
            {
                UserMethod um = new UserMethod();

                int insertedUserId = um.InsertUser(user, out error);
                if (insertedUserId > 0)
                {
                    HttpContext.Session.SetString("UserId", insertedUserId.ToString());
                    return RedirectToAction("Index", "Books");
                }
                else
                {
                    // Lägger till ett felmeddelande
                    ModelState.AddModelError(nameof(user.Password), error);
                }
            }
            return View();
        }

        [HttpPost]
        public IActionResult LougOut()
        {
            HttpContext.Session.Remove("UserId");
            return RedirectToAction("Index", "Login");
        }


        private readonly ILogger<LoginController> _logger;

        public LoginController(ILogger<LoginController> logger)
        {
            _logger = logger;
        }
    }
}