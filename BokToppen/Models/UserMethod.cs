using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Data.SqlTypes;
using System.Data;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace BokToppen.Models
{
    public class UserMethod
    {
        private readonly string _connectionString;
        
        public UserMethod() {
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

        // Implementerad baserat på kodexempel från https://www.thatsoftwaredude.com/content/6218/how-to-encrypt-passwords-using-sha-256-in-c-and-net
        private byte[] CalculateSHA256(string str)
        {
            SHA256 sha256 = SHA256.Create();
            byte[] hashValue;
            UTF8Encoding objUtf8 = new UTF8Encoding();
            hashValue = sha256.ComputeHash(objUtf8.GetBytes(str));

            return hashValue;
        }

        public int FindUser(UserModel user, out string errormsg)
        {
            byte[] hashedIncomingPassword = CalculateSHA256(user.Password);
            
            SqlConnection dbConnection = NewConnection();

            string query = "SELECT Us_Id, Us_Password FROM Tbl_User WHERE Us_Username = @username";
            SqlCommand dbCommand = new SqlCommand(query, dbConnection);

            dbCommand.Parameters.Add("username", SqlDbType.NVarChar, 50).Value = user.Username;

            SqlDataReader reader = null;
            int userId = 0;

            errormsg = "";

            try
            {
                dbConnection.Open();

                reader = dbCommand.ExecuteReader();

                if (reader.Read()) 
                {
                    byte[] hasedPassword = (byte[])reader["Us_Password"];
                    bool isPasswordValid = hasedPassword.SequenceEqual(hashedIncomingPassword);
                    if (isPasswordValid)
                    {
                        userId = Convert.ToInt32(reader["Us_Id"]);
                    } else
                    {
                        errormsg = "Fel användarnamn eller lösenord";
                        return 0;
                    }
                    
                }
                else
                {
                    errormsg = "Fel användarnamn eller lösenord";
                    return 0;
                }
                reader.Close();
                return userId;
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

        public int InsertUser(UserModel us, out string errormsg)
        {

            SqlConnection dbConnection = NewConnection();

            string query = "INSERT INTO Tbl_User (Us_Username, Us_Email, Us_Password) VALUES (@username, @email, @password); SELECT SCOPE_IDENTITY()";
            SqlCommand dbCommand = new SqlCommand(query, dbConnection);

            var hashedPassword = CalculateSHA256(us.Password);

            dbCommand.Parameters.Add("username", SqlDbType.NVarChar, 50).Value = us.Username;
            dbCommand.Parameters.Add("email", SqlDbType.NVarChar, 50).Value = us.Email;
            dbCommand.Parameters.Add("password", SqlDbType.Binary, 32).Value = hashedPassword;

            try
            {
                dbConnection.Open();

                int userId = Convert.ToInt32(dbCommand.ExecuteScalar());

                // ExecuteNonQuery returns the number of rows affected
                // int count = dbCommand.ExecuteNonQuery();

                if (userId > 0)
                {
                    errormsg = "";
                }
                else
                {
                    errormsg = "Ingen användare skapades";
                }
                return userId;
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

        public string GetUserName(int? userId, out string errormsg)
        {
            SqlConnection dbConnection = NewConnection();

            string query = "SELECT Us_Username FROM Tbl_User WHERE Us_Id = @userId";
            SqlCommand dbCommand = new SqlCommand(query, dbConnection);

            dbCommand.Parameters.Add("userId", SqlDbType.Int).Value = userId;

            SqlDataReader reader = null;
            string username = "";

            errormsg = "";

            try
            {
                dbConnection.Open();

                reader = dbCommand.ExecuteReader();

                if (reader.Read())
                {
                    // Om användare inte finns så blir använtarnamnet bara ""
                    username = reader["Us_Username"].ToString();
                };

                reader.Close();
                return username;
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

        public int GetUserId(string userName, out string errormsg)
        {
            SqlConnection dbConnection = NewConnection();

            string query = "SELECT Us_Id FROM Tbl_User WHERE Us_Username = @username";
            SqlCommand dbCommand = new SqlCommand(query, dbConnection);

            dbCommand.Parameters.Add("username", SqlDbType.NVarChar, 50).Value = userName;

            SqlDataReader reader = null;
            int userId = 0;

            errormsg = "";

            try
            {
                dbConnection.Open();

                reader = dbCommand.ExecuteReader();

                if (reader.Read())
                {
                    userId = Convert.ToInt32(reader["Us_Id"]);
                };

                reader.Close();
                return userId;
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

        public List<UserModel> GetUsers(out string errormsg)
        {
            SqlConnection dbConnection = NewConnection();

            string query = "SELECT * FROM Tbl_User";
            SqlCommand dbCommand = new SqlCommand(query, dbConnection);

            SqlDataReader reader = null;
            var userList = new List<UserModel>();

            errormsg = "";

            try
            {
                dbConnection.Open();

                reader = dbCommand.ExecuteReader();

                while(reader.Read())
                {
                    UserModel user = new UserModel()
                    { 
                        Id = Convert.ToInt32(reader["Us_Id"]),
                        Username = reader["Us_Username"].ToString(), 
                        Email = reader["Us_Email"].ToString()
                    };

                    userList.Add(user);
                }
                reader.Close();
                return userList;
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

        public int DeleteUser(int Id, out string errormsg)
        {

            SqlConnection dbConnection = NewConnection();

            string query = "DELETE FROM Tbl_User WHERE Us_Id = @userId";
            SqlCommand dbCommand = new SqlCommand(query, dbConnection);

            dbCommand.Parameters.Add("userId", SqlDbType.Int).Value = Id;

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
                    errormsg = "Gick inte att ta bort användaren";
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