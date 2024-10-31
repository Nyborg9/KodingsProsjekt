namespace WebApplication2.Models
{
    public class GeoChange
    {
        // Properties
        public int Id { get; set; }
        public string? Description { get; set; }
        public string? GeoJson { get; set; }
        public string? Status { get; set; }
        public int? CaseworkerId { get; set; }
        public virtual CaseWorker? Caseworker { get; set; }
        public string? CaseworkerNotes { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}