using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeOffTracker.Models;

namespace TimeOffTracker.Business
{
    public interface IVacationControlDataModel
    {
        void BindingMissingVacationByEmail(string email);
        List<string> BindingMissingVacationWithMessageByEmail(string email);

        void UpdateUserVacationDaysByEmail(string email);
    }
}
