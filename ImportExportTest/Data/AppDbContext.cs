using ImportExportTest.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ImportExportTest.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("NedappDB")
        {

        }

        public DbSet<PilotGroups> PilotGroups { get; set; }
    }
}