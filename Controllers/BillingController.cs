using MediCare_MVC_Project.MediCare.Application.Interfaces.BillingManagement;
using MediCare_MVC_Project.MediCare.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace MediCare_MVC_Project.Controllers
{
    public class BillingController : Controller
    {
        private readonly IBillingService _billingService;
        private readonly IBillingRepository _billingRepository;

        public BillingController(IBillingService billingService, IBillingRepository billingRepository)
        {
            _billingService = billingService;
            _billingRepository = billingRepository;
        }

        [HttpGet]
        public async Task<IActionResult> PaymentInvoiceList()
        {
            try
            {
                var paymentInvoices = await _billingRepository.GetPaymentInvoicesAsync();
                return View(paymentInvoices); // Pass the data to the view
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: Billing/CalculateTotalBill/5
        [HttpGet]
        public async Task<IActionResult> CalculateTotalBill(int appointmentId)
        {
            try
            {
                var totalBill = await _billingService.CalculateTotalBillAsync(appointmentId);
                return View(totalBill); // Assuming you have a view to display the total bill
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: Billing/GenerateInvoice/5
        [HttpGet]
        public async Task<IActionResult> GenerateInvoice(int appointmentId)
        {
            try
            {
                var invoice = await _billingService.GenerateInvoiceAsync(appointmentId);
                return View(invoice); // Return the invoice to display in the view
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: Billing/ProcessPayment
        [HttpPost]
        public async Task<IActionResult> ProcessPayment(int invoiceId, decimal amountPaid, string method)
        {
            try
            {
                var payment = await _billingService.ProcessPaymentAsync(invoiceId, amountPaid, method);
                return RedirectToAction("SendReceiptEmail", new { paymentId = payment.PaymentId });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: Billing/SendReceiptEmail/5
        [HttpGet]
        public async Task<IActionResult> SendReceiptEmail(int paymentId)
        {
            try
            {
                await _billingService.SendReceiptEmailAsync(paymentId);
                return View("ReceiptSent"); // Return a view confirming that the email was sent
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: Billing/DownloadReceipt/5
        [HttpGet]
        public async Task<IActionResult> DownloadReceipt(int paymentId)
        {
            try
            {
                var receiptPdf = await _billingService.GenerateReceiptPdfAsync(paymentId);
                return File(receiptPdf, "application/pdf", "Receipt.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: Billing/SendPaymentEmail/5
        [HttpGet]
        public async Task<IActionResult> SendPaymentEmail(int paymentId)
        {
            try
            {
                await _billingService.SendReceiptEmailAsync(paymentId);
                return RedirectToAction("Index"); // Redirect back to the main page or show a confirmation
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //// GET: Billing/DeletePayment/5
        //[HttpGet]
        //public async Task<IActionResult> DeletePayment(int paymentId)
        //{
        //    try
        //    {
        //        var payment = await _billingRepository.GetPaymentByIdAsync(paymentId);
        //        if (payment == null)
        //        {
        //            return NotFound();
        //        }

        //        // Delete the payment
        //        _billingRepository.Delete(payment); // Assuming Delete method exists in your repository
        //        await _billingRepository.SaveChangesAsync();

        //        return RedirectToAction("Index"); // Redirect back to the list of payments
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}
    }
}