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
    public class TestSwitchLockoutUserByEmail
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
        public void SwitchLockoutUserByEmail_CreateTestUserWithoutLockoutAndLock_UserLock()
        {
            //arrange
            var store = new UserStore<ApplicationUser>(context);
            var userManager = new ApplicationUserManager(store);
            ApplicationUser user = new ApplicationUser
            {
                UserName = "WithoutLockout@gmail.com",
                Email = "WithoutLockout@gmail.com",
                FullName = "WithoutLockout",
                EmploymentDate = new DateTime(2019, 03, 12),
                LockoutEnabled = true,
                LockoutEndDateUtc = null
            };
            context.Users.Add(user);
            context.SaveChanges();

            //act
            new AdminDataModel().SwitchLockoutUserByEmail(userManager, user.Email);
            var actual = context.Users.Where(x => x.Email == user.Email).First();

            //assert
            Assert.IsNotNull(actual.LockoutEndDateUtc);
        }

        [TestMethod]
        public void SwitchLockoutUserByEmail_CreateTestUserWithLockoutAndUnlock_UserUnlock()
        {
            //arrange
            var store = new UserStore<ApplicationUser>(context);
            var userManager = new ApplicationUserManager(store);
            ApplicationUser user = new ApplicationUser
            {
                UserName = "WithLockout@gmail.com",
                Email = "WithLockout@gmail.com",
                FullName = "WithLockout",
                EmploymentDate = new DateTime(2019, 03, 12),
                LockoutEnabled = true,
                LockoutEndDateUtc = DateTime.Now.AddYears(1000)
            };
            context.Users.Add(user);
            context.SaveChanges();

            //act
            new AdminDataModel().SwitchLockoutUserByEmail(userManager, user.Email);
            var actual = context.Users.Where(x => x.Email == user.Email).First();

            //assert
            Assert.IsNull(actual.LockoutEndDateUtc);
        }

        [TestMethod]
        public void SwitchLockoutUserByEmail_NullEmail_Nothing()
        {
            //arrange
            var store = new UserStore<ApplicationUser>(context);
            var userManager = new ApplicationUserManager(store);

            //act
            new AdminDataModel().SwitchLockoutUserByEmail(userManager, null);

            //assert
            Assert.IsTrue(true);
        }   

        [ClassCleanup]
        public static void TearDown()
        {
            context.Dispose();
        }
    }
}
