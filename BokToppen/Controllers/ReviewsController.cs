using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BokToppen.Models;
using BokToppen.Models.ViewModels;
using System.Data;

namespace BokToppen.Controllers
{
    public class ReviewsController : Controller
    {
        List<int> ratingNumbers = new List<int>{1, 2, 3, 4, 5};

        [HttpGet]
        public IActionResult Create(int id)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                TempData["notLoggedIn"] = "Du måste vara inloggad för att skapa ändra eller ta bort inlägg och kommentarer";
                return RedirectToAction("Index", "Login");
            }

            var bm = new BookMethod();
            var book = bm.GetBookById(id, out string error);

            if (book != null)
            {
                ViewBag.BookTitle = book.Title;
                ViewBag.BookId = book.Id;

                ViewData["ratings"] = ratingNumbers;
                return View();
            }

            TempData["unsuccessful"] = "Bokinlägget du försökte skapa ett omdömme på finns inte. " + error;
            return RedirectToAction("Index", "Books");
        }

        [HttpPost]
        public ActionResult Create(ReviewModel review, int bookId, string bookTitle){

            review.CreatorId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
            review.Points = Convert.ToInt32(review.Points);

            if (ModelState.IsValid)
            {
                ReviewMethod rm = new ReviewMethod();

                int antalRowsAffected = rm.InsertReview(review, out string error);

                if (antalRowsAffected != 0)
                {
                    return RedirectToAction("Details", "Books", new {id = bookId});
                }
                
                ViewBag.error = error;
            }

                ViewBag.BookTitle = bookTitle;
                ViewBag.BookId = bookId;
                ViewData["ratings"] = ratingNumbers;
                return View();
        }

        [HttpPost]
        public IActionResult Delete(int ratingId, int bookId){

            // Hämta reviews listan och hitta det omdöme som har rätt id
            var reviews = HttpContext.Session.GetObject<List<ReviewModel>>("reviews");
            int reviewIndex = reviews.FindIndex(r => r.Id == ratingId);

            if (reviewIndex != -1)
            {
                // Ta bort omdöme från listan
                reviews.RemoveAt(reviewIndex);
                HttpContext.Session.SetObject("reviews", reviews);
            }
            else
            {
                TempData["unsuccessful"] = "Det gick inte att ta bort omdömet";
            }
            
            return RedirectToAction("Details", "Books", new {id = bookId});
        }

    }
}