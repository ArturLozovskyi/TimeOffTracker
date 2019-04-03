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
                ListShowUsersInfo model = new ListShowUsersInfo();

                var userList = (from user in context.Users
                                     select new
                                     {
                                         UserId = user.Id,
                                         FullName = user.FullName,
                                         user.Email,
                                         user.dateCreateAccount,

                                         RoleNames = (from userRole in user.Roles 
                                                      join role in context.Roles 
                                                      on userRole.RoleId
                                                      equals role.Id
                                                      select role.Name).ToList()
                                     }).ToList();

                model.MenuItems = userList.Select(p => new ShowUsersInfo
                {
                    Id = p.UserId,
                    FullName = p.FullName,
                    Email = p.Email,
                    AllRoles = string.Join(", ", p.RoleNames),
                    DateCreate = p.dateCreateAccount.ToShortDateString()
                }).ToList();

                return View(model);
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult CreateUser()
        {
            CreateUserModel model = new CreateUserModel
            {
                AvailableRoles = GetSelectListRoles()
            };
            return View(model);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateUser(CreateUserModel model)
        {
            model.AvailableRoles = GetSelectListRoles();

            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.FullName, daysVacationInYear = 28, dateCreateAccount = DateTime.Now.Date};
                IdentityResult result = await UserManager.CreateAsync(user, model.Password);


                if (result.Succeeded)
                {
                    foreach (string role in model.SelectedRoles)
                    {
                        result = UserManager.AddToRole(user.Id, role);
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
        private IList<SelectListItem> GetSelectListRoles()
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

        [HttpPost]
        //[ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult ConfirmDeleteUser(ShowUsersInfo model)
        {
            if (ModelState.IsValid)
            {
                using (ApplicationDbContext context = new ApplicationDbContext())
                {
                    ShowUsersInfo modelToConfirm = new ShowUsersInfo();

                    var userList = (from user in context.Users where user.Id == model.Id
                                    select new
                                    {
                                        UserId = user.Id,
                                        FullName = user.FullName,
                                        user.Email,
                                        user.dateCreateAccount,

                                        RoleNames = (from userRole in user.Roles 
                                                     join role in context.Roles 
                                                     on userRole.RoleId
                                                     equals role.Id
                                                     select role.Name).ToList()
                                    }
                                    ).ToList();

                    modelToConfirm = userList.Select(p => new ShowUsersInfo
                    {
                        Id = p.UserId,
                        FullName = p.FullName,
                        Email = p.Email,
                        AllRoles = string.Join(", ", p.RoleNames),
                        DateCreate = p.dateCreateAccount.ToShortDateString()
                    }).First();

                    return View(modelToConfirm);
                }
            }
            return View(model);

        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteUser(ShowUsersInfo model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await UserManager.FindByNameAsync(model.Email);

                var rolesUser = await UserManager.GetRolesAsync(user.Id);

                if (rolesUser.Count() > 0)
                {
                    foreach (var item in rolesUser.ToList())
                    {

                        var result = await UserManager.RemoveFromRoleAsync(user.Id, item);
                    }
                }

                await UserManager.DeleteAsync(user);
            }
            else
            {
                return View(model);
            }
            return RedirectToAction("AdminUsersPanel");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult ConfirmEditUser(ShowUsersInfo model)
        {
            if (ModelState.IsValid)
            {
                using (ApplicationDbContext context = new ApplicationDbContext())
                {
                    EditUserModel modelToConfirmEdit = new EditUserModel();

                    var userList = (from user in context.Users
                                    where user.Id == model.Id
                                    select new
                                    {
                                        FullName = user.FullName,
                                        user.Email,
                                        

                                        RolesName = (from userRole in user.Roles 
                                                     join role in context.Roles
                                                     on userRole.RoleId
                                                     equals role.Id
                                                     select role.Name).ToList()
                                    }
                                    ).ToList();

                    modelToConfirmEdit = userList.Select(p => new EditUserModel
                    {
                        OldFullName = p.FullName,
                        OldEmail = p.Email,
                        OldRoles = string.Join(", ", p.RolesName),                       
                    }).First();

                    modelToConfirmEdit.AvailableRoles = GetSelectListRoles();

                    return View(modelToConfirmEdit);
                }

            }
            return View(model);
        }

        //[HttpPost]
        //[Authorize(Roles = "Admin")]
        //public async Task<ActionResult> ConfirmEditUser(ShowUsersInfo model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        //if (model.Id == null)
        //        //{
        //        //    return RedirectToAction("AdminUsersPanel");
        //        //}
        //        //using (ApplicationDbContext context = new ApplicationDbContext())
        //        //{

        //        //}
        //        //var user = await UserManager.FindByNameAsync(model.Email);
        //        //if (!String.IsNullOrWhiteSpace(model.Email))
        //        //{
        //        //    return RedirectToAction("CreateUser");
        //        //}
        //        ApplicationUser user = await UserManager.FindByNameAsync(model.Email);

        //        var rolesUser = await UserManager.GetRolesAsync(user.Id);
        //        //var rolesUser = user.Roles;
        //        if (rolesUser.Count() > 0)
        //        {
        //            foreach (var item in rolesUser.ToList())
        //            {

        //                var result = await UserManager.RemoveFromRoleAsync(user.Id, item);
        //            }
        //        }

        //        await UserManager.DeleteAsync(user);
        //    }
        //    else
        //    {
        //        return View(model);
        //    }
        //    return RedirectToAction("AdminUsersPanel");
        //}


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

        //private ApplicationRoleManager RoleManager
        //{
        //    get
        //    {
        //        return HttpContext.GetOwinContext().GetUserManager<ApplicationRoleManager>();
        //    }
        //}
    }
}