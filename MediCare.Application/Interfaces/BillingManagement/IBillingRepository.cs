using MediCare_MVC_Project.MediCare.Domain.Entity;
using MediCare_MVC_Project.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediCare_MVC_Project.MediCare.Application.Interfaces.BillingManagement
{
    public interface IBillingRepository
    {
        Task<IEnumerable<PaymentInvoiceViewModel>> GetPaymentInvoicesAsync();
        Task<IEnumerable<Appointment>> GetCompletedAppointmentsQuery();
        Task<IEnumerable<PatientTest>> GetPatientTestsQuery(int patientId);
        Task<Invoice> CreateInvoiceQuery(Invoice invoice);
        Task<Payment> GetPaymentByIdAsync(int paymentId);
        Task<Invoice> GetInvoiceByIdAsync(int invoiceId);
        Task<Payment> CreatePaymentQuery(Payment payment);
        Task<decimal> GetLabTestTotalQuery(int patientId);
        Task<int> GetAdmissionDaysQuery(int patientId);
        Task<decimal> GetAdmissionCostQuery(int patientId);
    }
}
