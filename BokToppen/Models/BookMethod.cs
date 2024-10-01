using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BokToppen.Models.ViewModels;
using Microsoft.Data.SqlClient;

namespace BokToppen.Models
{
    public class BookMethod
    {
        private readonly string _connectionString;
        
        public BookMethod() {
            _connectionString = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ConnectionStrings")["DefaultConnection"];
        }
        
        UserMethod um = new UserMethod();
       
        private SqlConnection NewConnection()
        {
            //Skapa SQL-connection
            SqlConnection dbConnection = new SqlConnection();

            // Koppling mot server
            dbConnection.ConnectionString = _connectionString;
            return dbConnection;
        }
        
        public List<BookModel> GetBooks(string searchParam, string filter, bool sortByPublishedDate, out string errormsg)
        {
            SqlConnection dbConnection = NewConnection();

            string query = "SELECT * FROM Tbl_Books";

             if (searchParam != null || (filter != null && filter != "0")) query += " WHERE";

            if (searchParam != null) query += " Bo_Title LIKE @searchParams";
            if (filter != null && filter != "0")
            {
                string andOperator = searchParam != null ? " AND" : "";
                query +=  andOperator + " Bo_CategoryId = @filterParams";
            } 
            if (sortByPublishedDate)
            {
                string andOperator = (searchParam != null || (filter != null && filter != "0")) ? " AND" : "";
                query += " ORDER BY Bo_PublicationYear";
            }

            SqlCommand dbCommand = new SqlCommand(query, dbConnection);


            if (searchParam != null) dbCommand.Parameters.Add("searchParams", SqlDbType.NVarChar, 50).Value = "%" + searchParam + "%";
            if (filter != null && filter != "0") dbCommand.Parameters.Add("filterParams", SqlDbType.NVarChar, 50).Value = filter;


            SqlDataReader reader = null;
            var bookList = new List<BookModel>();

            errormsg = "";

            try
            {
                dbConnection.Open();
                reader = dbCommand.ExecuteReader();

                while(reader.Read())
                {
                    BookModel book = new BookModel()
                    {
                        Id = Convert.ToInt32(reader["Bo_Id"]),
                        Title = reader["Bo_Title"].ToString(),
                        ISBN = reader["Bo_ISBN"].ToString(),
                        CategoryId = Convert.ToInt32(reader["Bo_CategoryId"]),
                        Description = reader["Bo_Description"].ToString(),
                        PublicationYear = Convert.ToInt32(reader["Bo_PublicationYear"]),
                        UserId = reader["Bo_UserId"] != DBNull.Value ? Convert.ToInt32(reader["Bo_UserId"]) : null,
                    };

                    bookList.Add(book);
                }
                if (bookList.Count() == 0)
                {
                     errormsg = "Hittade inga bokinlägg";
                    return bookList;
                }
                reader.Close();
                return bookList;
            }
            catch (Exception e)
            {
                errormsg = e.Message;
                return null;
            }
            finally
            {
                dbConnection.Close();
            }
        }

        public BookWithAuthorsVM GetBookById(int bookId, out string errormsg)
        {
            SqlConnection dbConnection = NewConnection();

            string query = "SELECT Tbl_Books.*, Au_Name AS Bo_Authors, Ca_Category AS Bo_CategoryName FROM Tbl_Books INNER JOIN Tbl_Books_Authors ON Bo_Id = Tbl_Books_Authors.BA_BookID INNER JOIN Tbl_Authors ON Tbl_Books_Authors.BA_AuthorID = Au_Id INNER JOIN Tbl_Categories ON Tbl_Categories.Ca_Id = Bo_CategoryId WHERE Bo_Id = @bookid;";
            SqlCommand dbCommand = new SqlCommand(query, dbConnection);

            dbCommand.Parameters.Add("bookid", SqlDbType.Int).Value = bookId;

            SqlDataReader reader = null;
            var bookItem = new BookWithAuthorsVM
            {
                Book = new BookModel(),
                Authors = new List<string>()
            };

            errormsg = "";

            try
            {
                dbConnection.Open();

                reader = dbCommand.ExecuteReader();

                while (reader.Read())
                {
                    if (bookItem.Book.Title == null)
                    {
                        bookItem.Book.Id = Convert.ToInt32(reader["Bo_Id"]);
                        bookItem.Book.Title = reader["Bo_Title"]?.ToString();
                        bookItem.Book.ISBN = reader["Bo_ISBN"]?.ToString();
                        bookItem.Book.CategoryId = Convert.ToInt32(reader["Bo_CategoryId"]);
                        bookItem.Book.Description = reader["Bo_Description"]?.ToString();
                        bookItem.Book.PublicationYear = Convert.ToInt32(reader["Bo_PublicationYear"]);
                        bookItem.Book.PublishedDate = Convert.ToDateTime(reader["Bo_CreatedAt"]);
                        bookItem.ImageData = (byte[])reader["Bo_ImageData"];
                        bookItem.ContentType = reader["Bo_ImageContentType"]?.ToString();
                        bookItem.Book.UserId = reader["Bo_UserId"] != DBNull.Value ? Convert.ToInt32(reader["Bo_UserId"]) : null;
                        bookItem.CategoryName = reader["Bo_CategoryName"]?.ToString();
                    }

                    // If there exists many book, they are added to the list in this while loop,
                    // the other content is just added ones
                    bookItem.Authors.Add(reader["Bo_Authors"].ToString());
                };

                if (bookItem.Book.Title == null)
                {
                    errormsg = "Gick inte att hämta bokinlägget";
                    return null;
                }
                reader.Close();
                return bookItem;
            }
            catch (Exception e)
            {
                errormsg = e.Message;
                return null;
            }
            finally
            {
                dbConnection.Close();
            }
        }

        public int InsertBook(BookModel book, MemoryStream memoryStream, string authors, out string errormsg)
        {

            SqlConnection dbConnection = NewConnection();

            string query = "EXEC addBook @title, @isbn, @categoryId, @description, @publicationYear, @imageData, @imageContentType, @userId, @authors";
            SqlCommand dbCommand = new SqlCommand(query, dbConnection);

            dbCommand.Parameters.Add("title", SqlDbType.NVarChar, 50).Value = book.Title;
            dbCommand.Parameters.Add("isbn", SqlDbType.Char, 13).Value = book.ISBN;
            dbCommand.Parameters.Add("categoryId", SqlDbType.NVarChar, 20).Value = book.CategoryId;
            dbCommand.Parameters.Add("description", SqlDbType.NVarChar, 1000).Value = book.Description;
            dbCommand.Parameters.Add("publicationYear", SqlDbType.Int).Value = book.PublicationYear;
            dbCommand.Parameters.Add("imageData", SqlDbType.VarBinary).Value = memoryStream.ToArray();
            dbCommand.Parameters.Add("imageContentType", SqlDbType.NVarChar, 50).Value = book.Image.ContentType;
            dbCommand.Parameters.Add("userId", SqlDbType.Int).Value = book.UserId;
            dbCommand.Parameters.Add("authors", SqlDbType.NVarChar, 50).Value = authors;

            try
            {
                dbConnection.Open();
                int affectedRows = 0;

                // ExecuteNonQuery returns the number of rows affected
                affectedRows = dbCommand.ExecuteNonQuery();

                if (affectedRows > 0)
                {
                    errormsg = "";
                }
                else
                {
                    errormsg = "Skapades inte en bok";
                }
                return (affectedRows);
            }
            catch (Exception e)
            {
                errormsg = e.Message;
                return 0;
            }
            finally
            {
                dbConnection.Close();
            }
        }

        public int UpdateBook(BookModel book, MemoryStream memoryStream, out string errormsg)
        {
            SqlConnection dbConnection = NewConnection();

            string query;
            if (string.IsNullOrEmpty(book.Image.ContentType))
            {
                query = "UPDATE Tbl_Books SET Bo_Title = @title, Bo_ISBN = @isbn, Bo_CategoryId = @categoryId, Bo_PublicationYear = @publicationYear, Bo_Description = @description WHERE Bo_Id = @bookId";
            }
            else
            {
                query = "UPDATE Tbl_Books SET Bo_Title = @title, Bo_ISBN = @isbn, Bo_CategoryId = @categoryId, Bo_PublicationYear = @publicationYear, Bo_ImageData = @imageData, Bo_ImageContentType = @imageContentType, Bo_Description = @description WHERE Bo_Id = @bookId";
            }
            SqlCommand dbCommand = new SqlCommand(query, dbConnection);

            dbCommand.Parameters.Add("bookId", SqlDbType.Int).Value = book.Id;
            dbCommand.Parameters.Add("title", SqlDbType.NVarChar, 50).Value = book.Title;
            dbCommand.Parameters.Add("isbn", SqlDbType.Char, 13).Value = book.ISBN;
            dbCommand.Parameters.Add("categoryId", SqlDbType.NVarChar, 20).Value = book.CategoryId;
            dbCommand.Parameters.Add("publicationYear", SqlDbType.NVarChar, 20).Value = book.PublicationYear;
            dbCommand.Parameters.Add("description", SqlDbType.NVarChar, 1000).Value = book.Description;

            if (!string.IsNullOrEmpty(book.Image.ContentType))
            {
                dbCommand.Parameters.Add("imageData", SqlDbType.VarBinary).Value = memoryStream.ToArray();
                dbCommand.Parameters.Add("imageContentType", SqlDbType.NVarChar, 50).Value = book.Image.ContentType;
            }

            try
            {
                dbConnection.Open();
                int affectedRows = 0;

                // ExecuteNonQuery returns the number of rows affected
                affectedRows = dbCommand.ExecuteNonQuery();

                if (affectedRows == 1)
                {
                    errormsg = "";
                }
                else
                {
                    errormsg = "Gick inte att uppdatera boken";
                }
                return affectedRows;
            }
            catch (Exception e)
            {
                errormsg = e.Message;
                return 0;
            }
            finally
            {
                dbConnection.Close();
            }
        }

        public int DeleteBook(int Id, out string errormsg)
        {

            SqlConnection dbConnection = NewConnection();

            string query = "DELETE FROM Tbl_Books WHERE Bo_Id = @bookId";
            SqlCommand dbCommand = new SqlCommand(query, dbConnection);

            dbCommand.Parameters.Add("bookId", SqlDbType.Int).Value = Id;

            try
            {
                dbConnection.Open();
                int affectedRows = 0;

                // ExecuteNonQuery returns the number of rows affected
                affectedRows = dbCommand.ExecuteNonQuery();

                if (affectedRows == 1)
                {
                    errormsg = "";
                }
                else
                {
                    errormsg = "Gick inte att ta bort boken";
                }
                return affectedRows;
            }
            catch (Exception e)
            {
                errormsg = e.Message;
                return 0;
            }
            finally
            {
                dbConnection.Close();
            }
        }
    }
}