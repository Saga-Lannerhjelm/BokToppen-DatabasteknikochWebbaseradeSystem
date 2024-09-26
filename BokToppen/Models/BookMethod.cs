using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace BokToppen.Models
{
    public class BookMethod
    {
        public BookMethod() {}

        UserMethod um = new UserMethod();

        private static SqlConnection ConnectToServer()
        {
            //Skapa SQL-connection
            SqlConnection dbConnection = new SqlConnection();

            //Koppling mot server
            // dbConnection.ConnectionString = configuration.GetValue<string>("ConnectionStrings:DefaultConnection");
            dbConnection.ConnectionString = "Data Source=localhost, 1433;Database=Lab2DB;User Id=sa;Password=lab2_ReDo;Encrypt=True;TrustServerCertificate=True;";
            return dbConnection;
        }
        public List<BookModel> GetBooks(out string errormsg)
        {
            SqlConnection dbConnection = ConnectToServer();

            string query = "SELECT * FROM Tbl_Books";
            SqlCommand dbCommand = new SqlCommand(query, dbConnection);

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
                        Category = reader["Bo_Category"].ToString(),
                        Description = reader["Bo_Description"].ToString(),
                        PublicationYear = Convert.ToInt32(reader["Bo_PublicationYear"]),
                        User = Convert.ToInt32(reader["Bo_UserId"]),
                    };


                    bookList.Add(book);
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

        public BookModel GetBookById(int bookId, out string errormsg)
        {
            SqlConnection dbConnection = ConnectToServer();

            string query = "SELECT Tbl_Books.*, Au_Name AS Bo_Authors FROM Tbl_Books INNER JOIN Tbl_Books_Authors ON Bo_Id = Tbl_Books_Authors.BA_BookID INNER JOIN Tbl_Authors ON Tbl_Books_Authors.BA_AuthorID = Au_Id WHERE Bo_Id = @bookid;";
            SqlCommand dbCommand = new SqlCommand(query, dbConnection);

            dbCommand.Parameters.Add("bookid", SqlDbType.Int).Value = bookId;

            SqlDataReader reader = null;
            var book = new BookModel();

            errormsg = "";

            try
            {
                dbConnection.Open();

                reader = dbCommand.ExecuteReader();

                while (reader.Read())
                {
                    if (book.Title == null)
                    {
                        book.Id = Convert.ToInt32(reader["Bo_Id"]);
                        book.Title = reader["Bo_Title"].ToString();
                        book.ISBN = reader["Bo_ISBN"].ToString();
                        book.Category = reader["Bo_Category"].ToString();
                        book.Description = reader["Bo_Description"].ToString();
                        book.PublicationYear = Convert.ToInt32(reader["Bo_PublicationYear"]);
                        book.PublishedDate = Convert.ToDateTime(reader["Bo_CreatedAt"]);
                        book.User = Convert.ToInt32(reader["Bo_UserId"]);
                    }

                    // If there exists many book, they are added to the list in this while loop,
                    // the other content is just added ones
                    book.Authors.Add(reader["Bo_Authors"].ToString());
                };

                if (book.Title == null)
                {
                    errormsg = "Gick inte att hämta bokinlägget";
                    return null;
                }
                reader.Close();
                return book;
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

        public int InsertBook(BookModel book, out string errormsg)
        {

            SqlConnection dbConnection = ConnectToServer();

            string query = "INSERT INTO Tbl_Books (Bo_Title, Bo_ISBN, Bo_Category, Bo_Description, Bo_PublicationYear, Bo_UserId) VALUES (@title, @isbn, @category, @description, @publicationYear, @userId)";
            SqlCommand dbCommand = new SqlCommand(query, dbConnection);

            dbCommand.Parameters.Add("title", SqlDbType.NVarChar, 50).Value = book.Title;
            dbCommand.Parameters.Add("isbn", SqlDbType.Char, 13).Value = book.ISBN;
            dbCommand.Parameters.Add("category", SqlDbType.NVarChar, 20).Value = book.Category;
            dbCommand.Parameters.Add("description", SqlDbType.NVarChar, 1000).Value = book.Description;
            dbCommand.Parameters.Add("publicationYear", SqlDbType.Int).Value = book.PublicationYear;
            dbCommand.Parameters.Add("userId", SqlDbType.Int).Value = book.User;

            try
            {
                dbConnection.Open();
                int i = 0;

                // ExecuteNonQuery returns the number of rows affected
                i = dbCommand.ExecuteNonQuery();

                if (i == 1)
                {
                    errormsg = "";
                }
                else
                {
                    errormsg = "Skapades inte en bok";
                }
                return (i);
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