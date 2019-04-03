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


namespace TimeOffTracker.Controllers
{
      public class AdminController : Controller
    {
        [Authorize(Roles = "Admin")]
        public ActionResult AdminUsersPanel()
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                ListShowUserViewModel model = new ListShowUserViewModel();

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

                model.MenuItems = userList.Select(p => new ShowUserViewModel
                {
                    FullName = p.FullName
                    , Email = p.Email
                    , LockoutTime = p.LockoutEndDateUtc
                    , AllRoles = string.Join(", ", p.RoleNames)
                    , EmploymentDate = p.EmploymentDate.ToShortDateString()
                }).ToList();

                return View(model);
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult CreateUser()
        {
            CreateUserViewModel model = new CreateUserViewModel
            {
                AvailableRoles = GetSelectListItemRoles()
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateUser(CreateUserViewModel model)
        {
            model.AvailableRoles = GetSelectListItemRoles(model.SelectedRoles);

            if (ModelState.IsValid)
            {
                if(model.EmploymentDate > DateTime.Now)
                {
                    ModelState.AddModelError("", "Дата приема на работу не может быть больше текущей даты");
                    return View(model);
                }

                ListRequestModel user = new ListRequestModel
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    EmploymentDate = model.EmploymentDate
                };

                IdentityResult result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (model.SelectedRoles != null)
                    {
                        foreach (string role in model.SelectedRoles)
                        {
                            result = UserManager.AddToRole(user.Id, role);
                        }
                    }
                    if (result.Succeeded)
                    {
                        return RedirectToAction("AdminUsersPanel");
                    }
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
                using (ApplicationDbContext context = new ApplicationDbContext())
                {
                    ShowUserViewModel modelToConfirm;

                    var user = UserManager.FindByEmail(email);
                    var userRoles = UserManager.GetRoles(user.Id);

                    modelToConfirm = new ShowUserViewModel
                    {
                        FullName = user.FullName,
                        Email = user.Email,
                        LockoutTime = user.LockoutEndDateUtc,
                        AllRoles = string.Join(", ", userRoles),
                        EmploymentDate = user.EmploymentDate.ToShortDateString()
                    };

                    return View(modelToConfirm);
                }
            }
            return RedirectToAction("AdminUsersPanel");

        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> SwitchLockoutUser(string email)
        {
            if (ModelState.IsValid && !string.IsNullOrWhiteSpace(email))
            {
                var user = UserManager.FindByEmail(email);

                if (UserManager.FindById(user.Id).LockoutEndDateUtc == null)
                {
                    await UserManager.SetLockoutEndDateAsync(user.Id, DateTime.Now.AddYears(1000));
                }
                else
                {
                    await UserManager.SetLockoutEndDateAsync(user.Id, DateTimeOffset.MinValue);
                }              
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
                using (ApplicationDbContext context = new ApplicationDbContext())
                {
                    EditUserViewModel modelToConfirmEdit;

                    var user = UserManager.FindByEmail(email);
                    var userRoles = UserManager.GetRoles(user.Id);

                    modelToConfirmEdit = new EditUserViewModel
                    {
                        OldFullName = user.FullName
                        , NewFullName = user.FullName
                        , OldEmail = user.Email
                        , NewEmail = user.Email
                        , OldEmploymentDate = user.EmploymentDate.ToShortDateString()
                        , NewEmploymentDate = user.EmploymentDate
                        , OldRoles = string.Join(", ", userRoles)
                        , AvailableRoles = GetSelectListItemRoles(userRoles)
                    };

                    return View(modelToConfirmEdit);
                }

            }
            return RedirectToAction("AdminUsersPanel");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> EditUser(EditUserViewModel model)
        {
            model.AvailableRoles = GetSelectListItemRoles(model.SelectedRoles);

            if (ModelState.IsValid)
            {
                if (model.NewEmploymentDate > DateTime.Now)
                {
                    ModelState.AddModelError("", "Дата приема на работу не может быть больше текущей даты");
                    return View(model);
                }
                ListRequestModel user = await UserManager.FindByEmailAsync(model.OldEmail);
                var rolesUser = await UserManager.GetRolesAsync(user.Id);

                IdentityResult result;
                if (rolesUser.Count() > 0)
                {
                    //Удалаем все старые роли перед обновлением
                    foreach (var item in rolesUser.ToList())
                    {
                        result = await UserManager.RemoveFromRoleAsync(user.Id, item);
                    }
                }

                user.Email = model.NewEmail;
                user.UserName = model.NewEmail;
                user.FullName = model.NewFullName;
                user.EmploymentDate = model.NewEmploymentDate;

                result = await UserManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    foreach (string role in model.SelectedRoles)
                    {
                        if (model.SelectedRoles != null)
                        {
                            result = UserManager.AddToRole(user.Id, role);
                        }
                    }
                    if (result.Succeeded)
                    {
                        return RedirectToAction("AdminUsersPanel");
                    }
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
                using (ApplicationDbContext context = new ApplicationDbContext())
                {
                    ChangeUserPasswordViewModel modelToConfirmChangePassword;

                    var user = UserManager.FindByEmail(email);
                    var userRoles = UserManager.GetRoles(user.Id);

                    modelToConfirmChangePassword = new ChangeUserPasswordViewModel
                    {
                        FullName = user.FullName
                        , Email = user.Email
                        , EmploymentDate = user.EmploymentDate.ToShortDateString()
                        , AllRoles = string.Join(", ", userRoles)
                    };                 

                    return View(modelToConfirmChangePassword);
                }

            }
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ChangeUserPassword(ChangeUserPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                ListRequestModel user = await UserManager.FindByEmailAsync(model.Email);
                IdentityResult result;
                result = await UserManager.PasswordValidator.ValidateAsync(model.NewPassword);
                
                if (result.Succeeded)
                {
                    string token = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                    result = await UserManager.ResetPasswordAsync(user.Id, token, model.NewPassword);
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

        [Authorize(Roles = "Admin")]
        private IList<SelectListItem> GetSelectListItemRoles()
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                List<SelectListItem> result = new List<SelectListItem>();
                foreach (IdentityRole role in context.Roles)
                {
                    result.Add(new SelectListItem { Text = role.Name, Value = role.Name });
                }
                return result;
            }
        }

        [Authorize(Roles = "Admin")]
        private IList<SelectListItem> GetSelectListItemRoles(IList<string> roles)
        {
            IList<SelectListItem> result = GetSelectListItemRoles();
            for (int i = 0; i < result.Count; i++)
            {
                foreach (string str in roles)
                {
                    if (result[i].Value == str)
                    {
                        result[i].Selected = true;
                        break;
                    }

                }
            }
            return result;
        }

    }
}