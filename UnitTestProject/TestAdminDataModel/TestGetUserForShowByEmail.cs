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

namespace UnitTestProject.TestAdminDataModel
{
    [TestClass]
    public class TestGetUserForShowByEmail
    {
        ApplicationDbContext context;

        [TestInitialize]
        public void SetUp()
        {
            var dropper = new DropCreateDatabaseAlways<ApplicationDbContext>();
            dropper.InitializeDatabase(new ApplicationDbContext());
            context = new ApplicationDbContext();
        }

        [TestMethod]
        public void GetUserForShowByEmail_UserEmailNull_Null()
        {
            //arrange
            var store = new UserStore<ApplicationUser>(context);
            var userManager = new ApplicationUserManager(store);

            var expected = new ShowUserViewModel();

            //act
            var actual = new AdminDataModel().GetUserForShowByEmail(userManager, null);

            //assert
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void GetUserForShowByEmail_CreateAndGetTestUser_UserExist()
        {
            //arrange
            var store = new UserStore<ApplicationUser>(context);
            var userManager = new ApplicationUserManager(store);

            ApplicationUser user = new ApplicationUser
            {
                UserName = "Test@gmail.com",
                Email = "Test1@gmail.com",
                FullName = "Test",
                EmploymentDate = new DateTime(2019, 03, 12)
            };

            var expected = new ShowUserViewModel()
            {
                FullName = user.FullName,
                Email = user.Email,
                EmploymentDate = user.EmploymentDate.ToShortDateString(),
                LockoutTime = null,
                AllRoles = ""
            };
            userManager.Create(user);
            
            //act
            var actual = new AdminDataModel().GetUserForShowByEmail(userManager, user.Email);

            //assert
            Assert.IsTrue(Equal(expected, actual));
        }

        private bool Equal(ShowUserViewModel expected, ShowUserViewModel actual)
        {
            if(expected.FullName == actual.FullName &&
                expected.Email == actual.Email &&
                expected.EmploymentDate == actual.EmploymentDate &&
                expected.LockoutTime == actual.LockoutTime &&
                expected.AllRoles == actual.AllRoles)
            {
                return true;
            }
            return false;
        }

        [TestCleanup]
        public void TearDown()
        {
            context.Dispose();
        }
    }
}
