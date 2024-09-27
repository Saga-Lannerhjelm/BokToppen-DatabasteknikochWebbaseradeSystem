using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BokToppen.Models
{
    public class ReviewModel
    {
        public int Id { get; set; }

        public int BookId { get; set;}

        public int CreatorId { get; set; }
        public string? CreatorName { get; set; }

        [Required(ErrorMessage = "Du måste välja ett betyg")]
        [Range(1, 5, ErrorMessage = "Du måste välja ett betyg mellan 1 och 5")]
        public int Points { get; set; }

        [Required(ErrorMessage = "Fältet kan inte vara tomt")]
        [MinLength(5, ErrorMessage = "Kommentaren måste vara minst 5 tecken långt")]
        [StringLength(100, ErrorMessage = "Kommentaren är för lång. Den kan inte var längre än 100 tecken")]
        public string? Comment { get; set; }

        public DateTime PublishedDate { get; set; } = DateTime.Now;
    }
}