﻿using MediCare_MVC_Project.MediCare.Application.DTOs.AdmissionDTOs;

namespace MediCare_MVC_Project.MediCare.Application.Interfaces.AdmissionManagement
{
    public interface IBedService
    {
        Task<ICollection<GetBedDTO>> GetAllBedsAsync();
        Task AddNewBedAsync(BedDTO bed);
        Task DeleteBedAsync(int BedId);
        Task UpdateBedAsync(int BedId, int bedNo, int roomNo, string RoomType, bool IsOccupied);
    }
}
