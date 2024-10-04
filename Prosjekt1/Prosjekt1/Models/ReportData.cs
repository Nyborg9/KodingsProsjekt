using System.ComponentModel.DataAnnotations;

namespace Prosjekt_1.Models
{
    public class ReportData
    {
        [Required(ErrorMessage = "Du må skrive kommunen der feilen er")]
        public string Kommune { get; set; }

        [Required(ErrorMessage = "Du må skrive hva som er feil")]
        public string FreeText { get; set; }
    }
}
