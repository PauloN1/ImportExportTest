using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImportExportTest.Models
{
    public class PilotGroups
    {
        public int Id { get; set; }
        public string NameOrDesc { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime AuditDate { get; set; }
        public string AuditUser { get; set; }
        public bool Active { get; set; }
    }
}