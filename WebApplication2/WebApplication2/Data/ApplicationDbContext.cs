using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using WebApplication2.Models;

namespace WebApplication2.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSet for GeoChange
        public DbSet<GeoChange> GeoChanges { get; set; }
    }
}
