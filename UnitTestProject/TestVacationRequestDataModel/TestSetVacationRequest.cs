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

namespace UnitTestProject.TestVacationRequestDataModel
{
    [TestClass]
    public class TestSetVacationRequest
    {
        static ApplicationDbContext context;

        [ClassInitialize]
        public static void SetUp(TestContext dBcontext)
        {
            var dropper = new DropCreateDatabaseAlways<ApplicationDbContext>();
            dropper.InitializeDatabase(new ApplicationDbContext());
            context = new ApplicationDbContext();
        }

        [TestMethod]
        public void SetVacationRequest_UserIdIsNull_IsNull()
        {
            //arrange
            var store = new UserStore<ApplicationUser>(context);
            var userManager = new ApplicationUserManager(store);

            //act
            var actual = new VacationRequestDataModel(null).GetVacationRequest(userManager, null);

            //assert
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void GetUserHistoryVacation_TestHistoryInitializel_IsNull()
        {
            //arrange
            var store = new UserStore<ApplicationUser>(context);
            var userManager = new ApplicationUserManager(store);
            var expected = TestRequestInitialize(context, "user");
            var user = context.Users.Where(x => x.Email == "user@gmail.com").First();

            //act
            var actual = new VacationRequestDataModel(null).GetVacationRequest(userManager, user.Id);

            //assert
            Assert.IsTrue(Equal(expected, actual));
        }

        private bool Equal(VacationRequestViewModels expected, VacationRequestViewModels actual)
        {
            if (expected.User.Email != actual.User.Email ||
                expected.VacationTypes.Count != actual.VacationTypes.Count ||
                expected.Approvers.Count != actual.Approvers.Count 
                )
            {
                return false;
            }

            return true;
        }

        [ClassCleanup]
        public static void TearDown()
        {
            context.Dispose();
        }

        private VacationRequestViewModels TestRequestInitialize(ApplicationDbContext context, string userName/*ApplicationUser manager*/)
        {
            VacationRequestViewModels result = new VacationRequestViewModels();

            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var roleresult = RoleManager.Create(new IdentityRole("Employee"));
            roleresult = RoleManager.Create(new IdentityRole("Manager"));
            roleresult = RoleManager.Create(new IdentityRole("Admin"));

            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            ApplicationUser user = new ApplicationUser
            {
                UserName = userName + "@gmail.com",
                Email = userName + "@gmail.com",
                FullName = userName,
                EmploymentDate = new DateTime(2019, 03, 12)
            };

            if (userManager.Create(user, "123456-Pass").Succeeded)
            {
                userManager.AddToRole(user.Id, "Employee");
            }
            result.User = user;

            ApplicationUser manager = new ApplicationUser
            {
                UserName = "Manager@gmail.com",
                Email = "Manager@gmail.com",
                FullName = "Manager",
                EmploymentDate = DateTime.Now.Date
            };
            result.Approvers = new List<ApplicationUser>() { manager };

            if (userManager.Create(manager, "123456-Pass").Succeeded)
            {
                userManager.AddToRole(manager.Id, "Manager");
            }

            VacationTypes testVacationType = new VacationTypes()
            {
                Name = "TestVacationType",
                MaxDays = 30
            };
            context.VacationTypes.Add(testVacationType);

            result.VacationTypes = new List<VacationTypes>() { testVacationType };
       
            context.SaveChanges();
            return result;
        }
    }
}
