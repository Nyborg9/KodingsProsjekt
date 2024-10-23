namespace WebApplication2.Models
{
    public class AreaChange
    {
        public string Id { get; set; }

        //GeoJSON format - markører, linjer, polygoner
        public string GeoJson { get; set; }  
        public string Description { get; set; }

        public string MapVariant { get; set; }
        public Status Status { get; set; } // Store as enum type
    }

    public enum Status
    {
        Innsendt,
        UnderVurdering,
        IkkeTattTilFølge,
        Godkjent,
    }
}

