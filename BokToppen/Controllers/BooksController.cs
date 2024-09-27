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

        public IActionResult Index()
        {
            var books = new List<BookModel>();
            var bm = new BookMethod();
            books = bm.GetBooks(out string error);

            ViewBag.UserIsLoggedIn = HttpContext.Session.GetString("UserId") == null;
            ViewBag.error = error;

            return View(books);
        }

        public IActionResult Details(int id)
        {

            string bookError = "";
            string reviewError = "";
            string userError = "";
                
            var BookReviewsViewModel = new BookReviewsViewModel
            {
                Book = bm.GetBookById(id, out bookError),
                Reviews = rm.GetReviewsByBook(id, out reviewError)
            };

            // Send username to view based on user id
            ViewBag.user = BookReviewsViewModel.Reviews.Count() > 0 ? um.GetUserName(BookReviewsViewModel.Book.User, out userError) : "";

            ViewBag.error = "Book: " + bookError + ", Review: " + reviewError + ", User: " + userError;
            ViewBag.UserIsLoggedIn = HttpContext.Session.GetString("UserId") == null;
            return View(BookReviewsViewModel);
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


                // if (notUniqueBook != null)
                // {
                //     // Lägger till valideringserror
                //     ModelState.AddModelError(nameof(book.Title),
                //     "Denna bok finns redan uppladdad. Gå dit ifall du vill lägga till en recension av boken.");
                // }

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

            BookModel? book = GetBookById(id);

            if (book == null)
            {
                TempData["unsuccessful"] = "Bokinlägget kunde inte hittas";
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
            var books = HttpContext.Session.GetObject<List<BookModel>>("books");
            int bookIndex = books.FindIndex(x => x.Id == book.Id);
            
            if (bookIndex != -1)
            {
                book.User = books[bookIndex].User;
                books[bookIndex] = book;
            }

            HttpContext.Session.SetObject("books", books);

            return RedirectToAction("Index");
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

        private BookModel? GetBookById(int id)
        {
            List<BookModel> books = HttpContext.Session.GetObject<List<BookModel>>("books");
            BookModel? book = books.Where(b => b.Id == id).FirstOrDefault();
            return book;

           
        }

    }
}