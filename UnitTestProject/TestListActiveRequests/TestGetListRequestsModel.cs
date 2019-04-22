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
    public class TestGetListRequestsModel
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
        public void GetListRequestsModel_UserIdIsNull_IsEmtpty()
        {
            //arrange
            int expected = 0;

            //act
            var actual = new ListActiveRequests().GetListRequestsModel(null);

            //assert
            Assert.AreEqual(expected, actual.Items.Count);
        }

        [TestMethod]
        public void GetListRequestsModel_TestRequestsInitialize_Equal()
        {
            //arrange
            var store = new UserStore<ApplicationUser>(context);
            var userManager = new ApplicationUserManager(store);
            var expected = TestRequestsInitialize(context, "manager");
            var user = context.Users.Where(x => x.Email == "manager@gmail.com").First();

            //act
            var actual = new ListActiveRequests().GetListRequestsModel(user.Id);

            //assert
            Assert.IsTrue(Equal(expected, actual));
        }

        private bool Equal(ListRequestsModel expected, ListRequestsModel actual)
        {
            if (expected.Items.Count != actual.Items.Count)
            {
                return false;
            }
         
            for (int i = 0; i < expected.Items.Count; i++)
            {
                if (expected.Items[i].EmailEmployee != actual.Items[i].EmailEmployee ||
                    expected.Items[i].FullNameEmployee != actual.Items[i].FullNameEmployee ||
                    expected.Items[i].Reason != actual.Items[i].Reason ||
                    expected.Items[i].VacationType != actual.Items[i].VacationType ||
                    expected.Items[i].Description != actual.Items[i].Description ||
                    expected.Items[i].DateStart != actual.Items[i].DateStart ||
                    expected.Items[i].DateEnd != actual.Items[i].DateEnd
                    )
                {
                    return false;
                }
            }
                     
            return true;
        }

        [ClassCleanup]
        public static void TearDown()
        {
            context.Dispose();
        }

        private ListRequestsModel TestRequestsInitialize(ApplicationDbContext context, string managerName/*ApplicationUser manager*/)
        {
            ListRequestsModel result = new ListRequestsModel() { Items = new List<RequestsModel>()};

            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var roleresult = RoleManager.Create(new IdentityRole("Employee"));
            roleresult = RoleManager.Create(new IdentityRole("Manager"));
            roleresult = RoleManager.Create(new IdentityRole("Admin"));

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

            VacationTypes testVacationType = new VacationTypes()
            {
                Name = "TestVacationType",
                MaxDays = 30
            };
            context.VacationTypes.Add(testVacationType);

            UserVacationDays userVacDays = new UserVacationDays()
            {
                User = user,
                VacationDays = testVacationType.MaxDays,
                VacationType = testVacationType,
                LastUpdate = user.EmploymentDate,
            };
            context.UserVacationDays.Add(userVacDays);


            RequestStatuses testStatus = new RequestStatuses
            {
                Name = "Waiting"
            };

            for (int i = 1; i < 6; i++)
            {
                Requests request = new Requests()
                {
                    Employee = user,
                    VacationTypes = testVacationType,
                    DateStart = user.EmploymentDate.AddDays(i),
                    DateEnd = user.EmploymentDate.AddDays(i + 2),
                    Description = i.ToString()
                };
                context.Requests.Add(request);


                RequestChecks requestChecks = new RequestChecks()
                {
                    Request = request,
                    Priority = 1,
                    Status = testStatus,
                    Approver = manager
                };
                context.RequestChecks.Add(requestChecks);

                result.Items.Add(new RequestsModel()
                {
                    EmailEmployee = user.Email,
                    FullNameEmployee = user.FullName,
                    Reason = null,
                    DateStart = request.DateStart,
                    DateEnd = request.DateEnd,                  
                    Description = request.Description,                
                    VacationType = testVacationType.Name
                });
            }
            context.SaveChanges();
            return result;
        }
    }
}
