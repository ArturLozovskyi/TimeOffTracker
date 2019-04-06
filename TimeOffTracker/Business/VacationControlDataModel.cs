using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TimeOffTracker.Models;

namespace TimeOffTracker.Business
{
    public class VacationControlDataModel: IVacationControlDataModel
    {
        public void BindingMissingVacationByEmail(string email)
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                ApplicationUser user = context.Users.Where(m => m.Email == email).First();

                var listVacationTypes = context.VacationTypes.ToList();
                var listUserVacation = context.UserVacationDays.Where(m => m.User.Id == user.Id).ToList();

                foreach (var vacType in listVacationTypes)
                {
                    if (DoesUserContainVacationType(vacType, listUserVacation) == false)
                    {
                        context.UserVacationDays.Add(new UserVacationDays()
                        {
                            User = user,
                            LastUpdate = user.EmploymentDate,
                            VacationType = vacType,
                            VacationDays = vacType.MaxDays
                        });
                    }
                }
                context.SaveChanges();
            }
        }

        //Возвращает список отпусков которые были добавлены пользователю
        public List<string> BindingMissingVacationWithMessageByEmail(string email)
        {
            List<string> result = new List<string>();
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                ApplicationUser user = context.Users.Where(m => m.Email == email).First();

                var listVacationTypes = context.VacationTypes.ToList();
                var listUserVacation = context.UserVacationDays.Where(m => m.User.Id == user.Id).ToList();

                foreach (var vacType in listVacationTypes)
                {
                    if (DoesUserContainVacationType(vacType, listUserVacation) == false)
                    {
                        result.Add(vacType.Name);
                        context.UserVacationDays.Add(new UserVacationDays()
                        {
                            User = user,
                            LastUpdate = user.EmploymentDate,
                            VacationType = vacType,
                            VacationDays = vacType.MaxDays
                        });
                    }
                }
                context.SaveChanges();
            }
            return result;
        }

        private bool DoesUserContainVacationType(VacationTypes vacationType, List<UserVacationDays> userVacations)
        {
            foreach (var item in userVacations)
            {
                if (item.VacationType == vacationType)
                {
                    return true;
                }
            }
            return false;
        }

        public void UpdateUserVacationDaysByEmail(string email)
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                ApplicationUser user = context.Users.Where(m => m.Email == email).First();

                var listUserVacation = context.UserVacationDays.Where(m => m.User.Id == user.Id).ToList();

                int daysInYear = 365;
                bool isOk = true;
                //Обновление происходит пошагово, для того что бы верно просчитать кол-во дней для старых сотрудников
                do
                {
                    isOk = true;
                    foreach(var userVac in listUserVacation)
                    {
                        if ((DateTime.Now - userVac.LastUpdate).Days >= daysInYear)
                        {
                            userVac.VacationDays += userVac.VacationType.MaxDays;
                            userVac.LastUpdate = new DateTime(userVac.LastUpdate.AddYears(1).Year, user.EmploymentDate.Month, user.EmploymentDate.Day);
                            isOk = false;
                        }
                    }
                } while (!isOk);
                context.SaveChanges();
            }
        }
    }
}