﻿using System.ComponentModel.DataAnnotations;
using WebApplication2.Models;

namespace WebApplication2.Data
{
    public class GeoChange
    {
        public int Id { get; set; }

        [Required]
        public string? Description { get; set; }

        [Required]
        public string? GeoJson { get; set; }

        [Required]
        public string UserId { get; set; } // Foreign key to associate with a user

        [Required]
        public string MapVariant { get; set; }

        // Navigation property
        public virtual ApplicationUser? User { get; set; }

        // New properties for Status and Priority
        public ReportStatus? Status { get; set; }
        public PriorityLevel? Priority { get; set; }

        public string MunicipalityName { get; set; }
        public string MunicipalityNumber { get; set; }

        public string CountyName { get; set; }
    }
}