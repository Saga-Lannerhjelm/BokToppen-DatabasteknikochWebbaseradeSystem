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
        private BookMethod _bookMethod;
        private ReviewMethod _reviewMethod;

        public ReviewsController()
        {
            _bookMethod = new BookMethod();
            _reviewMethod = new ReviewMethod();
        }
        List<int> ratingNumbers = new List<int>{1, 2, 3, 4, 5};

        [HttpGet]
        public IActionResult Create(int id)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                TempData["notLoggedIn"] = "Du måste vara inloggad för att skapa ändra eller ta bort inlägg och kommentarer";
                return RedirectToAction("Index", "Login");
            }

            var bookItem = _bookMethod.GetBookById(id, out string error);

            if (bookItem != null)
            {
                ViewBag.BookTitle = bookItem.Book.Title;
                ViewBag.BookId = bookItem.Book.Id;

                ViewData["ratings"] = ratingNumbers;
                return View();
            }

            TempData["unsuccessful"] = "Bokinlägget du försökte skapa ett omdömme på finns inte. Error: " + error;
            return RedirectToAction("Index", "Books");
        }

        [HttpPost]
        public ActionResult Create(ReviewModel review, int bookId, string bookTitle){

            review.UserId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
            review.Rating = Convert.ToInt32(review.Rating);

            if (ModelState.IsValid)
            {

                int antalRowsAffected = _reviewMethod.InsertReview(review, out string error);

                if (antalRowsAffected != 0)
                {
                    return RedirectToAction("Details", "Books", new {id = bookId});
                }
                
                TempData["unsuccessful"] = "Bokinlägget du försökte skapa ett omdömme på finns inte. Error: " + error;
            }

                ViewBag.BookTitle = bookTitle;
                ViewBag.BookId = bookId;
                ViewData["ratings"] = ratingNumbers;
                return View();
        }

        [HttpPost]
        public IActionResult Delete(int ratingId, int bookId){

            // Kollar om boken med idt finns
            int reviewId = _reviewMethod.GetReviewId(ratingId, out string reviewError);

            if (reviewId > 0)
            {
                //Ta bort bok från listan
                int rowsAffected = _reviewMethod.DeleteReview(ratingId, out string error);
                if (rowsAffected <= 0)
                {
                    TempData["unsuccessful"] = "Det gick inte att ta bort omdömet. " + error;
                }

            } 
            else 
            {
                TempData["unsuccessful"] = " Det gick inte att ta bort omdömet :(. " + reviewError;
            }

            return RedirectToAction("Details", "Books", new {id = bookId});
        }

    }
}