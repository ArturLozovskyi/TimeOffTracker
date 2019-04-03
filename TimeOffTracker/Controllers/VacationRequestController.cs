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

        public async Task<ActionResult> VacationRequest()
        {
            var currentUser = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (currentUser == null)
            {
                return View("Error");

            }
            var approvers = await (
                from user in db.Users
                join userRole in db.UserRoles on user.Id equals userRole.UserId
                join roles in db.Roles on userRole.RoleId equals roles.Id
                where roles.Name.Contains("Admin") || roles.Name.Contains("Approver")
                select new ApplicationUser()
                {
                    Id = currentUser.Id,
                    UserName = currentUser.UserName
                }
            ).ToListAsync();
            
            VacationRequestViewModel vacationRequestViewModel = new VacationRequestViewModel();
            List<VacationTypes> vacationTypes = await db.VacationTypes.ToListAsync();
            vacationRequestViewModel.User = currentUser;
            vacationRequestViewModel.VacationTypes = vacationTypes;
            vacationRequestViewModel.Approvers = approvers;
            return View(vacationRequestViewModel);
        }

        [HttpPost]
        public async Task<ActionResult> CreateVacationRequest([FromBody]CreateVacationRequestViewModel createRequestViewModel)
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