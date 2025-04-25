using System.Net.Mail;
using System.Net;
using MediCare_MVC_Project.MediCare.Application.Interfaces;
using MediCare_MVC_Project.MediCare.Application.DTOs;
using MediCare_MVC_Project.MediCare.Application.DTOs.PatientDTOs;
using MediCare_MVC_Project.MediCare.Domain.Entity;
using MediCare_MVC_Project.MediCare.Application.DTOs.AppointmentDTOs;
using MediCare_MVC_Project.MediCare.Application.DTOs.CheckUpDTOs;

namespace MediCare_MVC_Project.MediCare.Common.Helpers
{
    public class EmailHelper : IEmailHelper
    {
        private readonly IConfiguration _configuration;

        public EmailHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, byte[]? attachmentBytes = null, string? attachmentName = null)
        {
            try
            {
                using var smtpClient = new SmtpClient(_configuration["EmailSettings:SmtpServer"])
                {
                    Port = int.Parse(_configuration["EmailSettings:SmtpPort"]),
                    Credentials = new NetworkCredential(
                        _configuration["EmailSettings:SenderEmail"],
                        _configuration["EmailSettings:SenderPassword"]
                    ),
                    EnableSsl = true
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(_configuration["EmailSettings:SenderEmail"], _configuration["EmailSettings:SenderName"]),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                MemoryStream? memoryStream = null;

                try
                {
                    // Attach PDF if provided
                    if (attachmentBytes != null && !string.IsNullOrEmpty(attachmentName))
                    {
                        memoryStream = new MemoryStream(attachmentBytes);
                        var attachment = new Attachment(memoryStream, attachmentName, "application/pdf");
                        mailMessage.Attachments.Add(attachment);
                    }

                    await smtpClient.SendMailAsync(mailMessage);
                    Console.WriteLine($"✅ Email sent successfully to {toEmail}");
                }
                finally
                {
                    memoryStream?.Dispose(); // Clean up memory stream after email is sent
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email Error: {ex.Message}");
            }
        }

        private const string ContainerStart = @"
            <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #ddd; border-radius: 10px; max-width: 600px; margin: auto; background-color: #f9f9f9;'>
                <div style='text-align: center;'>
                    <img src='https://drive.google.com/uc?export=view&id=1SVjUvEmC-oxtaBkQ0f8ounCkQ2xMSPAN' alt='MediCare+ Logo' style='width: 40%; margin-bottom: 10px;' />
                </div>";

        private const string ContainerEnd = @"
                <div style='text-align: center; margin-top: 30px;'>
                    <p style='color: #7f8c8d; font-size: 14px;'>Best Regards,</p>
                    <p style='color: #2c3e50; font-size: 16px; font-weight: bold;'>MediCare+ Team</p>
                    <img src='https://drive.google.com/uc?export=view&id=1J-OM4zr7GXGMuI6Hzl6pT8HOsAL98z5n' alt='Bacancy Logo' style='width: 60%; margin-top: 10px;' />
                </div>
            </div>";

        public async Task SendUserQueryAcknowledgementEmailAsync(ContactUsDTO contactUs)
        {
            string emailSubject = "We’ve received your query – MediCare+ Support";

            string emailBody = $@"
        {ContainerStart}
            <div style='text-align: center;'>
                <h2 style='color: #2c3e50;'>Hi {contactUs.Name},</h2>
                <p style='font-size: 16px; color: #333;'>
                    Thank you for reaching out to <strong>MediCare+</strong>. We've received your query and our support team will get back to you as soon as possible.
                </p>

                <div style='background-color: #ffffff; padding: 20px; border-radius: 8px; 
                            box-shadow: 0px 0px 10px rgba(0, 0, 0, 0.1); margin: 20px auto; max-width: 500px; text-align: left;'>
                    <p style='font-size: 16px;'><strong>Query Type:</strong> {contactUs.QueryType}</p>
                    <p style='font-size: 16px;'><strong>Your Message:</strong></p>
                    <p style='font-size: 15px; color: #555;'>{contactUs.Message}</p>
                </div>

                <p style='font-size: 14px; color: #888;'>
                    We aim to respond within 24 hours. If it's urgent, feel free to call our helpline.
                </p>

                <div style='text-align: center; margin-top: 20px;'>
                    <a href='https://yourwebsite.com/contact' 
                       style='background-color: #007bff; color: #ffffff; padding: 12px 25px; 
                              text-decoration: none; border-radius: 6px; font-size: 15px; font-weight: bold;'>
                        Visit Support Page
                    </a>
                </div>
            </div>
        {ContainerEnd}";

            await SendEmailAsync(contactUs.Email, emailSubject, emailBody);
        }

        public async Task SendUserRegistrationEmailAsync(string toEmail, string? password)
        {
            string emailSubject = "Welcome to Our Platform!";
            string emailBody = $@"
                            {ContainerStart}
                                <h2 style='color: #2c3e50; text-align: center;'>Welcome to MediCare+</h2>
                                <p style='font-size: 16px; color: #333; text-align: center;'>
                                    Your account has been successfully registered.
                                </p>
                                <div style='background-color: #ffffff; padding: 15px; border-radius: 8px; 
                                            box-shadow: 0px 0px 10px rgba(0, 0, 0, 0.1); text-align: center;'>
                                    <p style='font-size: 16px;'><strong>Email:</strong> {toEmail}</p>
                                    <p style='font-size: 16px;'><strong>Password:</strong> {password}</p>
                                    <p style='color: #e74c3c;'><em>Please change your password after logging in.</em></p>
                                </div>
                                <div style='text-align: center; margin-top: 20px;'>
                                    <a href='https://yourwebsite.com/login' 
                                       style='background-color: #248f24; color: #ffffff; padding: 12px 20px; 
                                              text-decoration: none; border-radius: 5px; font-size: 16px; font-weight: bold;'>
                                        Login Now
                                    </a>
                                </div>
                            {ContainerEnd}";

            await SendEmailAsync(toEmail, emailSubject, emailBody);
        }

        public async Task SendPatientRegistrationEmailAsync(GetPatientDTO patient)
        {
            string emailSubject = "Your Account Registration is Successful!";
            string emailBody = $@"
                            {ContainerStart}
                                <h2 style='color: #2c3e50; text-align: center;'>Welcome to MediCare+</h2>
                                <p style='font-size: 16px; color: #333; text-align: center;'>
                                    Your registration has been successfully completed.
                                </p>
                                <div style='background-color: #ffffff; padding: 15px; border-radius: 8px; 
                                            box-shadow: 0px 0px 10px rgba(0, 0, 0, 0.1); text-align: center;'>
                                    <p style='font-size: 16px;'><strong>Your Unique Patient ID:</strong></p>
                                    <p style='background-color: #0073e6; color: #ffffff; padding: 10px 15px; 
                                              display: inline-block; font-size: 18px; font-weight: bold; border-radius: 5px;'>
                                        ID: {patient.AadharNo}
                                    </p>
                                    <p style='color: #333;'><em>Please keep this ID safe for future appointments and records.</em></p>
                                </div>
                                <p style='font-size: 16px; color: #e74c3c; text-align: center; margin-top: 20px;'>
                                    This email is for your reference only.
                                </p>
                                <div style='text-align: center; margin-top: 20px;'>
                                    <a href='mailto:support@medicare.com' 
                                       style='background-color: #248f24; color: #ffffff; padding: 12px 20px; 
                                              text-decoration: none; border-radius: 5px; font-size: 16px; font-weight: bold;'>
                                        Contact Support
                                    </a>
                                </div>
                            {ContainerEnd}";
            await SendEmailAsync(patient.Email, emailSubject, emailBody);
        }

        public async Task SendAppointmentStatusEmailAsync(string patientEmail, string doctorName, Appointment appointment)
        {
            var startTimeFormatted = DateTime.Today.Add(appointment.AppointmentStarts).ToString("hh:mm tt");
            var endTimeFormatted = DateTime.Today.Add(appointment.AppointmentEnds).ToString("hh:mm tt");

            // Define the email subject.
            string emailSubject = "Your Appointment Status Update!";

            // Build the email body using an HTML template.
            string emailBody = $@"
                        {ContainerStart}
                            <h2 style='color: #2c3e50; text-align: center;'>Appointment Status Update</h2>
                            <p style='font-size: 16px; color: #333; text-align: center;'>
                                Your appointment has been <strong>{appointment.Status}</strong>.
                            </p>
                            <div style='background-color: #ffffff; padding: 15px; border-radius: 8px; 
                                        box-shadow: 0px 0px 10px rgba(0, 0, 0, 0.1); text-align: center;'>
                                <p style='font-size: 16px;'><strong>Appointment Details:</strong></p>
                                <p style='font-size: 16px;'><strong>ID:</strong> {appointment.AppointmentId}</p>
                                <p style='font-size: 16px;'><strong>Date:</strong> {appointment.AppointmentDate}</p>
                                <p style='font-size: 16px;'><strong>Start Time:</strong> {startTimeFormatted}</p>
                                <p style='font-size: 16px;'><strong>End Time:</strong> {endTimeFormatted}</p>
                                <p style='font-size: 16px;'><strong>Doctor:</strong> {doctorName}</p>
                            </div>
                            <p style='font-size: 16px; color: #333; text-align: center; margin-top: 20px;'>
                                If you have any questions or require further assistance, please contact our support team.
                            </p>
                            <div style='text-align: center; margin-top: 20px;'>
                                <a href='mailto:support@medicare.com' 
                                   style='background-color:  #248f24; color: #ffffff; padding: 12px 20px; 
                                          text-decoration: none; border-radius: 5px; font-size: 16px; font-weight: bold;'>
                                    Contact Support
                                </a>
                            </div>
                        {ContainerEnd}";

            // Call the common method to send the email.
            await SendEmailAsync(patientEmail, emailSubject, emailBody);
        }

        public async Task SendAppointmentReminderEmailAsync(string patientEmail, string doctorName, GetAppointmentDTO appointment)
        {
            var startTimeFormatted = DateTime.Today.Add(appointment.AppointmentStarts).ToString("hh:mm tt");
            var endTimeFormatted = DateTime.Today.Add(appointment.AppointmentEnds).ToString("hh:mm tt");

            string emailSubject = "Upcoming Appointment Reminder";

            string emailBody = $@"
                {ContainerStart}
                    <h2 style='color: #2c3e50; text-align: center;'>Appointment Reminder</h2>
                    <p style='font-size: 16px; color: #333; text-align: center;'>
                        This is a friendly reminder for your upcoming appointment.
                    </p>
                    <div style='background-color: #ffffff; padding: 15px; border-radius: 8px; 
                                box-shadow: 0px 0px 10px rgba(0, 0, 0, 0.1); text-align: center;'>
                        <p style='font-size: 16px;'><strong>Appointment Details:</strong></p>
                        <p style='font-size: 16px;'><strong>ID:</strong> {appointment.AppointmentId}</p>
                        <p style='font-size: 16px;'><strong>Date:</strong> {appointment.AppointmentDate:dd MMM yyyy}</p>
                        <p style='font-size: 16px;'><strong>Time:</strong> {startTimeFormatted} - {endTimeFormatted}</p>
                        <p style='font-size: 16px;'><strong>Doctor:</strong> {doctorName}</p>
                    </div>
                    <p style='font-size: 16px; color: #333; text-align: center; margin-top: 20px;'>
                        Please arrive at least 10 minutes early. If you need to reschedule or cancel, contact us as soon as possible.
                    </p>
                    <div style='text-align: center; margin-top: 20px;'>
                        <a href='mailto:support@medicare.com' 
                           style='background-color: #007bff; color: #ffffff; padding: 12px 20px; 
                                  text-decoration: none; border-radius: 5px; font-size: 16px; font-weight: bold;'>
                            Contact Support
                        </a>
                    </div>
                {ContainerEnd}";

            await SendEmailAsync(patientEmail, emailSubject, emailBody);
        }

        public async Task SendPatientNotesEmailAsync(string patientEmail, PdfNoteDTO pdfNotesDTO, byte[] PdfBytes)
        {
            string emailSubject = "Your Medical Notes from MediCare+";

            string emailBody = $@"
                        {ContainerStart}
                            <h2 style='color: #2c3e50; text-align: center;'>Your Medical Notes</h2>
                            <p style='font-size: 16px; color: #333; text-align: center;'>
                                Below are your medical notes provided by Dr. {pdfNotesDTO.DoctorName}.
                            </p>
                            <div style='background-color: #ffffff; padding: 15px; border-radius: 8px; 
                                        box-shadow: 0px 0px 10px rgba(0, 0, 0, 0.1); text-align: center;'>
                                <p style='font-size: 16px;'><strong>Patient Name:</strong> {pdfNotesDTO.PatientName}</p>
                                <p style='font-size: 16px;'><strong>Doctor Name:</strong> {pdfNotesDTO.DoctorName}</p>
                                <hr style='border: 1px solid #ddd;'>
                                <p style='font-size: 16px;'><strong>Description:</strong></p>
                                <p style='background-color: #f4f4f4; padding: 10px; border-radius: 5px;'>{pdfNotesDTO.Description}</p>
                                <p style='font-size: 16px;'><strong>Notes:</strong></p>
                                <p style='background-color: #f4f4f4; padding: 10px; border-radius: 5px;'>{pdfNotesDTO.NoteText}</p>
                            </div>
                            <p style='font-size: 16px; color: #333; text-align: center; margin-top: 20px;'>
                                If you have any questions or need further assistance, please contact our support team.
                            </p>
                            <div style='text-align: center; margin-top: 20px;'>
                                <a href='mailto:support@medicare.com' 
                                   style='background-color:  #248f24; color: #ffffff; padding: 12px 20px; 
                                          text-decoration: none; border-radius: 5px; font-size: 16px; font-weight: bold;'>
                                    Contact Support
                                </a>
                            </div>
                        {ContainerEnd}";

            await SendEmailAsync(patientEmail, emailSubject, emailBody, PdfBytes, "Medical_Notes.pdf");
        }

        public async Task SendPatientInvoiceEmailAsync(string patientEmail, PdfNoteDTO pdfNotesDTO, byte[] PdfBytes)
        {
            string emailSubject = "Your Invoice from MediCare+";

            string emailBody = $@"
        {ContainerStart}
            <h2 style='color: #2c3e50; text-align: center;'>Your Invoice</h2>
            <p style='font-size: 16px; color: #333; text-align: center;'>
                Below is your invoice provided by Dr. {pdfNotesDTO.DoctorName}.
            </p>
            <div style='background-color: #ffffff; padding: 15px; border-radius: 8px; 
                        box-shadow: 0px 0px 10px rgba(0, 0, 0, 0.1); text-align: center;'>
                <p style='font-size: 16px;'><strong>Patient Name:</strong> {pdfNotesDTO.PatientName}</p>
                <p style='font-size: 16px;'><strong>Doctor Name:</strong> {pdfNotesDTO.DoctorName}</p>
                <hr style='border: 1px solid #ddd;'>
                <p style='font-size: 16px;'><strong>Description:</strong></p>
                <p style='background-color: #f4f4f4; padding: 10px; border-radius: 5px;'>{pdfNotesDTO.Description}</p>
                <p style='font-size: 16px;'><strong>Note:</strong></p>
                <p style='background-color: #f4f4f4; padding: 10px; border-radius: 5px;'>{pdfNotesDTO.NoteText}</p>
            </div>
            <p style='font-size: 16px; color: #333; text-align: center; margin-top: 20px;'>
                Your invoice is attached as a PDF document.
            </p>
            <div style='text-align: center; margin-top: 20px;'>
                <a href='mailto:support@medicare.com' 
                   style='background-color: #248f24; color: #ffffff; padding: 12px 20px; 
                          text-decoration: none; border-radius: 5px; font-size: 16px; font-weight: bold;'>
                    Contact Support
                </a>
            </div>
        {ContainerEnd}";

            await SendEmailAsync(patientEmail, emailSubject, emailBody, PdfBytes, "Invoice.pdf");
        }

    }
}