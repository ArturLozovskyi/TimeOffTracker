using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using TimeOffTracker.Models;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using System;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Linq;
using System.Collections.Generic;
using TimeOffTracker.Business;

namespace TimeOffTracker.Controllers
{
    public class AdminController : Controller
    {
        IAdminDataModel _adminDataModel;

        public AdminController(IAdminDataModel adminDataModel)
        {
            _adminDataModel = adminDataModel;
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AdminUsersPanel()
        {
            return View(_adminDataModel.GetAllUsersForShow());
        }



        [Authorize(Roles = "Admin")]
        public ActionResult CreateUser()
        {
            CreateUserViewModel model = new CreateUserViewModel
            {
                AvailableRoles = _adminDataModel.GetSelectListItemRoles()
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult CreateUser(CreateUserViewModel model)
        {
            model.AvailableRoles = _adminDataModel.GetSelectListItemRoles(model.SelectedRoles);

            if (ModelState.IsValid)
            {
                if (model.EmploymentDate > DateTime.Now)
                {
                    ModelState.AddModelError("", "Дата приема на работу не может быть больше текущей даты");
                    return View(model);
                }

                IdentityResult result = _adminDataModel.CreateUser(UserManager, model);

                if (result.Succeeded)
                {
                    return RedirectToAction("AdminUsersPanel");
                }
                else
                {
                    AddErrorsFromResult(result);
                }
            }
            return View(model);
        }



        [Authorize(Roles = "Admin")]
        public ActionResult ConfirmSwitchLockoutUser(string email)
        {
            if (ModelState.IsValid && !string.IsNullOrWhiteSpace(email))
            {
                return View(_adminDataModel.GetUserForShowByEmail(UserManager, email));
            }
            return RedirectToAction("AdminUsersPanel");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult SwitchLockoutUser(string email)
        {
            if (ModelState.IsValid && !string.IsNullOrWhiteSpace(email))
            {
                _adminDataModel.SwitchLockoutUserByEmail(UserManager, email);
            }
            else
            {
                return RedirectToAction("AdminUsersPanel");
            }
            return RedirectToAction("AdminUsersPanel");
        }



        [Authorize(Roles = "Admin")]
        public ActionResult EditUser(string email)
        {
            if (ModelState.IsValid && !string.IsNullOrWhiteSpace(email))
            {
                return View(_adminDataModel.GetUserForEditByEmail(UserManager, email));
            }
            return RedirectToAction("AdminUsersPanel");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult EditUser(EditUserViewModel model)
        {
            model.AvailableRoles = _adminDataModel.GetSelectListItemRoles(model.SelectedRoles);

            if (ModelState.IsValid)
            {
                if (model.NewEmploymentDate > DateTime.Now)
                {
                    ModelState.AddModelError("", "Дата приема на работу не может быть больше текущей даты");
                    return View(model);
                }
                IdentityResult result = _adminDataModel.EditUser(UserManager, model);

                if (result.Succeeded)
                {
                    return RedirectToAction("AdminUsersPanel");
                }
                else
                {
                    AddErrorsFromResult(result);
                }
            }
            return View(model);
        }



        [Authorize(Roles = "Admin")]
        public ActionResult ChangeUserPassword(string email)
        {
            if (ModelState.IsValid && !string.IsNullOrWhiteSpace(email))
            {
                return View(_adminDataModel.GetUserForChangePasswordByEmail(UserManager, email));
            }
            return RedirectToAction("AdminUsersPanel");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult ChangeUserPassword(ChangeUserPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = _adminDataModel.ChangeUserPassword(UserManager, model);

                if (result.Succeeded)
                {

                    return RedirectToAction("AdminUsersPanel");
                }
                else
                {
                    AddErrorsFromResult(result);
                }
            }
            return View(model);
        }



        [Authorize(Roles = "Admin")]
        private void AddErrorsFromResult(IdentityResult result)
        {
            foreach (string error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

    }
}