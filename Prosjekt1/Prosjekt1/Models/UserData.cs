using System.ComponentModel.DataAnnotations;

namespace Prosjekt_1.Models
{
    public class UserData
    {
        [Required(ErrorMessage = "Navnet ditt er påkrevd")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Emailen din er påkrevd")]
        [EmailAddress(ErrorMessage = "Ugyldig e-postadresse")]
        public string Email { get; set; }
    }
}
