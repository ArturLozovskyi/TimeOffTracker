using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TimeOffTracker.Models
{
    public class CreateVacationRequestViewModel
    {
        [Required]
        public Requests VacationRequest { get; set; }
        [Required]
        public List<ApplicationUser> Approvers { get; set; }
    }
}