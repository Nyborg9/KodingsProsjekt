namespace WebApplication2.Models
{
    public class CaseWorker
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public virtual ICollection<GeoChange> GeoChanges { get; set; }
    }
}