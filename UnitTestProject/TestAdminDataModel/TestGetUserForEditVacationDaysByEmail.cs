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

namespace UnitTestProject.TestAdminDataModel
{
    [TestClass]
    public class TestGetUserForEditVacationDaysByEmail
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
        public void GetUserForEditVacationDaysByEmail_UserEmailNull_Null()
        {
            //arrange
            var store = new UserStore<ApplicationUser>(context);
            var userManager = new ApplicationUserManager(store);

            //act
            var actual = new AdminDataModel().GetUserForEditVacationDaysByEmail(userManager, null);

            //assert
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void GetUserForEditVacationDaysByEmail_UserEmailIsNull_IdentityResultSucceeded()
        {
            //arrange
            var store = new UserStore<ApplicationUser>(context);
            var userManager = new ApplicationUserManager(store);
            ApplicationUser user = new ApplicationUser
            {
                UserName = "TestUser@gmail.com",
                Email = "TestUser@gmail.com",
                FullName = "TestUser",
                EmploymentDate = new DateTime(2019, 03, 12),
            };
            context.Users.Add(user);
            context.SaveChanges();

            var expected = new EditUserVacationDaysViewModel()
            {
                FullName = user.FullName,
                Email = user.Email,
                EmploymentDate = user.EmploymentDate.ToShortDateString(),
                AllRoles = "",
                Vacations = new Dictionary<string, int>()
            };        

            //act
            var actual = new AdminDataModel().GetUserForEditVacationDaysByEmail(userManager, user.Email);

            //assert
            Assert.IsTrue(Equal(expected, actual));
        }

        private bool Equal(EditUserVacationDaysViewModel expected, EditUserVacationDaysViewModel actual)
        {
            if (expected.FullName == actual.FullName &&
                expected.Email == actual.Email &&
                expected.EmploymentDate == actual.EmploymentDate &&
                expected.AllRoles == actual.AllRoles &&
                expected.Vacations.Count == actual.Vacations.Count)
            {
                return true;
            }
            return false;
        }       

        [ClassCleanup]
        public static void TearDown()
        {
            context.Dispose();
        }
    }
}
