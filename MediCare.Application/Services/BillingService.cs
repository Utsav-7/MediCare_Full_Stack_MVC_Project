using MediCare_MVC_Project.MediCare.Application.Interfaces.BillingManagement;
using MediCare_MVC_Project.MediCare.Domain.Entity;
using MediCare_MVC_Project.MediCare.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net;
using MediCare_MVC_Project.MediCare.Application.DTOs.CheckUpDTOs;

namespace MediCare_MVC_Project.MediCare.Application.Services
{
    public class BillingService : IBillingService
    {
        private readonly IBillingRepository _billingRepository;
        private readonly IEmailHelper _emailHelper;
        private readonly ILogger<BillingService> _logger;

        public BillingService(IBillingRepository billingRepository, IEmailHelper emailHelper, ILogger<BillingService> logger)
        {
            _billingRepository = billingRepository;
            _emailHelper = emailHelper;
            _logger = logger;
        }

        public async Task<decimal> CalculateTotalBillAsync(int appointmentId)
        {
            // First, await the result to get the list of completed appointments
            var appointments = await _billingRepository.GetCompletedAppointmentsQuery();

            // Then, use LINQ to filter the result
            var appointment = appointments
                .Where(a => a.AppointmentId == appointmentId)
                .FirstOrDefault();

            if (appointment == null)
                throw new InvalidOperationException("Appointment not found.");

            // Define fixed cost per appointment
            decimal fixedAppointmentCost = 1000;  // Adjust this value as per your requirement

            // Get the total lab test cost
            var labTestTotal = await _billingRepository.GetLabTestTotalQuery(appointment.PatientId);

            // Get the admission days cost (if applicable)
            var admissionDays = await _billingRepository.GetAdmissionDaysQuery(appointment.PatientId);
            var admissionCost = admissionDays * 1000; // Example fixed cost per day (adjust accordingly)

            // Total cost calculation using fixed appointment cost
            var totalBill = fixedAppointmentCost + labTestTotal + admissionCost;

            return totalBill;
        }

        public async Task<Invoice> GenerateInvoiceAsync(int appointmentId)
        {
            // Await the result to get the list of completed appointments
            var appointments = await _billingRepository.GetCompletedAppointmentsQuery();

            // Then, use LINQ to filter the result
            var appointment = appointments
                .Where(a => a.AppointmentId == appointmentId)
                .FirstOrDefault();

            if (appointment == null) throw new InvalidOperationException("Appointment not found.");

            // Calculate total bill
            var totalBill = await CalculateTotalBillAsync(appointmentId);

            // Create invoice
            var invoice = new Invoice
            {
                AppointmentId = appointmentId,
                Amount = totalBill,
                PaymentStatus = "Pending",
                InvoiceUrl = "", // You can generate a URL for storing the invoice, if needed
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Save the invoice to the database
            return await _billingRepository.CreateInvoiceQuery(invoice);
        }

        public async Task<Payment> ProcessPaymentAsync(int invoiceId, decimal amountPaid, string method)
        {
            // Use the appropriate repository method to get the invoice (not appointments)
            var invoice = await _billingRepository.GetInvoiceByIdAsync(invoiceId); // Assuming you have this method.

            if (invoice == null)
                throw new InvalidOperationException("Invoice not found.");

            // Create the payment record
            var payment = new Payment
            {
                InvoiceId = invoiceId,
                AmountPaid = amountPaid,
                PaymentMethod = method,
                PaymentDate = DateOnly.FromDateTime(DateTime.Today),
                CreatedAt = DateTime.UtcNow
            };

            // Save the payment to the database
            return await _billingRepository.CreatePaymentQuery(payment);
        }

        public async Task<byte[]> GenerateReceiptPdfAsync(int paymentId)
        {
            // Get the payment record from the repository
            var payment = await _billingRepository.GetPaymentByIdAsync(paymentId); // Assuming GetPaymentByIdAsync exists

            if (payment == null)
                throw new InvalidOperationException("Payment not found.");

            // Prepare PDF document
            var document = new Document(PageSize.A4);
            using (var ms = new MemoryStream())
            {
                var writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                // Add content to the PDF
                document.Add(new Paragraph($"Receipt for Payment {paymentId}"));
                document.Add(new Paragraph($"Amount Paid: {payment.AmountPaid}"));
                document.Add(new Paragraph($"Payment Method: {payment.PaymentMethod}"));
                document.Add(new Paragraph($"Payment Date: {payment.PaymentDate.ToShortDateString()}"));

                document.Close();

                // Return the PDF as a byte array
                return ms.ToArray();
            }
        }

        public async Task SendReceiptEmailAsync(int paymentId)
        {
            var completedAppointments = await _billingRepository.GetCompletedAppointmentsQuery();

            // Use LINQ on the resulting list
            var payment = completedAppointments
                .Where(p => p.Invoice.Payment.PaymentId == paymentId)
                .FirstOrDefault();

            if (payment == null) throw new InvalidOperationException("Payment not found.");

            // Now, use the already awaited completedAppointments to find the patient
            var patient = completedAppointments
                .Where(a => a.AppointmentId == payment.Invoice.AppointmentId)
                .Select(a => a.Patient)
                .FirstOrDefault();

            if (patient == null) throw new InvalidOperationException("Patient not found.");

            // Generate PDF receipt
            var receiptPdf = await GenerateReceiptPdfAsync(paymentId);

            // Create a dummy PdfNoteDTO (or InvoiceDTO if you have one)
            var pdfNoteDTO = new PdfNoteDTO
            {
                PatientName = $"{patient.FirstName} {patient.LastName}",
                DoctorName = $"{payment.Invoice?.Appointment?.Doctor?.User?.FirstName} {payment.Invoice?.Appointment?.Doctor?.User?.LastName}" ?? "Unknown Doctor",
                Description = payment.Invoice?.Appointment?.AppointmentDescription ?? "Payment for medical services",
                NoteText = $"Payment ID: {paymentId}, Amount: {payment.Invoice.Amount:C}" // Assuming Amount is on the Payment object
            };

            try
            {
                await _emailHelper.SendPatientInvoiceEmailAsync(patient.Email, pdfNoteDTO, receiptPdf);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email: {ex.Message}");
                throw new InvalidOperationException("Failed to send email.");
            }
        }
    }
}