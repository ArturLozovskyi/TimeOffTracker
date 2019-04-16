using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeOffTracker.Models.ManagerModels
{

    /*DELETE THIS*/
    public class ListHistoryModel
    {
        public IList<HistoryModel> Items { get; set; }
    }

    public class HistoryModel
    {
        public ApplicationUser Employee { get; set; }
    }
}