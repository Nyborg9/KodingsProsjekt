namespace WebApplication2.Models
{
    // Enumerations for the priority level and status of a report
    public enum PriorityLevel
    {
        Lav = 1,        
        Vanlig = 2,     
        Høy = 3         
    }

    public enum ReportStatus
    {
        IkkePåbegynt = 1,    
        UnderBehandling = 2, 
        Ferdigstilt = 3,       
        IkkeTattTilFølge = 4 
    }
}