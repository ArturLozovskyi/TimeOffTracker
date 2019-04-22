using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.QualityTools.UnitTestFramework;
using System;
using System.Activities.Statements;
using System.Collections.Generic;

using TimeOffTracker;
using TimeOffTracker.Business;
using TimeOffTracker.Models;
using System.Data.Entity;

namespace UnitTestProject.TestAdminDataModel
{
    [TestClass]
    public class TestGetAllUsersForShow
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
        public void GetAllUsersForShow_EmptyDB_EmptyMenuItem()
        {
            //arrange
            int expected = 0;

            //act
            var actual = new AdminDataModel().GetAllUsersForShow();

            //assert
            Assert.AreEqual(expected, actual.MenuItems.Count);
        }

        [TestMethod]
        public void GetAllUsersForShow_AddOneUser_OneMenuItem()
        {
            //arrange
            int expected = 1;
            ApplicationUser user = new ApplicationUser
            {
                UserName = "Test@gmail.com",
                Email = "Test1@gmail.com",
                FullName = "Test",
                EmploymentDate = new DateTime(2019, 03, 12)
            };
            context.Users.Add(user);
            context.SaveChanges();

            //act
            var actual = new AdminDataModel().GetAllUsersForShow();

            //assert
            Assert.AreEqual(expected, actual.MenuItems.Count);
        }

        [TestMethod]
        public void GetAllUsersForShow_AddRangeFiveItems_FiveMenuItem()
        {
            //arrange
            int expected = 5;
            List<ApplicationUser> users = new List<ApplicationUser>();
            for(int i = 1; i < expected + 1; i++)
            {
                users.Add(new ApplicationUser
                {
                    UserName = "Test" + i + "@gmail.com",
                    Email = "Test" + i + "@gmail.com",
                    FullName = "Test" + i,
                    EmploymentDate = new DateTime(2019, 03, i)
                });
            }

            foreach(var user in users)
            {
                context.Users.Add(user);
            }
            context.SaveChanges();

            //act
            var actual = new AdminDataModel().GetAllUsersForShow();

            //assert
            Assert.AreEqual(expected, actual.MenuItems.Count);
        }


        [TestCleanup]
        public void TearDown()
        {
            context.Dispose();
        }
    }
}
