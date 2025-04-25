using MediCare_MVC_Project.MediCare.Application.Interfaces.BillingManagement;
using MediCare_MVC_Project.MediCare.Domain.Entity;
using MediCare_MVC_Project.MediCare.Infrastructure.Database;
using MediCare_MVC_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace MediCare_MVC_Project.MediCare.Infrastructure.Repository
{
    public class BillingRepository : GenericRepository<Invoice>, IBillingRepository
    {
        public BillingRepository(ApplicationDBContext context) : base(context)
        {
        }

        public async Task<IEnumerable<PaymentInvoiceViewModel>> GetPaymentInvoicesAsync()
        {
            const decimal fixedAmount = 1500;

            var appointments = await _context.Appointments
                .Where(a => a.Status == "Completed")
                .Include(a => a.Patient)
                .ToListAsync();

            var paymentInvoices = new List<PaymentInvoiceViewModel>();

            foreach (var appointment in appointments)
            {
                // Check if invoice already exists for this appointment to avoid duplicates
                var existingInvoice = await _context.Invoices
                    .FirstOrDefaultAsync(i => i.AppointmentId == appointment.AppointmentId);

                if (existingInvoice != null)
                {
                    // Optionally include existing payments and add to list
                    var payment = await _context.Payments
                        .FirstOrDefaultAsync(p => p.InvoiceId == existingInvoice.InvoiceId);

                    paymentInvoices.Add(new PaymentInvoiceViewModel
                    {
                        PaymentId = payment?.PaymentId ?? 0,
                        FirstName = appointment.Patient.FirstName,
                        LastName = appointment.Patient.LastName,
                        Email = appointment.Patient.Email,
                        MobileNo = appointment.Patient.MobileNo,
                        AppointmentId = appointment.AppointmentId,
                        Amount = existingInvoice.Amount,
                        AmoundPaid = payment?.AmountPaid ?? 0,
                        PaymentStatus = existingInvoice.PaymentStatus,
                        PaymentMethod = payment?.PaymentMethod ?? "Not Paid",
                        PaymentDate = payment.PaymentDate
                    });

                    continue;
                }

                // Create and save new invoice
                var invoice = new Invoice
                {
                    AppointmentId = appointment.AppointmentId,
                    Amount = fixedAmount,
                    PaymentStatus = "Pending",
                    InvoiceUrl = "generated-invoice-url", // Optional
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync(); // Save to get InvoiceId

                // Create and save payment
                var paymentEntity = new Payment
                {
                    InvoiceId = invoice.InvoiceId,
                    PaymentMethod = "Other",
                    AmountPaid = 0,
                    PaymentDate = DateOnly.FromDateTime(DateTime.Now),
                    CreatedAt = DateTime.Now
                };

                _context.Payments.Add(paymentEntity);
                await _context.SaveChangesAsync(); // Save to get PaymentId

                // Add to result list
                paymentInvoices.Add(new PaymentInvoiceViewModel
                {
                    PaymentId = paymentEntity.PaymentId,
                    FirstName = appointment.Patient.FirstName,
                    LastName = appointment.Patient.LastName,
                    Email = appointment.Patient.Email,
                    MobileNo = appointment.Patient.MobileNo,
                    AppointmentId = appointment.AppointmentId,
                    Amount = invoice.Amount,
                    AmoundPaid = paymentEntity.AmountPaid,
                    PaymentStatus = invoice.PaymentStatus,
                    PaymentMethod = paymentEntity.PaymentMethod,
                    PaymentDate = paymentEntity.PaymentDate
                });
            }

            return paymentInvoices;
        }

        public async Task<Invoice> CreateInvoiceQuery(Invoice invoice)
        {
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
            return invoice;
        }

        public async Task<Invoice> GetInvoiceByIdAsync(int invoiceId)
        {
            return await _context.Invoices
                .Where(i => i.InvoiceId == invoiceId)
                .FirstOrDefaultAsync();
        }

        public async Task<Payment> GetPaymentByIdAsync(int paymentId)
        {
            return await _context.Payments
                .Where(p => p.PaymentId == paymentId)
                .FirstOrDefaultAsync();
        }

        public async Task<Payment> CreatePaymentQuery(Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<IEnumerable<Appointment>> GetCompletedAppointmentsQuery()
        {
            return await _context.Appointments.Include(s => s.Invoice).ThenInclude(s => s.Payment)
                .Where(a => a.Status == "Completed")
                .ToListAsync();
        }

        public async Task<decimal> GetLabTestTotalQuery(int patientId)
        {
            return await (from pt in _context.PatientTests
                          join lt in _context.LabTests on pt.TestId equals lt.TestId
                          where pt.PatientId == patientId
                          select lt.Cost)
                          .SumAsync();
        }

        public async Task<IEnumerable<PatientTest>> GetPatientTestsQuery(int patientId)
        {
            return await _context.PatientTests
                .Include(pt => pt.LabTest) // If navigation property exists
                .Where(pt => pt.PatientId == patientId)
                .ToListAsync();
        }

        public async Task<int> GetAdmissionDaysQuery(int patientId)
        {
            var admission = await _context.PatientAdmissions
                .Where(pa => pa.PatientId == patientId && pa.IsDischarged)
                .OrderByDescending(pa => pa.AdmissionDate)
                .FirstOrDefaultAsync();

            if (admission == null || !admission.DischargeDate.HasValue)
            {
                return 0; // No admission or discharge date available
            }

            var admissionDays = (admission.DischargeDate.Value.ToDateTime(new TimeOnly(0, 0)) - admission.AdmissionDate.ToDateTime(new TimeOnly(0, 0))).Days;

            return admissionDays;
        }

        public async Task<decimal> GetAdmissionCostQuery(int patientId)
        {
            // Find the most recent patient admission where the patient has been discharged
            var admission = await _context.PatientAdmissions
                .Where(pa => pa.PatientId == patientId && pa.IsDischarged)
                .OrderByDescending(pa => pa.AdmissionDate)
                .FirstOrDefaultAsync();

            if (admission == null || !admission.DischargeDate.HasValue)
            {
                return 0m; // No admission or discharge date available
            }

            // Get the bed details for the admission
            var bed = await _context.Beds
                .Where(b => b.BedId == admission.BedId)
                .FirstOrDefaultAsync();

            if (bed == null)
            {
                return 0m; // No bed information found
            }

            // Calculate the number of days the patient was admitted
            var admissionDays = (admission.DischargeDate.Value.ToDateTime(new TimeOnly(0, 0)) - admission.AdmissionDate.ToDateTime(new TimeOnly(0, 0))).Days;

            // Assuming the bed's room charges per day is stored in the `Room` table
            var room = await _context.Rooms
                .Where(r => r.RoomId == bed.RoomId)
                .FirstOrDefaultAsync();

            if (room == null)
            {
                return 0m; // No room info found
            }

            // Assuming Room charges are stored in `RoomType` or a fixed amount
            decimal roomChargePerDay = room.RoomType == "Standard" ? 1000 : 2000; // Example pricing

            // Calculate the total cost of admission
            return admissionDays * roomChargePerDay;
        }
    }
}