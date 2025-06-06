using Spiro_Andon.Models;
using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer;

namespace Spiro_Andon.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Incident_Report> Incident_Report { get; set; }

        public DbSet<Quality> Quality { get; set; }
        public DbSet<QualityReport> QualityReports { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Incident_Report>().ToTable("Incident_Report");
        }
    }

}
