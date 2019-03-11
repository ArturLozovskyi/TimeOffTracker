using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TimeOffTracker.Models
{
    public class CreateUserModel
    {
        [Required]
        [Display(Name = "ФИО")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Адрес электронной почты")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Значение {0} должно содержать не менее {2} символов.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Display(Name = "Роли")]
        public IList<SelectListItem> AvailableRoles { get; set; }
        public IList<string> SelectedRoles { get; set; }
    }

    public class EditUserModel
    {
        [Required]
        [Display(Name = "Новый ФИО")]
        public string FullName { get; set; }
        [Display(Name = "Старый ФИО")]
        public string OldFullName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Новый адрес электронной почты")]
        public string Email { get; set; }
        [Display(Name = "Старый адрес электронной почты")]
        public string OldEmail { get; set; }

        //[Required]
        //[StringLength(100, ErrorMessage = "Значение {0} должно содержать не менее {2} символов.", MinimumLength = 6)]
        //[DataType(DataType.Password)]
        //[Display(Name = "Новый пароль")]
        //public string Password { get; set; }
        //[Display(Name = "Старый пароль")]
        //public string OldPassword { get; set; }

        [Display(Name = "Новые роли")]
        public IList<SelectListItem> AvailableRoles { get; set; }
        public IList<string> SelectedRoles { get; set; }
        [Display(Name = "Старые роли")]
        public string OldRoles { get; set; }
    }

        public class ShowUsersInfo
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string DateCreate { get; set; }
        public string AllRoles { get; set; }
    }
    public class ListShowUsersInfo
    {
        public IList<ShowUsersInfo> MenuItems { get; set; }
    }
}