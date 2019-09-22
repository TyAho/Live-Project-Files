using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using ManagementPortal.Enums;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ManagementPortal.Models
{
    public class ApplicationUser : IdentityUser
    {

        // PROFILE PICTURE
        public byte[] ProfilePicture { get; set; }

        //Set DisplayName with first name and last initial
        public string DisplayName { get { return FirstName + " " + LastName.Substring(0, 1); } internal set { FirstName = value; LastName = value; } }
        [Required(ErrorMessage = "Required Field. Please enter a First Name:"), Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Required Field. Please enter a Last Name: "), Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Required Field. Please enter a Work Type >> Leadman,Foreman,ExpMBA, or NewMBA: "), Display(Name = "Work Type")]
        public WorkType WorkType { get; set; }
        public string UserRole { get; set; }
        [Display(Name = "Suspended")]
        public bool Suspended { get; set; }

        public string FullName { get { return (FirstName + " " + LastName); } }
        public virtual List<Job> Jobs { get; set; }
        public virtual List<Schedule> Schedules { get; set; }
        public virtual PersonalProfile Profile { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
        //add the user identity here for the aspnetuser table.

    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        // Begin DbSets

        public DbSet<CalendarEvent> CalendarEvents { get; set; }
        public DbSet<CreateUserRequest> CreateUserRequests { get; set; }

        public DbSet<ChatMessage> ChatMessages { get; set; }

        public DbSet<CompanyNews> CompanyNews { get; set; }

        public DbSet<Job> Jobs { get; set; }
        public DbSet<JobSite> JobSites { get; set; }
        public DbSet<ShiftTime> ShiftTime { get; set; }
        public DbSet<JobOther> JobOthers { get; set; }
        public DbSet<PersonalProfile> Profile { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<JobAction> JobActions { get; set; }

        //public virtual DbSet<CalEvent> CalendarEvents { get; set; }

        // End DbSets

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //FluentAPI Model Config Goes Here
        }

        //Scafolding databases
        //public System.Data.Entity.DbSet<ManagementPortal.Models.Jobsite> Jobsites { get; set; }
        //Area specific scaffolding databases
        public System.Data.Entity.DbSet<ManagementPortal.Areas.Employee.Models.EmployeeInfo> Employees { get; set; }
    }
}