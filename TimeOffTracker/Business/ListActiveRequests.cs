using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using TimeOffTracker.Models;
using TimeOffTracker.Models.ManagerModels;

namespace TimeOffTracker.Business
{
    public class ListActiveRequests:IListActiveRequests
    {
        

        /*
private string id;

public ListActiveRequests(string id)
{
   this.id = id;
}

public ListRequestsModel GetListRequestsModel()
{
   using (var context = new ApplicationDbContext())
   {
       var r = context.RequestChecks
           .Where(s => s.Status.Name == "Ожидание1")   // Тестовый статус заменить на прод
           .AsEnumerable()     // Нужно юзать enumerable, потому что если его не юзать, то будет ругаться на TakeWhile
           .GroupBy(g => g.Request)
           .Select(group => group.OrderBy(o => o.Priority).TakeWhile(w => w.Approver.Id == id))
           .SelectMany(grop => grop)
           .Select(rCheck => new
           {
               rCheck.Id,
               rCheck.Request.Employee.FullName,
               rCheck.Request.Employee.Email,
               rCheck.Request.DateStart,
               rCheck.Request.DateEnd,
               rCheck.Request.VacationTypes.Name,
               rCheck.Request.Description
           }).ToList();


       var model = new ListRequestsModel
       {
           Items = r.Select(s => new RequestsModel
           {
               Id = s.Id,
               FullNameEmployee = s.FullName,
               EmailEmployee = s.Email,
               DateStart = s.DateStart,
               DateEnd = s.DateEnd,
               VacationType = s.Name,
               Description = s.Description
           }).ToList()
       };

       return model;
   }
}
*/


        public ListRequestsModel GetListRequestsModel(string id)
        {
            using (var context = new ApplicationDbContext())
            {
                var r = context.RequestChecks
                    .Where(s => s.Status.Name == "Ожидание1")   // Тестовый статус заменить на прод
                    .AsEnumerable()     // Нужно юзать enumerable, потому что если его не юзать, то будет ругаться на TakeWhile
                    .GroupBy(g => g.Request)
                    .Select(group => group.OrderBy(o => o.Priority).TakeWhile(w => w.Approver.Id == id))
                    .SelectMany(grop => grop)
                    .Select(rCheck => new
                    {
                        rCheck.Id,
                        rCheck.Request.Employee.FullName,
                        rCheck.Request.Employee.Email,
                        rCheck.Request.DateStart,
                        rCheck.Request.DateEnd,
                        rCheck.Request.VacationTypes.Name,
                        rCheck.Request.Description
                    }).ToList();

                var model = new ListRequestsModel
                {
                    Items = r.Select(s => new RequestsModel
                    {
                        Id = s.Id,
                        FullNameEmployee = s.FullName,
                        EmailEmployee = s.Email,
                        DateStart = s.DateStart,
                        DateEnd = s.DateEnd,
                        VacationType = s.Name,
                        Description = s.Description
                    }).ToList()
                };
                return model;
            }
        }

        public void Confirm(int? id)
        {
            using (var context = new ApplicationDbContext())
            {
                RequestChecks request = context.RequestChecks.Find(id);
                if (request != null)
                {
                    context.Entry(request).State = EntityState.Modified;

                    RequestStatuses status = context.RequestStatuses.Find(1);
                    request.Status = status;
                    context.SaveChanges();
                }
            }
        }

        public void Reject(int? id, string reason)
        {
            using (var context = new ApplicationDbContext())
            {
                RequestChecks request = context.RequestChecks.Find(id);
                if (request != null)
                {
                    var rej = context.RequestChecks
                        .Where(w => w.Request.Id == request.Request.Id)
                        .Where(e => e.Priority >= request.Priority)
                        .OrderBy(o => o.Priority)
                        .ToList();

                    RequestStatuses statusReject = context.RequestStatuses.Find(2);
                    for (int i = 0; i < rej.Count; i++)
                    {
                        context.Entry(rej[i]).State = EntityState.Modified;
                        rej[i].Status = statusReject;
                        if (i == 0) { rej[i].Reason = reason; }
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}