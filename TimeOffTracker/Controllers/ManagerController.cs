using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimeOffTracker.Models;
using TimeOffTracker.Models.ManagerModels;
using Microsoft.AspNet.Identity;
using TimeOffTracker.Business;
using System.Data.Entity;

namespace TimeOffTracker.Controllers.ManagerControllers
{
    public class ManagerController : Controller
    {
        // GET: Manager
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Manager")]
        public ActionResult ManagerPanel()
        {
            // Приоритет будет таков:
            // Самый верхний аппр = 1 и дальше инкремент
            // т.е. для проверки должно быть несколько условий

            string id = User.Identity.GetUserId();      //Узнать ID активного пользователя

            var model = new ListActiveRequests(id);
            return View(model.GetListRequestsModel());
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public ActionResult Confirm(int? id)
        {
            string idUser = User.Identity.GetUserId();
             if (id == null) { return HttpNotFound(); }

            // Менять статус на подтверждено
            using (var context = new ApplicationDbContext())
            {

                RequestChecks request = context.RequestChecks.Find(id);
                if (request != null)
                {
                    context.Entry(request).State = EntityState.Modified;

                    RequestStatuses status = context.RequestStatuses.Find(1);   // Подтвержден
                    request.Status = status;
                    context.SaveChanges();  // ?   
                }

            }

            var model = new ListActiveRequests(idUser);
            var m = model.GetListRequestsModel();
            return View("ManagerPanel", m);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public ActionResult Reject()
        {
            // Менять статус на отклонено у всех последующих аппров

            return View();
        }
    }
}