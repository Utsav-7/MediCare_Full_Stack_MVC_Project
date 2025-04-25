using MediCare_MVC_Project.MediCare.Application.Interfaces;
using MediCare_MVC_Project.MediCare.Application.Interfaces.AdmissionManagement;
using MediCare_MVC_Project.MediCare.Application.Interfaces.AppointmentManagement;
using MediCare_MVC_Project.MediCare.Application.Interfaces.Authentication;
using MediCare_MVC_Project.MediCare.Application.Interfaces.BillingManagement;
using MediCare_MVC_Project.MediCare.Application.Interfaces.CheckUpListManagement;
using MediCare_MVC_Project.MediCare.Application.Interfaces.DashboardManagement;
using MediCare_MVC_Project.MediCare.Application.Interfaces.DoctorManagement;
using MediCare_MVC_Project.MediCare.Application.Interfaces.LabTestManagement;
using MediCare_MVC_Project.MediCare.Application.Interfaces.PatientManagement;
using MediCare_MVC_Project.MediCare.Application.Interfaces.PaymentManagement;
using MediCare_MVC_Project.MediCare.Application.Interfaces.ReceptionistManagement;
using MediCare_MVC_Project.MediCare.Application.Interfaces.SpecializationManagement;
using MediCare_MVC_Project.MediCare.Application.Interfaces.UserManagement;
using MediCare_MVC_Project.MediCare.Application.Services;
using MediCare_MVC_Project.MediCare.Common.Helpers;
using MediCare_MVC_Project.MediCare.Infrastructure.Repository;

namespace MediCare_MVC_Project.MediCare.Infrastructure.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IAdmissionRepository, AdmissionRepository>();
            services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IBedRepository, BedRepository>();
            services.AddScoped<ICheckUpRepository, CheckUpRepository>();
            services.AddScoped<IDoctorRepository, DoctorRepository>();
            services.AddScoped<ILabTestRepository, LabTestRepository>();
            services.AddScoped<IPatientRepository, PatientRepository>();
            services.AddScoped<IPatientTestRepository, PatientTestRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IReceptionistRepository, ReceptionistRepository>();
            services.AddScoped<IRoomRepository, RoomRepository>();
            services.AddScoped<ISpecializationRepository, SpecializationRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IBillingRepository, BillingRepository>();
            services.AddScoped<IReceptionistDashboardRepository, ReceptionistDashboardRepository>();
            services.AddScoped<IDoctorDashboardRepository, DoctorDashboardRepository>();
            services.AddScoped<IAdminDashboardRepository, AdminDashboardRepository>();
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthHelper>(); 
            services.AddScoped<IEmailHelper, EmailHelper>();
            services.AddScoped<JWTTokenHelper>();
            services.AddScoped<IDownloadHelper, NoteDownloadHelper>();
            services.AddScoped<IAdmissionService, AdmissionService>();
            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<IBedService, BedService>();
            services.AddScoped<ICheckUpService, CheckUpService>();
            services.AddScoped<IDoctorService, DoctorService>();
            services.AddScoped<ILabTestService, LabTestService>();
            services.AddScoped<IPatientService, PatientService>();
            services.AddScoped<IPatientTestService, PatientTestService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IReceptionistService, ReceptionistService>();
            services.AddScoped<IRoomService, RoomService>();
            services.AddScoped<ISpecializationService, SpecializationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IBillingService, BillingService>();
            services.AddScoped<IAdminDashboardService, AdminDashboardService>();
            services.AddScoped<IDoctorDashboardService, DoctorDashboardService>();
            services.AddScoped<IReceptionistDashboardService, ReceptionistDashboardService>();
            return services;
        }
    }
}