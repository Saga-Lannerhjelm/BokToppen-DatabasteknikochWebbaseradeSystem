using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BokToppen.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace BokToppen.Controllers
{
    public class FileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult UploadFile(IFormFile postedFile)
        {

            string fileName = Path.GetFileName(postedFile.FileName);
            string contentType = postedFile.ContentType;
            var fileM = new FileMethod();
            using (MemoryStream ms = new MemoryStream())
            {
                // Upload the file if less than 2 MB
            if (ms.Length < 2097152)
            {
                postedFile.CopyTo(ms);
                int rowsAffected = fileM.Upload(postedFile, ms, out string error);

                 if (error != "" && error != null)
                {
                    TempData["unsuccessful"] = "Något blev fel. " + error;
                }
                 return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("File", "The file is too large.");
                return View();
            }

                

                // string constr = this.Configuration.GetConnectionString("MyConn");
                // using (SqlConnection con = new SqlConnection(constr))
                // {
                //     string query = "INSERT INTO tblFiles VALUES (@Name, @ContentType, @Data)";
                //     using (SqlCommand cmd = new SqlCommand(query))
                //     {
                //         cmd.Connection = con;
                //         cmd.Parameters.AddWithValue("@Name", fileName);
                //         cmd.Parameters.AddWithValue("@ContentType", contentType);
                //         cmd.Parameters.AddWithValue("@Data", ms.ToArray());
                //         con.Open();
                //         cmd.ExecuteNonQuery();
                //         con.Close();
                //     }
                // }
            }
    
           
        }
    
        // [HttpPost]
        // public IActionResult DownloadFile(int fileId)
        // {
        //     byte[] bytes;
        //     string fileName, contentType;
        //     string constr = this.Configuration.GetConnectionString("MyConn");
        //     using (SqlConnection con = new SqlConnection(constr))
        //     {
        //         using (SqlCommand cmd = new SqlCommand())
        //         {
        //             cmd.CommandText = "SELECT Name, Data, ContentType FROM tblFiles WHERE Id=@Id";
        //             cmd.Parameters.AddWithValue("@Id", fileId);
        //             cmd.Connection = con;
        //             con.Open();
        //             using (SqlDataReader sdr = cmd.ExecuteReader())
        //             {
        //                 sdr.Read();
        //                 bytes = (byte[])sdr["Data"];
        //                 contentType = sdr["ContentType"].ToString();
        //                 fileName = sdr["Name"].ToString();
        //             }
        //             con.Close();
        //         }
        //     }
    
        //     return File(bytes, contentType, fileName);
        // }

        [HttpGet]
        public IActionResult File(int id)
        {
            // FileModel files = new FileModel();
            var fileM = new FileMethod();

            FileModel file = fileM.GetFile(id, out string error);

            // Konvertera byte-arrayen till en Base64-sträng om filen finns
            if (file != null && file.Data != null)
            {
                string base64String = Convert.ToBase64String(file.Data, 0,file.Data.Length);
                string imageUrl = $"data:{file.ContentType};base64,{base64String}";

                // Skicka Base64-strängen till vyn
                ViewBag.ImageUrl = imageUrl;
            }

            return View(file);

            // string constr = this.Configuration.GetConnectionString("MyConn");
            // using (SqlConnection con = new SqlConnection(constr))
            // {
            //     using (SqlCommand cmd = new SqlCommand("SELECT Id, Name FROM tblFiles"))
            //     {
            //         cmd.Connection = con;
            //         con.Open();
            //         using (SqlDataReader sdr = cmd.ExecuteReader())
            //         {
            //             while (sdr.Read())
            //             {
            //                 files.Add(new FileModel
            //                 {
            //                     Id = Convert.ToInt32(sdr["Id"]),
            //                     Name = sdr["Name"].ToString()
            //                 });
            //             }
            //         }
            //         con.Close();
            //     }
            // }
            // return files;
        }
    }
}