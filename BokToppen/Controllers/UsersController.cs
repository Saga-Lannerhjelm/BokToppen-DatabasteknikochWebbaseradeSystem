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
        UserMethod um = new UserMethod();

          public IActionResult Index()
        {

            return View();
        }

        public IActionResult GetUsers()
        {
            List<UserModel> userList = new List<UserModel>();
            string error = "";
            userList = um.GetUsers(out error);

            ViewBag.error = error;
            ViewBag.UserIsLoggedIn = HttpContext.Session.GetString("UserId") == null;

            return View(userList);
        }

        [HttpPost]
        public IActionResult Delete(int id){

            // Kollar om boken med idt finns
            // int userId = us.GetUserId(ratingId, out string userError);

            // if (userId > 0)
            // {
                //Ta bort bok från listan
                int rowsAffected = um.DeleteUser(id, out string error);
                if (rowsAffected <= 0)
                {
                    TempData["unsuccessful"] = "Det gick inte att ta bort omdömet. " + error;
                }

            // } 
            // else 
            // {
            //     TempData["unsuccessful"] = " Det gick inte att ta bort användaren :(. " + userError;
            // }

            return RedirectToAction("Index", "Books");
        }

       
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}