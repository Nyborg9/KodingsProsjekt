namespace WebApplication2.Models
{
    public enum PriorityLevel
    {
        Lav = 1,        // Low
        Vanlig = 2,     // Normal/Medium
        Høy = 3         // High
    }

    public enum ReportStatus
    {
        IkkePåbegynt = 1,    // Not Started
        UnderBehandling = 2, // In Progress
        Avsluttet = 3,       // Finished
        IkkeTattTilFølge = 4 // Not Followed Up
    }
}