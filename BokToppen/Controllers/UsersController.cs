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

          public IActionResult Index()
        {

            return View();
        }

        public IActionResult GetUsers()
        {
            List<UserModel> userList = new List<UserModel>();
            UserMethod um = new UserMethod();
            string error = "";
            userList = um.GetUsers(out error);

            ViewBag.error = error;

            return View(userList);
        }

       
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}