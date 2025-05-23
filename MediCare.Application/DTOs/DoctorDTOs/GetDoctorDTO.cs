﻿namespace MediCare_MVC_Project.MediCare.Application.DTOs.DoctorDTOs
{
    public class GetDoctorDTO
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string MobileNo { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string Specialization { get; set; }
        public string Qualification { get; set; }
        public string LicenseNumber { get; set; }
        public bool Status { get; set; }
    }
}