using MediCare_MVC_Project.MediCare.Domain.Entity;

namespace MediCare_MVC_Project.MediCare.Application.Interfaces.BillingManagement
{
    public interface IBillingService
    {
        Task<decimal> CalculateTotalBillAsync(int appointmentId);
        Task<Invoice> GenerateInvoiceAsync(int appointmentId);
        Task<Payment> ProcessPaymentAsync(int invoiceId, decimal amountPaid, string method);
        Task<byte[]> GenerateReceiptPdfAsync(int paymentId);
        Task SendReceiptEmailAsync(int paymentId);
    }
}