﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TimeOffTracker.Models
{
    public class CreateUserViewModel
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

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Дата приема на работу")]
        public DateTime EmploymentDate { get; set; }

        [Display(Name = "Роли")]
        public IList<SelectListItem> AvailableRoles { get; set; }
        public IList<string> SelectedRoles { get; set; }
    }

    public class ChangeUserPasswordViewModel
    {
        [Display(Name = "ФИО")]
        public string FullName { get; set; }

        [Display(Name = "Адрес электронной почты")]
        public string Email { get; set; }

        [Display(Name = "Роли")]
        public string AllRoles { get; set; }

        [Display(Name = "Дата приема на работу")]
        public string EmploymentDate { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Значение {0} должно содержать не менее {2} символов.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        public string NewPassword { get; set; }
    }

    public class EditUserViewModel
    {
        [Required]
        [Display(Name = "Новый ФИО")]
        public string NewFullName { get; set; }
        [Display(Name = "Старый ФИО")]
        public string OldFullName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Новый адрес электронной почты")]
        public string NewEmail { get; set; }
        [Display(Name = "Старый адрес электронной почты")]
        public string OldEmail { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Новая дата приема на работу")]   
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime NewEmploymentDate { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "Старая дата приема на работу")]
        public string OldEmploymentDate { get; set; }

        [Display(Name = "Новые роли")]
        public IList<SelectListItem> AvailableRoles { get; set; }
        public IList<string> SelectedRoles { get; set; }
        [Display(Name = "Старые роли")]
        public string OldRoles { get; set; }
    }

    public class ShowUserViewModel
    {
        [Display(Name = "ФИО")]
        public string FullName { get; set; }
        [Display(Name = "Почта")]
        public string Email { get; set; }
        [Display(Name = "Дата приема на работу")]
        public string EmploymentDate { get; set; }
        [Display(Name = "Роли")]
        public string AllRoles { get; set; }
        public DateTime? LockoutTime { get; set; }
    }
    public class ListShowUserViewModel
    {
        public IList<ShowUserViewModel> MenuItems { get; set; }
    }
}