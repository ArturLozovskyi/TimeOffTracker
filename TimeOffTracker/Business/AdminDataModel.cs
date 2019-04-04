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


namespace TimeOffTracker.Business
{
    public class AdminDataModel : IAdminDataModel
    {

        public ListShowUserViewModel GetAllUsersForShow()
        {
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

                allUsers.MenuItems = userList.Select(p => new ShowUserViewModel
                {
                    FullName = p.FullName,
                    Email = p.Email,
                    LockoutTime = p.LockoutEndDateUtc,
                    AllRoles = string.Join(", ", p.RoleNames),
                    EmploymentDate = p.EmploymentDate.ToShortDateString()
                }).ToList();

                return allUsers;
            }
        }       

        public ShowUserViewModel GetUserForShowByEmail(ApplicationUserManager UserManager, string email)
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                var user = UserManager.FindByEmail(email);
                var userRoles = UserManager.GetRoles(user.Id);

                return new ShowUserViewModel
                    {
                        FullName = user.FullName
                        , Email = user.Email
                        , LockoutTime = user.LockoutEndDateUtc
                        , AllRoles = string.Join(", ", userRoles)
                        , EmploymentDate = user.EmploymentDate.ToShortDateString()
                    };
             }
        }



        public IdentityResult CreateUser(ApplicationUserManager UserManager, CreateUserViewModel model)
        {
            ApplicationUser user = new ApplicationUser
            {
                UserName = model.Email
                , Email = model.Email
                , FullName = model.FullName
                , EmploymentDate = model.EmploymentDate
            };

            IdentityResult result = UserManager.Create(user, model.Password);
            
            if (result.Succeeded)
            {
                if (model.SelectedRoles != null)
                {
                    foreach (string role in model.SelectedRoles)
                    {
                        result = UserManager.AddToRole(user.Id, role);
                    }
                }
            }

            return result;
        }

        public void SwitchLockoutUserByEmail(ApplicationUserManager UserManager, string email)
        {
            var user = UserManager.FindByEmail(email);
            user.LockoutEnabled = true;
            if (user.LockoutEndDateUtc == null || user.LockoutEndDateUtc == DateTimeOffset.MinValue)
            {
                UserManager.SetLockoutEndDate(user.Id, DateTime.Now.AddYears(1000));
            }
            else
            {
                UserManager.SetLockoutEndDate(user.Id, DateTimeOffset.MinValue);
            }
        }



        public EditUserViewModel GetUserForEditByEmail(ApplicationUserManager UserManager, string email)
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                var user = UserManager.FindByEmail(email);
                var userRoles = UserManager.GetRoles(user.Id);

                return new EditUserViewModel
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
            }
        }

        public IdentityResult EditUser(ApplicationUserManager UserManager, EditUserViewModel model)
        {
            ApplicationUser user = UserManager.FindByEmail(model.OldEmail);
            var rolesUser = UserManager.GetRoles(user.Id);

            IdentityResult result;
            if (rolesUser.Count() > 0)
            {
                //Удалаем все старые роли перед обновлением
                foreach (var item in rolesUser.ToList())
                {
                    result = UserManager.RemoveFromRole(user.Id, item);
                }
            }

            user.Email = model.NewEmail;
            user.UserName = model.NewEmail;
            user.FullName = model.NewFullName;
            user.EmploymentDate = model.NewEmploymentDate;

            result = UserManager.Update(user);

            if (result.Succeeded)
            {
                foreach (string role in model.SelectedRoles)
                {
                    if (model.SelectedRoles != null)
                    {
                        result = UserManager.AddToRole(user.Id, role);
                    }
                }
            }

            return result;
        }



        public ChangeUserPasswordViewModel GetUserForChangePasswordByEmail(ApplicationUserManager UserManager, string email)
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                var user = UserManager.FindByEmail(email);
                var userRoles = UserManager.GetRoles(user.Id);

                return new ChangeUserPasswordViewModel
                {
                    FullName = user.FullName
                    , Email = user.Email
                    , EmploymentDate = user.EmploymentDate.ToShortDateString()
                    , AllRoles = string.Join(", ", userRoles)
                };
            }
        }

        public IdentityResult ChangeUserPassword(ApplicationUserManager UserManager, ChangeUserPasswordViewModel model)
        {
            ApplicationUser user =  UserManager.FindByEmail(model.Email);

            string token = UserManager.GeneratePasswordResetToken(user.Id);
            return UserManager.ResetPassword(user.Id, token, model.NewPassword);
        }

        public IList<SelectListItem> GetSelectListItemRoles()
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

        public IList<SelectListItem> GetSelectListItemRoles(IList<string> roles)
        {
            IList<SelectListItem> result = GetSelectListItemRoles();
            if (roles != null)
            {
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
            }
            return result;
        }
    }
}