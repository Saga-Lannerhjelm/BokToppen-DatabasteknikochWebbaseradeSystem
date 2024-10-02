using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BokToppen.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BokToppen.Controllers
{
    public class UsersController : Controller
    {
        private UserMethod _userMethod;

        public UsersController()
        {
            _userMethod = new UserMethod();
        }

        public IActionResult Index()
        {
            List<UserModel> userList = _userMethod.GetUsers(out string error);

            ViewBag.UserIsLoggedIn = HttpContext.Session.GetString("UserId") == null;

            if (!string.IsNullOrEmpty(error))
            {
                TempData["unsuccessful"] = "Det gick inte att hämta användare. Error: " + error;
            }
            return View(userList);
        }

         [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(UserModel user)
        {
            // Kollar ifall användarnamnet är mindre än 8
            if (user.Password == null || user.Password.Length < 8)
            {
                ModelState.AddModelError(nameof(user.Password), "Lösenordet måste vara mint 8 tecken långt");
            }

            if (user.Email == "" || user.Email == null)
            {
                ModelState.AddModelError(nameof(user.Email), "Skriv in en EmailAdress");
            }

            if (ModelState.IsValid)
            {
                int insertedUserId = _userMethod.InsertUser(user, out string error);
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
        public IActionResult Delete(int id)
        {
            int rowsAffected = _userMethod.DeleteUser(id, out string error);

            if (rowsAffected <= 0)
            {
                TempData["unsuccessful"] = "Det gick inte att ta bort användaren. Error: " + error;
            }

            if (error != "")
            {
                 TempData["unsuccessful"] = "Det gick inte att ta bort användaren. Error: " + error;
            }


            return RedirectToAction("Index");
        }
    }
}