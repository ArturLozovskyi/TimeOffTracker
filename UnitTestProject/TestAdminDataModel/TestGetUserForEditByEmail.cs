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
    public class TestGetUserForEditByEmail
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
        public void GetUserForEditByEmail_UserEmailNull_Null()
        {
            //arrange
            var store = new UserStore<ApplicationUser>(context);
            var userManager = new ApplicationUserManager(store);

            //act
            var actual = new AdminDataModel().GetUserForEditByEmail(userManager, null);

            //assert
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void GetUserForEditByEmail_CreateAndGetTestUser_UserExist()
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

            var expected = new EditUserViewModel()
            {
                OldEmail = user.Email,
                NewEmail = user.Email,
                OldFullName = user.FullName,
                NewFullName = user.FullName,
                OldEmploymentDate = user.EmploymentDate.ToShortDateString(),
                NewEmploymentDate = user.EmploymentDate,
                OldRoles = "",
                AvailableRoles = new List<SelectListItem>(),
            };

            //act
            var actual = new AdminDataModel().GetUserForEditByEmail(userManager, user.Email);

            //assert
            Assert.IsTrue(Equal(expected, actual));
        }


        private bool Equal(EditUserViewModel expected, EditUserViewModel actual)
        {
            if (expected.NewFullName == actual.NewFullName &&
                expected.OldFullName == actual.OldFullName &&
                expected.NewEmail == actual.NewEmail &&
                expected.OldEmail == actual.OldEmail &&
                expected.NewEmploymentDate == actual.NewEmploymentDate &&
                expected.OldEmploymentDate == actual.OldEmploymentDate &&
                expected.OldRoles == actual.OldRoles &&
                expected.AvailableRoles.Count == actual.AvailableRoles.Count)
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

