namespace WebApplication2.Models
{
    public class AreaChange
    {
        public string Id { get; set; }

        //GeoJSON format - markører, linjer, polygoner
        public string GeoJson { get; set; }  
        public string Description { get; set; }

    }
}