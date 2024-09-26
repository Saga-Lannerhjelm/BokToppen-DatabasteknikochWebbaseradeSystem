using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BokToppen.Models
{
    public class UserModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Skriv in ett användarnamn")]
        public string? Username { get; set; }

        // [Required(ErrorMessage = "Skriv in ett email")]
        [EmailAddress(ErrorMessage = "Inte giltig emailadress")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Skriv in ett lösenord")]
        public string? Password { get; set; }
    }
}