using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.EntityFramework;
using System;

namespace TimeOffTracker.Models
{
    public class VacationRequestViewModel
    {   [Required]
        public ApplicationUser User { get; set; }
        [Required]
        public Requests VacationRequest { get; set; }
        [Required]
        public List<VacationTypes> VacationTypes { get; set; }

        [Required]
        public List<ApplicationUser> Approvers { get; set; }
    }

    //Часть от меня
    public class HistoryOfVacationViewModel
    {
        public int Id { get; set; } // id Request

        [Display(Name = "Type vacation")]
        public string VacationType { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Date start")]
        public DateTime DateStart { get; set; }

        [Display(Name = "Date end")]
        public DateTime DateEnd { get; set; }

        [Display(Name = "Days")]
        public string Days { get; set; }

        public List<RequestChecks> Approvers { get; set; }

        [Display(Name = "Rejection reason")]
        public string Reson { get; set; }

    }
    //И тоже от меня
    public class ListHistoryOfVacationViewModel
    {
        public List<UserVacationDays> UserVacationDays { get; set; }
        public List<HistoryOfVacationViewModel> AllVacations { get; set; }
    }
}