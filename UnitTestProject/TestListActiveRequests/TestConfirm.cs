using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.QualityTools.UnitTestFramework;
using System;
using System.Activities.Statements;
using System.Collections.Generic;

using TimeOffTracker;
using TimeOffTracker.Business;
using TimeOffTracker.Models;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Linq;
using Microsoft.AspNet.Identity;
using System.Web;
using System.Web.Mvc;
using TimeOffTracker.Models.ManagerModels;

namespace UnitTestProject.TestListActiveRequests
{
    [TestClass]
    public class TestConfirm
    {
        static ApplicationDbContext context;

        [TestInitialize]
        public void SetUp()
        {
            var dropper = new DropCreateDatabaseAlways<ApplicationDbContext>();
            dropper.InitializeDatabase(new ApplicationDbContext());
            context = new ApplicationDbContext();
            StartInitialize(context);
        }

        [TestMethod]
        public void Confirm_RequestChecksIdIsNull_Exception()
        {
            //arrange
            //act
            try
            {
                new ListActiveRequests().Confirm(null);
            }
            catch
            {
                //assert
                Assert.IsTrue(true);
                return;
            }
            Assert.IsTrue(false);

        }

        [TestMethod]
        public void Confirm_TestRequestsInitialize_RequestPassed()
        {
            //arrange
            var store = new UserStore<ApplicationUser>(context);
            var userManager = new ApplicationUserManager(store);
            var id = TestRequestsInitialize(context, "manager", "Passed");
            var user = context.Users.Where(x => x.Email == "manager@gmail.com").First();

            //act
            new ListActiveRequests().Confirm(id);
            using(ApplicationDbContext newContext = new ApplicationDbContext())
            {

            var actual = newContext.RequestChecks.Where(x => x.Id == id).First();

            //assert
            Assert.AreEqual("Passed", actual.Status.Name);
            }
        }

        [TestMethod]
        public void Confirm_TestRequestsInitialize_27DaysInUser()
        {
            //arrange
            var store = new UserStore<ApplicationUser>(context);
            var userManager = new ApplicationUserManager(store);
            string userName = "User27";
            string managetName = "Manager27";
            int expected = 27;
            var id = TestRequestsInitialize(context, managetName, userName);
            var manager = context.Users.Where(x => x.Email == managetName + "@gmail.com").First();

            //act
            new ListActiveRequests().Confirm(id);
            using (ApplicationDbContext newContext = new ApplicationDbContext())
            {
                var user = newContext.RequestChecks.Where(x => x.Id == id).First().Request.Employee;
                var actual = newContext.UserVacationDays.Where(x => x.User.Id == user.Id).First();

                //assert
                Assert.AreEqual(expected, actual.VacationDays);
            }
        }

        [TestCleanup]
        public void TearDown()
        {
            context.Dispose();
        }

        private void StartInitialize(ApplicationDbContext context)
        {
            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var roleresult = RoleManager.Create(new IdentityRole("Employee"));
            roleresult = RoleManager.Create(new IdentityRole("Manager"));
            roleresult = RoleManager.Create(new IdentityRole("Admin"));

            VacationTypes testVacationType = new VacationTypes()
            {
                Name = "TestVacationType",
                MaxDays = 30
            };
            context.VacationTypes.Add(testVacationType);

            var requestStatuses = new List<RequestStatuses>()
            {
                new RequestStatuses(){ Name = "Passed" },
                new RequestStatuses(){ Name = "Rejected" },
                new RequestStatuses(){ Name = "Waiting" }
            };

            context.RequestStatuses.AddRange(requestStatuses);

            context.SaveChanges();
        }

        private int? TestRequestsInitialize(ApplicationDbContext context, string managerName, string userName)
        {
            int? result;

            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));


            ApplicationUser user = new ApplicationUser
            {
                UserName = userName + "@gmail.com",
                Email = userName + "@gmail.com",
                FullName = userName,
                EmploymentDate = DateTime.Now.Date
            };

            if (userManager.Create(user, "123456-Pass").Succeeded)
            {
                userManager.AddToRole(user.Id, "Employee");
            }

            ApplicationUser manager = new ApplicationUser
            {
                UserName = managerName + "@gmail.com",
                Email = managerName + "@gmail.com",
                FullName = managerName,
                EmploymentDate = new DateTime(2019, 03, 12)
            };

            if (userManager.Create(manager, "123456-Pass").Succeeded)
            {
                userManager.AddToRole(manager.Id, "Manager");
            }

            VacationTypes testVacationType = context.VacationTypes.Where(x => x.Id == 1).First();

            UserVacationDays userVacDays = new UserVacationDays()
            {
                User = user,
                VacationDays = testVacationType.MaxDays,
                VacationType = testVacationType,
                LastUpdate = user.EmploymentDate,
            };
            context.UserVacationDays.Add(userVacDays);

            Requests request = new Requests()
            {
                Employee = user,
                VacationTypes = testVacationType,
                DateStart = user.EmploymentDate.AddDays(1),
                DateEnd = user.EmploymentDate.AddDays(1 + 2),
                Description = 1.ToString()
            };
            context.Requests.Add(request);

            var waitStatus = context.RequestStatuses.Where(x => x.Name == "Waiting").First();

            RequestChecks requestChecks = new RequestChecks()
            {
                Request = request,
                Priority = 1,
                Status = waitStatus,
                Approver = manager
            };
            context.RequestChecks.Add(requestChecks);

            context.SaveChanges();

            var temp = context.RequestChecks.Where(x => x.Approver.Email == manager.Email).First();
            result = temp.Id;
            return result;
        }

    }
}
