﻿using MediCare_MVC_Project.MediCare.Application.DTOs.AdmissionDTOs;

namespace MediCare_MVC_Project.MediCare.Application.Interfaces.AdmissionManagement
{
    public interface IBedRepository
    {
        Task<ICollection<GetBedDTO>> GetAllBedsQuery();
        Task AddNewBedQuery(BedDTO bed);
        Task DeleteBedQuery(int BedId);
        Task UpdateBedQuery(int BedId, int bedNo, int roomNo, string RoomType, bool IsOccupied);
    }
}