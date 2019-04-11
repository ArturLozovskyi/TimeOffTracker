    using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.EntityFramework;

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
}