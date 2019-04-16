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
using TimeOffTracker.Business;

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
            using (var db = new ApplicationDbContext())
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
                int days = (int)(createRequestViewModel.VacationRequest.DateEnd - createRequestViewModel.VacationRequest.DateStart).TotalDays + 1;
            
                int userVacationDays = db.UserVacationDays.Where(v => v.User.Id == currentUser.Id && v.VacationType.Id == createRequestViewModel.VacationRequest.VacationTypesId).Single().VacationDays;

                if (days > userVacationDays)
                {
                    Response.StatusCode = 500;

                    return new JsonResult
                    {
                        Data = new { message = "You choose wrong vacation days" },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
         
                db.Requests.Add(createRequestViewModel.VacationRequest);
                db.SaveChanges();

                var status = db.RequestStatuses.Where(s => s.Name == "Waiting").Single();
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



        //Часть от меня
        public ActionResult UserHistoryPanel()
        {
            var history = new ListHistoryOfVacationViewModel();
            ApplicationUser user = UserManager.FindByEmail(User.Identity.GetUserName());

            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                var UserVacationDays = context.UserVacationDays.Where(x => x.User.Id == user.Id).Select(p=>p).ToList();
                history.UserVacationDays = UserVacationDays.Select(p => new UserVacationDays
                {
                    Id = p.Id,
                    User = p.User,
                    LastUpdate = p.LastUpdate,
                    VacationDays = p.VacationDays,
                    VacationType = p.VacationType
                }).ToList();



                var request = (from r in context.Requests
                               where r.Employee.Id == user.Id
                               orderby r.DateEnd
                               select new
                               {
                                   r.Id,
                                   r.VacationTypes,
                                   r.Description,                                   
                                   r.DateStart,                                   
                                   r.DateEnd,                                   
                                   Reason = (from appr in context.RequestChecks
                                             where appr.Request == r
                                             select appr.Reason).ToList(),
                                   Approvers = (from appr in context.RequestChecks
                                             where appr.Request == r
                                             select appr).ToList()                             
                               }).ToList();

                history.AllVacations = request.Select(p => new HistoryOfVacationViewModel
                {
                    Id = p.Id,
                    VacationType = p.VacationTypes.Name,
                    Description = p.Description,
                    DateStart = p.DateStart,
                    DateEnd = p.DateEnd,
                    Days = ((p.DateEnd - p.DateStart).Days + 1).ToString(),
                    Approvers = p.Approvers.Select(x => new RequestChecks
                    {
                         Id = x.Id
                            , Approver = x.Approver
                            , Priority = x.Priority
                            , Status = x.Status
                            , Reason = x.Reason
                            , Request = x.Request
                    }).ToList(),
                    Reson = string.Join("", p.Reason)
                }).ToList();

                return View(history);             
            }
        }
    }
}