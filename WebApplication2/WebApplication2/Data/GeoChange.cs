using System.Security.Cryptography.Xml;

namespace WebApplication2.Data
{
    public class GeoChange
    {
        // Use int for auto-incrementing primary key
        public int Id { get; set; }
        public string? Description { get; set; }
        public string? GeoJson { get; set; }
        public GeoChange()
        {
            
        }

    }
}
