
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BokToppen.Models;
using System.Security.Cryptography;
using System.Text;

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
                UserMethod unserMethod = new UserMethod();
                int existingUserId = unserMethod.GetUser(user, out string error);
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