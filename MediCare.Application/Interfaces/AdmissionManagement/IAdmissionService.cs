﻿using MediCare_MVC_Project.MediCare.Application.DTOs.AdmissionDTOs;

namespace MediCare_MVC_Project.MediCare.Application.Interfaces.AdmissionManagement
{
    public interface IAdmissionService
    {
        Task<ICollection<GetAdmissionDTO>> GetAllAdmissionRecordsAsync();
        Task AddAdmissionRecordsAsync(AdmissionDTO admission);
        Task DeleteAdmissionRecordAsync(int AdmissionId);
        Task UpdateAdmissionRecordAsync(int AdmissionId, AdmissionUpdateDTO admission);
        Task<AdmissionUpdateDTO> GetAdmissionRecordsByIdAsync(int id);
        Task<ICollection<GetAdmissionDTO>> GetAllAdmissionByDoctorAsync(int id);
    }
}