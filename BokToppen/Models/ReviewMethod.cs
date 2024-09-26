using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace BokToppen.Models
{
    public class ReviewMethod
    {
        public List<ReviewModel> GetReviewsByBook(int bookId, out string errormsg)
        {
            //Skapa SQL-connection
            SqlConnection dbConnection = new SqlConnection();

            // Koppling mot server
            // dbConnection.ConnectionString = configuration.GetValue<string>("ConnectionStrings:DefaultConnection");
            dbConnection.ConnectionString = "Data Source=localhost, 1433;Database=Lab2DB;User Id=sa;Password=lab2_ReDo;Encrypt=True;TrustServerCertificate=True;";

            string query = "SELECT Tbl_Reviews.*, Tbl_User.Us_Username AS Username FROM Tbl_Reviews INNER JOIN Tbl_User On Tbl_User.Us_Id = Tbl_Reviews.Re_UserId WHERE Re_BookId = @bookId";
            SqlCommand dbCommand = new SqlCommand(query, dbConnection);

            dbCommand.Parameters.Add("bookid", SqlDbType.Int).Value = bookId;

            SqlDataReader reader = null;
            var reviewList = new List<ReviewModel>();

            errormsg = "";

            try
            {
                dbConnection.Open();

                reader = dbCommand.ExecuteReader();

                 while(reader.Read())
                {
                    ReviewModel review = new ReviewModel()

                    {
                        Points = Convert.ToInt32(reader["Re_Rating"]),
                        Comment = reader["Re_Comment"].ToString(),
                        PublishedDate = Convert.ToDateTime(reader["Re_PublishedDate"]),
                        CreatorId = Convert.ToInt32(reader["Re_UserId"]),
                        CreatorName = reader["Username"].ToString(),
                        BookId = Convert.ToInt32(reader["Re_BookId"]),
                    };

                    reviewList.Add(review);
                }

                reader.Close();
                return reviewList;
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

        public int InsertReview(ReviewModel review, out string errormsg)
        {
            //Skapa SQL-connection
            SqlConnection dbConnection = new SqlConnection();

            // Koppling mot server
            // dbConnection.ConnectionString = configuration.GetValue<string>("ConnectionStrings:DefaultConnection");
            dbConnection.ConnectionString = "Data Source=localhost, 1433;Database=Lab2DB;User Id=sa;Password=lab2_ReDo;Encrypt=True;TrustServerCertificate=True;";

            string query = "INSERT INTO Tbl_Reviews (Re_Rating, Re_Comment, Re_UserId, Re_BookId) VALUES (@rating, @comment, @userid, @bookid)";
            SqlCommand dbCommand = new SqlCommand(query, dbConnection);

            dbCommand.Parameters.Add("rating", SqlDbType.Int).Value = review.Points;
            dbCommand.Parameters.Add("comment", SqlDbType.NVarChar, 100).Value = review.Comment;
            dbCommand.Parameters.Add("userid", SqlDbType.Int).Value = review.CreatorId;
            dbCommand.Parameters.Add("bookid", SqlDbType.Int).Value = review.BookId;

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
                    errormsg = "Omdömet skapades inte";
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