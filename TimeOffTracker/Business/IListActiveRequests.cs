using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeOffTracker.Models.ManagerModels;

namespace TimeOffTracker.Business
{
    public interface IListActiveRequests
    {
        // Добавить подробности о заявки
        ListRequestsModel GetListRequestsModel(string id);
        void Confirm(int? id);
        void Reject(int? id, string reason);
    }
}
