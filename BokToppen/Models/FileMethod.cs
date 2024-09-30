using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace BokToppen.Models
{
    public class FileMethod
    {

        private readonly string _connectionString;
        
        public FileMethod() {
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
        
        public int Upload(IFormFile postedFile, MemoryStream ms, out string errormsg)
        {
            
            SqlConnection dbConnection = NewConnection();

            string query = "INSERT INTO Tbl_Files (Fi_Name, Fi_ContentType, Fi_Data) VALUES (@name, @contentType, @data)";
            SqlCommand dbCommand = new SqlCommand(query, dbConnection);

            dbCommand.Parameters.Add("name", SqlDbType.NVarChar, 50).Value = postedFile.Name;
            dbCommand.Parameters.Add("contentType", SqlDbType.NVarChar, 50).Value = postedFile.ContentType;
            dbCommand.Parameters.Add("data", SqlDbType.VarBinary).Value = ms.ToArray();

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
                    errormsg = "Gick inte att läggat till fil";
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



        public FileModel GetFile(out string errormsg)
        {
            SqlConnection dbConnection = NewConnection();

            string query = "SELECT * FROM Tbl_Files WHERE Fi_Id = 3";
            SqlCommand dbCommand = new SqlCommand(query, dbConnection);

            // dbCommand.Parameters.Add("id", SqlDbType.Int).Value = fileId;

            SqlDataReader reader = null;
            var file = new FileModel();

            errormsg = "";

            try
            {
                dbConnection.Open();

                reader = dbCommand.ExecuteReader();

                while (reader.Read())
                {

                    file.Id = Convert.ToInt32(reader["Fi_Id"]);
                    file.Name = reader["Fi_Name"].ToString();
                    file.ContentType = reader["Fi_ContentType"].ToString();
                    file.Data = (byte[])reader["Fi_Data"];
                };

                if (file.Name == null)
                {
                    errormsg = "Gick inte att hämta filen";
                    return null;
                }
                reader.Close();
                return file;
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
    }
}