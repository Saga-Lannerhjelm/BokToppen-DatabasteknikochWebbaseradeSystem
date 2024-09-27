using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BokToppen.Models;
using System.Linq;
using Microsoft.VisualBasic;
using BokToppen.Models.ViewModels;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace BokToppen.Controllers
{
    public class BooksController : Controller
    {
        BookMethod bm = new BookMethod();
        ReviewMethod rm = new ReviewMethod();
        UserMethod um = new UserMethod();
        List<string> categoryList = new List<string>{ "Romantik", "Fantasi", "Science fiction", "Dystopisk", "Action & Äventyr", "Deckare", "Fakta", "Barnbok", "Ungdomsbok", "Roman", "Novell", "Biografi", "Poesi", };

        public IActionResult Index(string q)
        {
            var books = new List<BookModel>();
            var bm = new BookMethod();
            string error = "";
            if (q == null)
            {
                books = bm.GetBooks("", out error);
            }
            else
            {
                books = bm.GetBooks(q, out error);
            }


            ViewBag.UserIsLoggedIn = HttpContext.Session.GetString("UserId") == null;
            ViewBag.error = error;

            return View(books);
        }

        public IActionResult Details(int id)
        {
            // Kollar om boken med idt finns
            BookModel book = bm.GetBookById(id, out string bookError);

            string reviewError = "";
            string userError = "";

            if (book != null)
            {
                var BookReviewsViewModel = new BookReviewsViewModel
                {
                    Book = bm.GetBookById(id, out bookError),
                    Reviews = rm.GetReviewsByBook(id, out reviewError)
                };

                // Send username to view based on user id
                ViewBag.user = um.GetUserName(BookReviewsViewModel.Book.User, out userError);

                ViewBag.error = "Book: " + bookError + ", Review: " + reviewError + ", User: " + userError;
                ViewBag.UserIsLoggedIn = HttpContext.Session.GetString("UserId") == null;
                return View(BookReviewsViewModel);
            }
            else 
            {
                TempData["unsuccessful"] = "Bokinlägget du försökte nå finns inte. " + bookError;
                return RedirectToAction("index");
            }
                
           
        }

        [HttpGet]
        public IActionResult Create(){

            if (HttpContext.Session.GetString("UserId") == null)
            {
                TempData["notLoggedIn"] = "Du måste vara inloggad för att skapa ändra eller ta bort inlägg och kommentarer";
                return RedirectToAction("Index", "Login");
            }

            ViewData["category"] = categoryList;
            return View();
        }

        [HttpPost]
        public ActionResult Create(BookModel book){
            try
            {
                book.User = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

                int antal = 0;
                string error = "";
                if (ModelState.IsValid)
                {  

                    antal = bm.InsertBook(book, out error);

                    ViewBag.error = error;
                    ViewBag.antal = antal;

                    if (antal != 0)
                    {
                        return RedirectToAction("Index");  
                    }
                }
                // ViewBag.authors = authors;
            }
            catch (Exception e)
            {
                //Tempdata to show unseccess
                TempData["unsuccessful"] = "Något blev fel. " + e.Message;
            }
            
            ViewData["category"] = categoryList;
            return View("Create");
        }

        [HttpGet]
        public IActionResult Edit(int id){

            if (HttpContext.Session.GetString("UserId") == null)
            {
                TempData["notLoggedIn"] = "Du måste vara inloggad för att skapa ändra eller ta bort inlägg och kommentarer";
                return RedirectToAction("Index", "Login");
            }

            // Kollar om boken med idt finns
            BookModel book = bm.GetBookById(id, out string bookError);

            if (book == null)
            {
                TempData["unsuccessful"] = "Bokinlägget som du ville ändra kunde inte hittas";
                return RedirectToAction("Index");
            }

            ViewData["category"] = categoryList;
            return View(book);
        }

        [HttpPost]
        public ActionResult Edit(BookModel book){

            if (!ModelState.IsValid)
            {
                ViewData["category"] = categoryList;
                return View(book);
            }

            // Hämtar datan sparad i sessionen och hitta bokinlägget med ett visst id
            int affectedRows = bm.UpdateBook(book, out string error);
            
            if (affectedRows <= 0)
            {
                TempData["unsuccessful"] = "Bokinlägget kunde inte uppdateras";
            }

            return RedirectToAction("Details", "Books", new {id = book.Id});
        }

        [HttpPost]
        public ActionResult Delete(int id){

            // Kollar om boken med idt finns
            BookModel book = bm.GetBookById(id, out string bookError);

            if (book != null)
            {
                //Ta bort bok från listan
                int rowsAffected = bm.DeleteBook(id, out string error);
                if (rowsAffected <= 0)
                {
                    TempData["unsuccessful"] = "Det gick inte att ta bort inlägget. " + error;
                }

            } 
            else 
            {
                TempData["unsuccessful"] = " Det gick inte att ta bort inlägget :(. " + bookError;
            }

            return RedirectToAction("Index");
        }

        // [HttpGet]
        // public ActionResult SearchTitle(string q){

        //     // Kollar om boken med idt finns
        //     var 
        //     if (book != null)
        //     {
        //         //Ta bort bok från listan
        //         int rowsAffected = bm.DeleteBook(id, out string error);
        //         if (rowsAffected <= 0)
        //         {
        //             TempData["unsuccessful"] = "Det gick inte att ta bort inlägget. " + error;
        //         }

        //     } 
        //     else 
        //     {
        //         TempData["unsuccessful"] = " Det gick inte att ta bort inlägget :(. " + bookError;
        //     }

        //     return RedirectToAction("Index");
        // }



    }
}