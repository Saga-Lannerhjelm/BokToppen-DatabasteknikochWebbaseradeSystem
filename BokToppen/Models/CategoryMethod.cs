using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace BokToppen.Models
{
    public class CategoryMethod
    {
        private readonly string _connectionString;
        
        public CategoryMethod() {
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


        public List<CategoryModel> GetCategories(out string errormsg)
        {
            SqlConnection dbConnection = NewConnection();

            string query = "SELECT * FROM Tbl_Categories";
            SqlCommand dbCommand = new SqlCommand(query, dbConnection);

            SqlDataReader reader = null;
            var categoryList = new List<CategoryModel>();

            errormsg = "";

            try
            {
                dbConnection.Open();

                reader = dbCommand.ExecuteReader();

                while(reader.Read())
                {
                    var category = new CategoryModel()
                    {
                        Id = Convert.ToInt32(reader["Ca_Id"]),
                        Category = reader["Ca_Category"].ToString()
                    };

                    categoryList.Add(category);
                }
                reader.Close();
                return categoryList;
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