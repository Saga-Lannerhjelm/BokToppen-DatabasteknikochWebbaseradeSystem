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
        readonly BookMethod bm = new BookMethod();
        readonly ReviewMethod rm = new ReviewMethod();
        readonly UserMethod um = new UserMethod();
        readonly CategoryMethod cm = new CategoryMethod();

        public IActionResult Index(string q, string filter, bool sortByPublishedDate)
        {
            var books = new List<BookModel>();
            var bm = new BookMethod();
            string error = "";
            
            books = bm.GetBooks(q, filter, sortByPublishedDate, out error);

            ViewBag.UserIsLoggedIn = HttpContext.Session.GetString("UserId") == null;
            ViewBag.error = error;

            List<CategoryModel> categoryList = cm.GetCategories(out string categoryError);

            if (error == null)
            {
                TempData["unsuccessful"] = "Gick inte att hitta kategorier. " + categoryError;
            }
            
            ViewData["category"] = categoryList;
            ViewBag.query = q;
            ViewBag.filter = filter;
            ViewBag.sort = sortByPublishedDate;

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

                
                string username = um.GetUserName(BookReviewsViewModel.Book.User, out userError);

                // Send username to view based on user id
                ViewBag.user = (username != null) ? username : "'Borttagen användare'";

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

            List<CategoryModel> categoryList = cm.GetCategories(out string error);

            if (error == null)
            {
                TempData["unsuccessful"] = "Gick inte att hitta kategorier. " + error;
                return RedirectToAction("Index");
            }
            
            ViewData["category"] = categoryList;
            return View();
        }

        [HttpPost]
        public ActionResult Create(BookModel book, string authors){
            try
            {
                book.User = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

                int antal = 0;
                string bookError = "";

                if (authors == "" && authors != null) ModelState.AddModelError("authors", "Fältet kan inte vara tomt");
                if (book.PublicationYear < 1000 || book.PublicationYear > DateTime.Now.Year)  ModelState.AddModelError(nameof(book.PublicationYear), "Fältet måste vara ett år");

                if (ModelState.IsValid)
                {  

                    antal = bm.InsertBook(book, authors, out bookError);

                    if (bookError != "" && bookError != null)
                    {
                        TempData["unsuccessful"] = "Något blev fel. " + bookError;
                    }

                    if (antal != 0)
                    {
                        return RedirectToAction("Index");  
                    }
                }
            }
            catch (Exception e)
            {
                //Tempdata to show unseccess
                TempData["unsuccessful"] = "Något blev fel. " + e.Message;
            }

            ViewBag.authors = authors;
            
            List<CategoryModel> categoryList = cm.GetCategories(out string error);

            if (error == null)
            {
                TempData["unsuccessful"] = "Gick inte att hitta kategorier. " + error;
                return RedirectToAction("Index");
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

            List<CategoryModel> categoryList = cm.GetCategories(out string error);

            if (error == null)
            {
                TempData["unsuccessful"] = "Gick inte att hitta kategorier. " + error;
                return RedirectToAction("Detials", "Books", new {id});
            }
            
            ViewData["category"] = categoryList;
            return View(book);
        }

        [HttpPost]
        public ActionResult Edit(BookModel book)
        {
            if (book.PublicationYear < 1000 || book.PublicationYear > DateTime.Now.Year)  ModelState.AddModelError(nameof(book.PublicationYear), "Fältet måste vara ett år mellan år 1000 och " + DateTime.Now.Year);

            if (!ModelState.IsValid)
            {
                List<CategoryModel> categoryList = cm.GetCategories(out string categoryError);

                if (categoryError == null)
                {
                    TempData["unsuccessful"] = "Gick inte att hitta kategorier. " + categoryError;
                    return RedirectToAction("Detials", "Books", new { id = book.Id});
                }
                
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
    }
}