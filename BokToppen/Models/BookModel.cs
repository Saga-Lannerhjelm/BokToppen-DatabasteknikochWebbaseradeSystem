using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BokToppen.Models
{
    public class BookModel
    {
        public int Id { get; set; }

        public int? UserId { get; set; }

        [Required(ErrorMessage = "Fältet kan inte vara tomt")]        
        public string? Title { get; set; }
        public string? Category { get; set; }

        [Required(ErrorMessage = "Fältet kan inte vara tomt")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Fältet kan inte vara tomt")]
        [StringLength(1000, ErrorMessage = "Beskrivningen kan inte vara mer än 1000 tecken lång")]
        public string? Description {get; set;}

        // RegEx hämtad från https://www.geeksforgeeks.org/regular-expressions-to-validate-isbn-code/ den 9 sep 2024
        [RegularExpression("^(?=(?:[^0-9]*[0-9]){10}(?:(?:[^0-9]*[0-9]){3})?$)[\\d-]+$", ErrorMessage = "Fältet är inte en godkänd ISBN-kod")]
        [Required(ErrorMessage = "Fältet kan inte vara tomt")]
        public string? ISBN { get; set; }

        [Required(ErrorMessage = "Fältet kan inte vara tomt")]
        public int PublicationYear { get; set; }

        [DataType(DataType.Date)]
        public DateTime PublishedDate { get; set; } = DateTime.Now;
    }
}