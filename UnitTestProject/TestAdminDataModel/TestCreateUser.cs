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
    public class TestCreateUser
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
        public void CreateUser_CreateCorrectTestUser_UserExist()
        {
            //arrange        
            int expected = 1;          
            var store = new UserStore<ApplicationUser>(context);
            var userManager = new ApplicationUserManager(store);

            CreateUserViewModel user = new CreateUserViewModel
            {
                Email = "Test1@gmail.com",
                FullName = "Test",
                EmploymentDate = new DateTime(2019, 03, 12),
                Password = "123456-Pass"
            };

            new AdminDataModel().CreateUser(userManager, user);

            //act
            var actual = context.Users.Where(e => e.Email == user.Email).ToList();

            //assert
            Assert.AreEqual(expected, actual.Count);
        }


        [TestMethod]
        public void CreateUser_CreateCorrectTestUser_IdentityResultSucceeded()
        {
            //arrange
            var store = new UserStore<ApplicationUser>(context);
            var userManager = new ApplicationUserManager(store);

            CreateUserViewModel user = new CreateUserViewModel
            {
                Email = "CorrectTestUser@gmail.com",
                FullName = "CorrectTestUser",
                EmploymentDate = new DateTime(2019, 03, 12),
                Password = "123456-Pass"
            };

            //act
            var actual = new AdminDataModel().CreateUser(userManager, user);
          
            //assert
            Assert.IsTrue(actual.Succeeded);
        }

        [TestMethod]
        public void CreateUser_CreateTestUserWithInvaliPassword_UserIsNotExist()
        {
            var store = new UserStore<ApplicationUser>(context);
            var userManager = new ApplicationUserManager(store);

            CreateUserViewModel user = new CreateUserViewModel
            {
                Email = "UserWithInvaliPassword@gmail.com",
                FullName = "UserWithInvaliPassword",
                EmploymentDate = new DateTime(2019, 03, 12),
                Password = "icor"
            };

            new AdminDataModel().CreateUser(userManager, user);

            var actual = context.Users.Where(e => e.Email == user.Email).FirstOrDefault();

            Assert.IsNull(actual);
        }


        [ClassCleanup]
        public static void TearDown()
        {
            context.Dispose();
        }
    }
}
