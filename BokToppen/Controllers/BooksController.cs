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
        private BookMethod _bookMethod;
        private ReviewMethod _reviewMethod;
        private UserMethod _userMethod;
        private CategoryMethod _categoryMethod;

        public BooksController()
        {
            _bookMethod = new BookMethod();
            _reviewMethod = new ReviewMethod();
            _userMethod = new UserMethod();
            _categoryMethod = new CategoryMethod();
        }

        public IActionResult Index(string q, string filter, bool sortByPublishedDate)
        {
            var books = new List<BookModel>();
            string error = "";
            
            books = _bookMethod.GetBooks(q, filter, sortByPublishedDate, out error);

            ViewBag.UserIsLoggedIn = HttpContext.Session.GetString("UserId") == null;
            ViewBag.error = error;

            List<CategoryModel> categoryList = _categoryMethod.GetCategories(out string categoryError);

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
            BookWithAuthorsVM bookItem = _bookMethod.GetBookById(id, out string bookError);

            string reviewError = "";
            string userError = "";

            if (bookItem != null)
            {
                var book = _bookMethod.GetBookById(id, out bookError);
                var username =  _userMethod.GetUserName(book.Book.UserId, out userError);

                var BookWithReviews = new BookReviewsVM
                {
                    BookPost = book,
                    Username = (username != null) ? username : "'Borttagen användare'",
                    Reviews = _reviewMethod.GetReviewsByBook(id, out reviewError)
                };


                ViewBag.error = "Book: " + bookError + ", Review: " + reviewError + ", User: " + userError;
                ViewBag.UserIsLoggedIn = HttpContext.Session.GetString("UserId") == null;
                return View(BookWithReviews);
            }
            else 
            {
                TempData["unsuccessful"] = "Bågot gick fel med att hitta bokinlägget. " + bookError;
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

            List<CategoryModel> categoryList = _categoryMethod.GetCategories(out string error);

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
                book.UserId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

                int affectedRows = 0;
                string bookError = "";

                if (book.Image == null || book.Image.Length == 0) ModelState.AddModelError(nameof(book.Image), "Du måste välja en bild");
                if (string.IsNullOrEmpty(authors)) ModelState.AddModelError("authors", "Fältet kan inte vara tomt");
                if (book.PublicationYear < 1000 || book.PublicationYear > DateTime.Now.Year)  ModelState.AddModelError(nameof(book.PublicationYear), "Fältet måste vara ett år");

                if (ModelState.IsValid)
                {  
                    var fileMethod = new FileMethod();

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        book.Image.CopyTo(memoryStream);
                        // Upload the file if less than 2 MB
                        if (memoryStream.Length < 2097152)
                        { 
                            affectedRows = _bookMethod.InsertBook(book, memoryStream, authors, out bookError);

                            if (bookError != "" && bookError != null)
                            {
                                TempData["unsuccessful"] = "Något blev fel. Error: " + bookError;
                            }

                            if (affectedRows != 0)
                            {
                                return RedirectToAction("Index");  
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(nameof(book.Image), "Filen är för stor");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Tempdata to show unseccess
                TempData["unsuccessful"] = "Något blev fel. " + e.Message;
            }
            
            List<CategoryModel> categoryList = _categoryMethod.GetCategories(out string categoryError);

            if (!string.IsNullOrEmpty(categoryError))
            {
                TempData["unsuccessful"] = "Gick inte att hitta kategorier. " + categoryError;
                return RedirectToAction("Index");
            }
            
            ViewData["category"] = categoryList;
            ViewBag.authors = authors;
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
            BookWithAuthorsVM book = _bookMethod.GetBookById(id, out string bookError);

            if (book == null)
            {
                TempData["unsuccessful"] = "Bokinlägget som du ville ändra kunde inte hittas";
                return RedirectToAction("Index");
            }

            List<CategoryModel> categoryList = _categoryMethod.GetCategories(out string error);

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
                List<CategoryModel> categoryList = _categoryMethod.GetCategories(out string categoryError);

                if (categoryError == null)
                {
                    TempData["unsuccessful"] = "Gick inte att hitta kategorier. " + categoryError;
                    return RedirectToAction("Detials", "Books", new { id = book.Id});
                }
                
                ViewData["category"] = categoryList;

                var bookItem = new BookWithAuthorsVM()
                {
                    Book = book
                };
                return View(bookItem);
            }
                try
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        book.Image.CopyTo(memoryStream);
                        // Upload the file if less than 2 MB
                        if (memoryStream.Length < 2097152)
                        { 
                            int affectedRows = _bookMethod.UpdateBook(book, memoryStream, out string error);

                            if (error != "" && error != null)
                            {
                                TempData["unsuccessful"] = "Något blev fel. Error: " + error;
                            }

                            if (affectedRows <= 0)
                            {
                                    TempData["unsuccessful"] = "Bokinlägget kunde inte uppdateras";
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(nameof(book.Image), "Filen är för stor");
                        }
                    }
                }
                catch (Exception e)
                {
                    //Tempdata to show unseccess
                    TempData["unsuccessful"] = "Något blev fel. " + e.Message;
                }

            return RedirectToAction("Details", "Books", new {id = book.Id});
        }

        [HttpPost]
        public ActionResult Delete(int id){

            // Kollar om boken med idt finns
            BookWithAuthorsVM book = _bookMethod.GetBookById(id, out string bookError);

            if (book != null)
            {
                //Ta bort bok från listan
                int rowsAffected = _bookMethod.DeleteBook(id, out string error);
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