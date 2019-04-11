namespace TimeOffTracker.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;


    internal sealed class Configuration : DbMigrationsConfiguration<TimeOffTracker.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "TimeOffTracker.Models.ApplicationDbContext";
        }

        /*
        * Исправить почту, которая будет начинаться с маленькой буквы
        * Больничный отпуск ?
        */

        protected override void Seed(TimeOffTracker.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.

            ApplicationUser user = new ApplicationUser
            {
                UserName = "Admin@gmail.com",
                Email = "Admin@gmail.com",
                FullName = "Administrator",
                EmploymentDate = DateTime.Now.Date
            };
            
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            IdentityResult result = UserManager.Create(user, "Sfzom#231");

            if (result.Succeeded)
            {
                var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                var roleresult = RoleManager.Create(new IdentityRole("Employee"));
                roleresult = RoleManager.Create(new IdentityRole("Manager"));
                roleresult = RoleManager.Create(new IdentityRole("Admin"));
                result = UserManager.AddToRole(user.Id, "Admin");
            }

            InitVacationTypes(context);     // Инициализация типов отпусков и кол-ва максимально возможных дней для них
            InitRequestStatuses(context);   // Инициализация статусов ожиданий
            CreateStartPeople(context);     // Добавление в БД несколько работников

            // Создание запроса ( убрать, когда будет реализована возможность создания запроса )
           // CreateTestRequest(context);
            
        }

        private void CreateTestRequest(ApplicationDbContext context)
        {
            //@
            ApplicationUser startPerson = new ApplicationUser
            {
                UserName = "employee2@gmail.com",
                Email = "employee2@gmail.com",
                FullName = "Employee",
                EmploymentDate = DateTime.Now.Date
            };

            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            if (userManager.Create(startPerson, "123456-Pass").Succeeded)
            { userManager.AddToRole(startPerson.Id, "Employee"); }
            //@

            //@
            ApplicationUser startPerson2 = new ApplicationUser
            {
                UserName = "employee3@gmail.com",
                Email = "employee3@gmail.com",
                FullName = "Employee",
                EmploymentDate = DateTime.Now.Date
            };
            var userManager12 = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            if (userManager12.Create(startPerson2, "123456-Pass").Succeeded)
            { userManager12.AddToRole(startPerson2.Id, "Employee"); }
            //@

            //@
            ApplicationUser startManager = new ApplicationUser
            {
                UserName = "manager2@gmail.com",
                Email = "manager2@gmail.com",
                FullName = "Manager",
                EmploymentDate = DateTime.Now.Date
            };

            var userManager2 = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            if (userManager2.Create(startManager, "123456-Pass").Succeeded)
            { userManager2.AddToRole(startManager.Id, "Manager"); }
            //@

            //@
            ApplicationUser middleManager = new ApplicationUser
            {
                UserName = "manager3@gmail.com",
                Email = "manager3@gmail.com",
                FullName = "Manager",
                EmploymentDate = DateTime.Now.Date
            };

            var userManager3 = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            if (userManager3.Create(middleManager, "123456-Pass").Succeeded)
            { userManager3.AddToRole(middleManager.Id, "Manager"); }
            //@

            //@
            ApplicationUser endManager = new ApplicationUser
            {
                UserName = "manager4@gmail.com",
                Email = "manager4@gmail.com",
                FullName = "Manager",
                EmploymentDate = DateTime.Now.Date
            };

            var userManager4 = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            if (userManager4.Create(endManager, "123456-Pass").Succeeded)
            { userManager4.AddToRole(endManager.Id, "Manager"); }




            // Тестовый тип отпуска
            var vacationType = new VacationTypes() { Name = "Тестовый", MaxDays = 10 };

            Requests requests = new Requests()
            {
                Employee = startPerson,
                VacationTypes = vacationType,
                DateStart = DateTime.Now.Date,
                DateEnd = DateTime.Now.AddDays(5),
                Description = "Хочу в отпуск"
            };
            context.Requests.Add(requests);

            Requests requests2 = new Requests()
            {
                Employee = startPerson2,
                VacationTypes = vacationType,
                DateStart = DateTime.Now.Date.AddDays(2),
                DateEnd = DateTime.Now.AddDays(7),
                Description = "Друг идет в армию"
            };
            context.Requests.Add(requests2);
            // Зависимость в ManageController.cs
            RequestStatuses status = new RequestStatuses() { Name = "Ожидание1" };
            RequestChecks requestChecks = new RequestChecks()
            {
                Request = requests,
                Priority = 1,
                Status = status,
                Approver = startManager
            };
            
            RequestChecks requestChecks1 = new RequestChecks()
            {
                Request = requests,
                Priority = 2,
                Status = status,
                Approver = middleManager
            };

            RequestChecks requestChecks2 = new RequestChecks()
            {
                Request = requests,
                Priority = 3,
                Status = status,
                Approver = endManager
            };

            RequestChecks requestChecks3 = new RequestChecks()
            {
                Request = requests2,
                Priority = 1,
                Status = status,
                Approver = startManager
            };
            context.RequestChecks.Add(requestChecks3);
            context.RequestChecks.AddRange(new List<RequestChecks>() { requestChecks, requestChecks1, requestChecks2 });
        }

        // Зависимость в ListActiveRequest.cs
        private void InitRequestStatuses(ApplicationDbContext context)
        {
            var requestStatuses = new List<RequestStatuses>()
            {
                new RequestStatuses(){ Name = "Passed" },
                new RequestStatuses(){ Name = "Rejected" },
                new RequestStatuses(){ Name = "Waiting" }
            };

            context.RequestStatuses.AddRange(requestStatuses);
        }

        private void InitVacationTypes(ApplicationDbContext context)
        {
            List<VacationTypes> vacationTypes = new List<VacationTypes>() {
                new VacationTypes(){ Name = "Административный", MaxDays = 15 },
                new VacationTypes(){ Name = "Учебный", MaxDays = 10 },
                new VacationTypes(){ Name = "Больничный", MaxDays = 30 }, // Мутный отпуск 
                new VacationTypes(){ Name = "Ежегодный", MaxDays = 20 }
            };
            context.VacationTypes.AddRange(vacationTypes);
        }

        // Стандартный набор чуваков
        private void CreateStartPeople(ApplicationDbContext context)
        {
            CreatePerson(context, "manager1@gmail.com", "Manager");
            CreatePerson(context, "employee1@gmail.com", "Employee");
        }

        // Создание чувака
        // Стандартный пароль для менеджера и рабочего: 123456-Pass
        private void CreatePerson(ApplicationDbContext context, string email, string role)
        {
            ApplicationUser startPerson = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = role,
                EmploymentDate = DateTime.Now.Date
            };

            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            if (userManager.Create(startPerson, "123456-Pass").Succeeded)
            {
                userManager.AddToRole(startPerson.Id, role);
            }
        }
    }
}
