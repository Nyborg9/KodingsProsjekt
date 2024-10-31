using WebApplication2.Models;

namespace WebApplication2.Data
{
    public class GeoChange
    {
        // Primary key
        public int Id { get; set; }
        
        // Description provided by the user about the geospatial change
        public string? Description { get; set; }
        
        // GeoJSON data for map integration
        public string? GeoJson { get; set; }
        
        // Status of the change request (e.g., Pending, In Progress, Completed)
        public string? Status { get; set; }
        
        // Foreign key for the assigned caseworker
        public int? CaseworkerId { get; set; }
        
        // Navigation property for the associated CaseWorker
        public virtual CaseWorker? Caseworker { get; set; }
        
        // Caseworker's comments or notes on the geospatial change
        public string? CaseworkerNotes { get; set; }
        
        // Timestamp for when the change was last updated
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}