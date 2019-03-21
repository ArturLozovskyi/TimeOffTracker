using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.SqlClient;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeOffTracker.Models
{
    // Чтобы добавить данные профиля для пользователя, можно добавить дополнительные свойства в класс ApplicationUser. Дополнительные сведения см. по адресу: http://go.microsoft.com/fwlink/?LinkID=317594.
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [DisplayName("ФИО")]
        public string FullName { get; set; }

        [Required]
        [DisplayName("Дата приема на работу")]
        [Column(TypeName = "date")]
        public DateTime EmploymentDate { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Обратите внимание, что authenticationType должен совпадать с типом, определенным в CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Здесь добавьте утверждения пользователя
            return userIdentity;
        }
    }

    public class Requests
    {
        [Key]
        [DisplayName("Индекс")]
        public int Id { get; set; }

        [Required]
        [DisplayName("Дата начала")]
        [DataType(DataType.Date)]
        public DateTime DateStart { get; set; }

        [Required]
        [DisplayName("Дата конца")]
        [DataType(DataType.Date)]
        public DateTime DateEnd { get; set; }


        [DisplayName("Заказывающий")]
        virtual public ApplicationUser Employee { get; set; }

        [Required]
        [DisplayName("Тип отпуска")]
        virtual public VacationTypes VacationTypes { get; set; }

        [Required]
        [DisplayName("Описание")]
        public string Description { get; set; }
    }

    public class VacationTypes
    {
        [Key]
        [DisplayName("Индекс")]
        public int Id { get; set; }

        [Required]
        [DisplayName("Название отпуска")]
        public string Name { get; set; }

        [Required]
        [DisplayName("Макс. кол-во дней")]
        public int MaxDays { get; set; }
    }

    public class UserVacationDays
    {
        [Key]
        [DisplayName("Индекс")]
        public int Id { get; set; }

        [Required]
        [DisplayName("Сотрудник")]
        virtual public ApplicationUser User { get; set; }

        [Required]
        [DisplayName("Тип отпуска")]
        virtual public VacationTypes VacationType { get; set; }

        [Required]
        [DisplayName("Текущее кол-во дней")]
        public int VacationDays { get; set; }

        [Required]
        [DisplayName("Дата последнего обновления")]
        [DataType(DataType.Date)]
        public DateTime LastUpdate { get; set; }
    }

    public class RequestStatuses
    {
        [Key]
        [DisplayName("Индекс")]
        public int Id { get; set; }

        [Required]
        [DisplayName("Статус")]
        public string Name { get; set; }
    }

    public class RequestChecks
    {
        [Key]
        [DisplayName("Индекс")]
        public int Id { get; set; }

        [Required]
        [DisplayName("Запрос")]
        virtual public Requests Request { get; set; }

        [Required]
        [DisplayName("Проверяющий")]
        virtual public ApplicationUser Approver { get; set; }

        [Required]
        [DisplayName("Статус")]
        virtual public RequestStatuses Status { get; set; }

        [Required]
        [DisplayName("Приоритет проверяющего")]
        public int Priority { get; set; }

        [Required]
        [DisplayName("Причина отказа")]
        public string Reason { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection3", throwIfV1Schema: false)
        {

        }

        public DbSet<Requests> Requests { get; set; }
        public DbSet<VacationTypes> VacationTypes { get; set; }
        public DbSet<UserVacationDays> UserVacationDays { get; set; }
        public DbSet<RequestStatuses> RequestStatuses { get; set; }
        public DbSet<RequestChecks> RequestChecks { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}