using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using TimeOffTracker.Models;

namespace TimeOffTracker.Controllers
{
    [Authorize]
    public class VacationRequestController : Controller
    {
        private ApplicationDbContext db;
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public VacationRequestController()
        {
            
        }

        public VacationRequestController(ApplicationDbContext context, ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            db = context;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ActionResult VacationRequest()
        {
            var currentUser = UserManager.FindById(User.Identity.GetUserId());
            if (currentUser == null)
            {
                return View("Error");
            }

            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                ListShowUserViewModel allUsers = new ListShowUserViewModel();

                var userList = (from user in context.Users
                                orderby user.LockoutEndDateUtc
                                select new
                                {
                                    user.FullName,
                                    user.Email,
                                    user.EmploymentDate,
                                    user.LockoutEndDateUtc,
                                    RoleNames = (from userRole in user.Roles
                                                 join role in context.Roles
                                                 on userRole.RoleId
                                                 equals role.Id
                                                 select role.Name).ToList()

                                }).ToList();


                var approvers = userList.Where(w => w.RoleNames.Contains("Manager"));

                VacationRequestViewModel vacationRequestViewModel = new VacationRequestViewModel();
                List<VacationTypes> vacationTypes = context.VacationTypes.ToList();
                vacationRequestViewModel.User = currentUser;
                vacationRequestViewModel.VacationTypes = vacationTypes;
                vacationRequestViewModel.Approvers = approvers as List<ApplicationUser>;

                //return Json(approvers);
                return View(vacationRequestViewModel);

                /*
                using (var context = new ApplicationDbContext())
                {
                    var approvers = (
                    from user in context.Users
                    select new ApplicationUser()
                    {
                        Id = user.Id,
                        UserName = user.UserName
                    }
                    ).ToList();

                    VacationRequestViewModel vacationRequestViewModel = new VacationRequestViewModel();
                    List<VacationTypes> vacationTypes = db.VacationTypes.ToList();
                    vacationRequestViewModel.User = currentUser;
                    vacationRequestViewModel.VacationTypes = vacationTypes;
                    vacationRequestViewModel.Approvers = approvers;
                    return View(vacationRequestViewModel);
                }
                */

                /*

                    join userRole in db.Roles on user.Id equals userRole.Id
                    join roles in db.Roles on userRole.Id equals roles.Id
                    where roles.Name.Contains("Manager")
                 */
            }
        }

        [HttpPost]
        public async Task<ActionResult> CreateVacationRequest(CreateVacationRequestViewModel createRequestViewModel)
        {
            var currentUser = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (currentUser == null)
            {
                return View("Error");
            }
            if (!ModelState.IsValid)
            {
                return View("Error");
            }
            Requests vacationRequest = new Requests()
            {
                DateStart = createRequestViewModel.VacationRequest.DateStart,
                DateEnd = createRequestViewModel.VacationRequest.DateEnd,
                Description = createRequestViewModel.VacationRequest.Description,
                VacationTypes = createRequestViewModel.VacationRequest.VacationTypes,
                Employee = currentUser
            };
            db.Requests.Add(vacationRequest);
            await db.SaveChangesAsync();
            
            
            var approvers = createRequestViewModel.Approvers;
            foreach (var approver in approvers)
            {
                
                RequestChecks vacationRequestCheck = new RequestChecks()
                {
                    Request = vacationRequest,
                    Approver = approver
                };
                db.RequestChecks.Add(vacationRequestCheck);
                await db.SaveChangesAsync();
            }
            return Json(createRequestViewModel);
        }
    }
}