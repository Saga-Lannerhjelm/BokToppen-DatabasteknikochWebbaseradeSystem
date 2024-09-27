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
        private readonly string _connectionString;
        
        public ReviewMethod() {
            _connectionString = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ConnectionStrings")["DefaultConnection"];
        }

        private SqlConnection NewConnection()
        {
            //Skapa SQL-connection
            SqlConnection dbConnection = new SqlConnection();

            // Koppling mot server
            dbConnection.ConnectionString = _connectionString;
            return dbConnection;
        }

        public List<ReviewModel> GetReviewsByBook(int bookId, out string errormsg)
        {
            SqlConnection dbConnection = NewConnection();

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

                while (reader.Read())
                {
                    ReviewModel review = new ReviewModel()

                    {
                        Id = Convert.ToInt32(reader["Re_Id"]),
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

         public int GetReviewId(int reviewId, out string errormsg)
        {
            SqlConnection dbConnection = NewConnection();

            string query = "SELECT Re_Id FROM Tbl_Reviews WHERE Re_Id = @reviewId";
            SqlCommand dbCommand = new SqlCommand(query, dbConnection);

            dbCommand.Parameters.Add("reviewId", SqlDbType.Int).Value = reviewId;

            SqlDataReader reader = null;
            int id = 0;

            errormsg = "";

            try
            {
                dbConnection.Open();

                reader = dbCommand.ExecuteReader();

                if (reader.Read())
                {
                    id = Convert.ToInt32(reader["Re_Id"]);
                }

                reader.Close();
                return id;
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

        public int InsertReview(ReviewModel review, out string errormsg)
        {
            SqlConnection dbConnection = NewConnection();

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


        public int DeleteReview(int Id, out string errormsg)
        {

            SqlConnection dbConnection = NewConnection();

            string query = "DELETE FROM Tbl_Reviews WHERE Re_Id = @reviewId";
            SqlCommand dbCommand = new SqlCommand(query, dbConnection);

            dbCommand.Parameters.Add("reviewId", SqlDbType.Int).Value = Id;

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
                    errormsg = "Gick inte att ta bort omdömet";
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