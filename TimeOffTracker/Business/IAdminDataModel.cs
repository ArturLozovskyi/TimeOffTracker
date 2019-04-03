﻿using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using TimeOffTracker.Models;

namespace TimeOffTracker.Business
{
    public interface IAdminDataModel
    {
        ListShowUserViewModel GetAllUsersForShow();
        ShowUserViewModel GetUserForShowByEmail(ApplicationUserManager UserManager, string email);
        IdentityResult CreateUser(ApplicationUserManager UserManager, CreateUserViewModel model);
        void SwitchLockoutUserByEmail(ApplicationUserManager UserManager, string email);
        EditUserViewModel GetUserForEditByEmail(ApplicationUserManager UserManager, string email);
        IdentityResult EditUser(ApplicationUserManager UserManager, EditUserViewModel model);
        ChangeUserPasswordViewModel GetUserForChangePasswordByEmail(ApplicationUserManager UserManager, string email);
        IdentityResult ChangeUserPassword(ApplicationUserManager UserManager, ChangeUserPasswordViewModel model);
        IList<SelectListItem> GetSelectListItemRoles();
        IList<SelectListItem> GetSelectListItemRoles(IList<string> roles);
    }
}
