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
    public class TestEditUser
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
        public void CreateUser_UserEmailIsNull_IdentityResultSucceeded()
        {
            //arrange        
            var store = new UserStore<ApplicationUser>(context);
            var userManager = new ApplicationUserManager(store);

            var userEdited = new EditUserViewModel()
            {
                OldEmail = null,
            };

            //act
            var actual = new AdminDataModel().EditUser(userManager, userEdited);

            //assert
            Assert.IsFalse(actual.Succeeded);
        }

        [TestMethod]
        public void CreateUser_CreateAndEditCorrectTestUser_IdentityResultSucceeded()
        {
            //arrange        
            var store = new UserStore<ApplicationUser>(context);
            var userManager = new ApplicationUserManager(store);

            ApplicationUser user = new ApplicationUser
            {
                UserName = "CreateAndEdit@gmail.com",
                Email = "CreateAndEdit@gmail.com",
                FullName = "CreateAndEdit",
                EmploymentDate = new DateTime(2019, 03, 12),
            };

            context.Users.Add(user);
            context.SaveChanges();

            var userEdited = new EditUserViewModel()
            {
                OldEmail = user.Email,
                NewEmail = "EditedUser@gmail.com",
                OldFullName = user.FullName,
                NewFullName = "EditedUse",
                OldEmploymentDate = user.EmploymentDate.ToShortDateString(),
                NewEmploymentDate = user.EmploymentDate.AddMonths(-1),
                OldRoles = "",
                AvailableRoles = new List<SelectListItem>(),
            };

            //act
            var actual = new AdminDataModel().EditUser(userManager, userEdited);

            //assert
            Assert.IsTrue(actual.Succeeded);
        }

        [TestMethod]
        public void EditUser_CreateAndEditCorrectTestUser_UserEdited()
        {
            //arrange        
            var store = new UserStore<ApplicationUser>(context);
            var userManager = new ApplicationUserManager(store);

            ApplicationUser user = new ApplicationUser
            {
                UserName = "CreateAndEdit@gmail.com",
                Email = "CreateAndEdit@gmail.com",
                FullName = "CreateAndEdit",
                EmploymentDate = new DateTime(2019, 03, 12),
            };

            context.Users.Add(user);
            context.SaveChanges();

            var userEdited = new EditUserViewModel()
            {
                OldEmail = user.Email,
                NewEmail ="EditedUser@gmail.com",
                OldFullName = user.FullName,
                NewFullName = "EditedUse",
                OldEmploymentDate = user.EmploymentDate.ToShortDateString(),
                NewEmploymentDate = user.EmploymentDate.AddMonths(-1),
                OldRoles = "",
                AvailableRoles = new List<SelectListItem>()
            };

            var expected = new ApplicationUser
            {
                Email = userEdited.NewEmail,
                FullName = userEdited.NewFullName,
                EmploymentDate = userEdited.NewEmploymentDate
            };

            //act
            new AdminDataModel().EditUser(userManager, userEdited);
            var actual = context.Users.Where(e => e.Email == userEdited.NewEmail).First();

            //assert
            Assert.IsTrue(Equal(expected, actual));
        }      

        private bool Equal(ApplicationUser expected, ApplicationUser actual)
        {
            if (expected.FullName == actual.FullName &&
                expected.Email == actual.Email &&
                expected.EmploymentDate == actual.EmploymentDate
                )
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
