using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using TimeOffTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace TimeOffTracker.Controllers
{
    [Authorize]
    public class VacationRequestController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public VacationRequestController()
        {
            
        }

        public VacationRequestController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
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
            using(var db = new ApplicationDbContext())
            {
                var currentUser = UserManager.FindById(User.Identity.GetUserId());
                if (currentUser == null)
                {
                    return View("Error");

                }

                var approvers = db.Roles.Where(r => r.Name == "Manager").First().Users.Join(db.Users, role => role.UserId, u => u.Id, (role, u) => new ApplicationUser()
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email
                }).ToList();


                VacationRequestViewModel vacationRequestViewModel = new VacationRequestViewModel();
                List<VacationTypes> vacationTypes = db.VacationTypes.ToList();
                vacationRequestViewModel.User = currentUser;
                vacationRequestViewModel.VacationTypes = vacationTypes;
                vacationRequestViewModel.Approvers = approvers;
                return View(vacationRequestViewModel);
            }      
        }

        [HttpPost]
        public ActionResult CreateVacationRequest(CreateVacationRequestViewModel createRequestViewModel)
        {
            var currentUser = UserManager.FindById(User.Identity.GetUserId());
            if (currentUser == null)
            {
                return View("Error");
            }
            if (!ModelState.IsValid)
            {
                return View("Error");
            }

            createRequestViewModel.VacationRequest.EmployeeId = currentUser.Id;

            using (var db = new ApplicationDbContext())
            {
                db.Requests.Add(createRequestViewModel.VacationRequest);
                db.SaveChanges();

                var status = db.RequestStatuses.Where(s => s.Name == "Ожидание").Single();
                int priority = 1;
                var approvers = createRequestViewModel.ApproversId;
                foreach (var approver in approvers)
                {
                    RequestChecks vacationRequestCheck = new RequestChecks()
                    {
                        RequestId = createRequestViewModel.VacationRequest.Id,
                        ApproverId = approver,
                        Priority = priority,
                        StatusId = status.Id
                    };
                    db.RequestChecks.Add(vacationRequestCheck);
                    db.SaveChanges();
                    priority++;
                }
            }
            return Json(createRequestViewModel, JsonRequestBehavior.AllowGet);
        }
    }
}