using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BokToppen.Models
{
    public class UserModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Skriv in ett användarnamn")]
        [MinLength(4, ErrorMessage = "Användarnamnet måste vara minst 4 tecken långt")]
        public string? Username { get; set; }

        [EmailAddress(ErrorMessage = "Inte giltig emailadress")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Skriv in ett lösenord")]
        public string? Password { get; set; }
    }
}