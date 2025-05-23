﻿using MediCare_MVC_Project.MediCare.Application.DTOs.LabTestManagement;

namespace MediCare_MVC_Project.MediCare.Application.Interfaces.LabTestManagement
{
    public interface IPatientTestService
    {
        Task<ICollection<GetPatientTestDTO>> GetAllPatientTestAsync();
        Task AddPatientTestAsync(PatientTestDTO patientTest);
        Task DeletePatientTestAsync(int patientTestId);
        Task UpdatePatientTestAsync(int patientTestId, DateOnly date, string result);
        Task<ICollection<GetPatientTestDTO>> GetPatientTestByDoctorAsync(int doctorId);
    }
}