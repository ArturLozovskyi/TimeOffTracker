namespace TimeOffTracker.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using System;
    using System.Data.Entity.Migrations;


    internal sealed class Configuration : DbMigrationsConfiguration<TimeOffTracker.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "TimeOffTracker.Models.ApplicationDbContext";
        }

        protected override void Seed(TimeOffTracker.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.

            ApplicationUser user = new ApplicationUser { UserName = "Admin@gmail.com", Email = "Admin@gmail.com", FullName = "Администратор", daysVacationInYear = 0, dateCreateAccount = DateTime.Now.Date };
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            IdentityResult result = UserManager.Create(user, "Sfzom#231");

            if (result.Succeeded)
            {
                var RoleManager = new RoleManager<IdentityRole>(
                  new RoleStore<IdentityRole>(context));
                var roleresult = RoleManager.Create(new IdentityRole("Employee"));
                roleresult = RoleManager.Create(new IdentityRole("Manager"));
                roleresult = RoleManager.Create(new IdentityRole("Admin"));
                result = UserManager.AddToRole(user.Id, "Admin");
            }

        }
    }
}
