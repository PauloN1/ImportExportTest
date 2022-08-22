namespace ImportExportTest.Migrations
{
    using ImportExportTest.Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<ImportExportTest.Data.AppDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(ImportExportTest.Data.AppDbContext context)
        {
            if (!context.PilotGroups.Any())
            {
                PilotGroups[] groupList = 
                {
                    new PilotGroups { NameOrDesc = "ConfigGroup", Active=true, StartDate=DateTime.Now.AddDays(1),
                        AuditDate = DateTime.Now.AddDays(7), EndDate = DateTime.Now.AddDays(6), AuditUser = "User 1"
                    },
                     new PilotGroups { NameOrDesc = "QwertyGroup", Active=true, StartDate=DateTime.Now.AddDays(10),
                        AuditDate = DateTime.Now.AddDays(7), EndDate = DateTime.Now.AddDays(16), AuditUser = "User 1"
                    },
                      new PilotGroups { NameOrDesc = "NewConfig", Active=false, StartDate=DateTime.Now.AddDays(1),
                        AuditDate = DateTime.Now.AddDays(5), EndDate = DateTime.Now.AddDays(6), AuditUser = "User 2"
                    },
                       new PilotGroups { NameOrDesc = "BBDClient", Active=true, StartDate=DateTime.Now.AddDays(2),
                        AuditDate = DateTime.Now.AddDays(15), EndDate = DateTime.Now.AddDays(5), AuditUser = "User 13"
                    },
                        new PilotGroups { NameOrDesc = "AWSCongfig", Active=true, StartDate=DateTime.Now.AddDays(1),
                        AuditDate = DateTime.Now.AddDays(7), EndDate = DateTime.Now.AddDays(6), AuditUser = "User 4"
                        }

                };

                foreach (var item in groupList)
                {
                    context.PilotGroups.Add(new PilotGroups
                    {
                        Active = item.Active,
                        NameOrDesc = item.NameOrDesc,
                        StartDate = item.StartDate,
                        AuditDate = item.AuditDate,
                        EndDate = item.EndDate,
                        AuditUser = item.AuditUser
                    });
                }
            }
            context.SaveChanges();
        }
    }
}
