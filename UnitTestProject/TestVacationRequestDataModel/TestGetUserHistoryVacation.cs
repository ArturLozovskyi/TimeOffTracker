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
    public class TestGetUserHistoryVacation
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
        public void GetUserHistoryVacation_UserIdIsNull_IsNull()
        {
            //arrange
            var store = new UserStore<ApplicationUser>(context);
            var userManager = new ApplicationUserManager(store);         

            //act
            var actual = new VacationRequestDataModel(null).GetUserHistoryVacation(userManager, null);

            //assert
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void GetUserHistoryVacation_TestHistoryInitializel_IsNull()
        {
            //arrange
            var store = new UserStore<ApplicationUser>(context);
            var userManager = new ApplicationUserManager(store);
            var expected = TestHistoryInitialize(context, "user");
            var user = context.Users.Where(x => x.Email == "user@gmail.com").First();

            //act
            var actual = new VacationRequestDataModel(null).GetUserHistoryVacation(userManager, user.Id);

            //assert
            Assert.IsTrue(Equal(expected, actual));
        }

        private bool Equal(ListHistoryOfVacationViewModel expected, ListHistoryOfVacationViewModel actual)
        {
            if(expected.UserVacationDays.Count != actual.UserVacationDays.Count)
            {
                return false;
            }
            if (expected.AllVacations.Count != actual.AllVacations.Count)
            {
                return false;
            }
            for(int i = 0; i < expected.UserVacationDays.Count; i++)
            {
                if(expected.UserVacationDays[i].LastUpdate != actual.UserVacationDays[i].LastUpdate ||
                    expected.UserVacationDays[i].User.Email != actual.UserVacationDays[i].User.Email ||
                    expected.UserVacationDays[i].VacationDays != actual.UserVacationDays[i].VacationDays ||
                    expected.UserVacationDays[i].VacationType.Name != actual.UserVacationDays[i].VacationType.Name
                    )
                {
                    return false;
                }
            }

            for (int i = 0; i < expected.AllVacations.Count; i++)
            {
                if (expected.AllVacations[i].DateStart != actual.AllVacations[i].DateStart ||
                    expected.AllVacations[i].DateEnd != actual.AllVacations[i].DateEnd ||
                    expected.AllVacations[i].Days != actual.AllVacations[i].Days ||
                    expected.AllVacations[i].Description != actual.AllVacations[i].Description ||
                    expected.AllVacations[i].Approvers.Count != actual.AllVacations[i].Approvers.Count ||
                    expected.AllVacations[i].Reson != actual.AllVacations[i].Reson ||
                    expected.AllVacations[i].VacationType != actual.AllVacations[i].VacationType
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

        private ListHistoryOfVacationViewModel TestHistoryInitialize(ApplicationDbContext context, string userName/*ApplicationUser manager*/)
        {
            ListHistoryOfVacationViewModel result = new ListHistoryOfVacationViewModel() { AllVacations = new List<HistoryOfVacationViewModel>(), UserVacationDays = new List<UserVacationDays>() };

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

            //context.Users.Add(old);
            if (userManager.Create(user, "123456-Pass").Succeeded)
            {
                userManager.AddToRole(user.Id, "Employee");
            }

            ApplicationUser manager = new ApplicationUser
            {
                UserName = "Manager@gmail.com",
                Email = "Manager@gmail.com",
                FullName = "Manager",
                EmploymentDate = DateTime.Now.Date
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

            result.UserVacationDays.Add(userVacDays);

            RequestStatuses testStatus = new RequestStatuses
            {
                Name = "TestStatus"
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

                result.AllVacations.Add(new HistoryOfVacationViewModel()
                {
                    Approvers = new List<RequestChecks>() { requestChecks },
                    DateStart = request.DateStart,
                    DateEnd = request.DateEnd,
                    Days = ((request.DateEnd - request.DateStart).Days + 1).ToString(),
                    Description = request.Description,
                    Reson = "",
                    VacationType = testVacationType.Name,                 
                });
            }
            context.SaveChanges();
            return result;
        }
    }
}
