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
    public class TestReject
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
        public void Reject_RequestChecksIdIsNull_NoException()
        {
            //arrange
            //act
            try
            {
                new ListActiveRequests().Reject(null, "");
            }
            catch
            {
                //assert
                Assert.IsTrue(false);
                return;
            }
            Assert.IsTrue(true);

        }

        [TestMethod]
        public void Reject_TestRequestsInitialize_RequestRejected()
        {
            //arrange
            var store = new UserStore<ApplicationUser>(context);
            var userManager = new ApplicationUserManager(store);
            var id = TestRequestsInitialize(context, "manager");
            var user = context.Users.Where(x => x.Email == "manager@gmail.com").First();

            //act
            new ListActiveRequests().Reject(id, "You can't go");
            using (ApplicationDbContext newContext = new ApplicationDbContext())
            {

                var actual = newContext.RequestChecks.Where(x => x.Id == id).First();

                //assert
                Assert.AreEqual("Rejected", actual.Status.Name);
            }

        }

        [TestMethod]
        public void Reject_TestRequestsInitialize_RequestRejecteHaveReason()
        {
            //arrange
            var store = new UserStore<ApplicationUser>(context);
            var userManager = new ApplicationUserManager(store);
            var id = TestRequestsInitialize(context, "manager2");
            var user = context.Users.Where(x => x.Email == "manager2@gmail.com").First();
            var expected = "You can't go";

            //act
            new ListActiveRequests().Reject(id, expected);
            using (ApplicationDbContext newContext = new ApplicationDbContext())
            {

                var actual = newContext.RequestChecks.Where(x => x.Id == id).First();

                //assert
                Assert.AreEqual(expected, actual.Reason);
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

        private int? TestRequestsInitialize(ApplicationDbContext context, string managerName)
        {
            int? result;
       
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            ApplicationUser user = new ApplicationUser
            {
                UserName = "User@gmail.com",
                Email = "User@gmail.com",
                FullName = "User",
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
