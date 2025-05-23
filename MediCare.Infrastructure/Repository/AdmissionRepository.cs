﻿using MediCare_MVC_Project.MediCare.Application.DTOs.AdmissionDTOs;
using MediCare_MVC_Project.MediCare.Application.Interfaces.AdmissionManagement;
using MediCare_MVC_Project.MediCare.Domain.Entity;
using MediCare_MVC_Project.MediCare.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace MediCare_MVC_Project.MediCare.Infrastructure.Repository
{
    public class AdmissionRepository : GenericRepository<PatientAdmission>, IAdmissionRepository
    {
        public AdmissionRepository(ApplicationDBContext context) : base(context)
        {
            
        }

        public async Task AddAdmissionRecordsQuery(AdmissionDTO admission)
        {
            var existingRecords = await _context.PatientAdmissions.Include(s => s.Patient).FirstOrDefaultAsync(p => p.Patient.AadharNo == admission.AadharNo && p.AdmissionDate == admission.AdmissionDate && p.BedId == admission.BedId);

            if (existingRecords != null)
                throw new Exception("Records already exists");

            var patientRecord = await _context.Patients.FirstOrDefaultAsync(s => s.AadharNo == admission.AadharNo);

            var newRecords = new PatientAdmission
            {
                PatientId = patientRecord.PatientId,
                BedId = admission.BedId,
                AdmissionDate = admission.AdmissionDate,
                DischargeDate = admission.DischargeDate,
                IsDischarged = admission.IsDischarged,
                CreatedAt = DateTime.UtcNow
            };

            var existingBed = await _context.Beds.FindAsync(newRecords.BedId);

            existingBed.IsOccupied = true;

            _context.Beds.Update(existingBed);

            var appointment = await _context.Appointments
             .FirstOrDefaultAsync(a => a.PatientId == patientRecord.PatientId);

            appointment.Status = "Completed";
            appointment.UpdatedAt = DateTime.UtcNow;

            _context.Appointments.Update(appointment);

            _context.PatientAdmissions.Add(newRecords);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAdmissionRecordQuery(int AdmissionId)
        {
            var existingRecord = await _context.PatientAdmissions.FindAsync(AdmissionId);
            if (existingRecord == null)
                throw new Exception("No Record found.");

            _context.PatientAdmissions.Remove(existingRecord);

            var relieveBed = await _context.Beds.FindAsync(existingRecord.BedId);
            relieveBed.IsOccupied = false;

            await _context.SaveChangesAsync();
        }

        public async Task<AdmissionUpdateDTO> GetAdmissionRecordsByIdQuery(int id)
        {
            var existingRecords = await _context.PatientAdmissions.Where(s => s.AdmissionId == id)
                                                                  .Select(s => new AdmissionUpdateDTO
                                                                  {
                                                                      BedId = s.BedId,
                                                                      AdmissionDate = s.AdmissionDate,
                                                                      DischargeDate = s.DischargeDate,
                                                                      IsDischarged = s.IsDischarged
                                                                  }).FirstOrDefaultAsync();

            if (existingRecords == null)
                throw new Exception($"No Records found with {id}");

            return existingRecords;
        }

        public async Task<ICollection<GetAdmissionDTO>> GetAllAdmissionByDoctorQuery(int id)
        {
            //var existingPatientList = await _context.Appointments.Where(s => s.DoctorId == id)
            //                                                     .Include(p => p.Patient)
            //                                                        .ThenInclude(p => p.PatientAdmission)
            //                                                        .ThenInclude(p => p.Patient.PatientAdmission.Bed.Room)
            //                                                        .Select(s => new GetAdmissionDTO
            //                                                        {
            //                                                            AdmissionId = s.Patient.PatientAdmission.AdmissionId,
            //                                                            FirstName = s.Patient.FirstName,
            //                                                            LastName = s.Patient.LastName,
            //                                                            MobileNo = s.Patient.MobileNo,
            //                                                            Email = s.Patient.Email,
            //                                                            RoomNo = s.Patient.PatientAdmission.Bed.Room.RoomNumber,
            //                                                            BedNo = s.Patient.PatientAdmission.Bed.BedNumber,
            //                                                            RoomType = s.Patient.PatientAdmission.Bed.Room.RoomType,
            //                                                            AdmissionDate = s.Patient.PatientAdmission.AdmissionDate,
            //                                                            DischargeDate = s.Patient.PatientAdmission.DischargeDate
            //                                                        }).ToListAsync();

            var existingPatientList = await _context.Appointments
    .Where(s => s.DoctorId == id &&
                s.Patient != null &&
                s.Patient.PatientAdmission != null &&
                s.Patient.PatientAdmission.Bed != null &&
                s.Patient.PatientAdmission.Bed.Room != null)
    .Include(p => p.Patient)
        .ThenInclude(p => p.PatientAdmission)
            .ThenInclude(pa => pa.Bed)
                .ThenInclude(b => b.Room)
    .Select(s => new GetAdmissionDTO
    {
        AdmissionId = s.Patient.PatientAdmission.AdmissionId,
        FirstName = s.Patient.FirstName,
        LastName = s.Patient.LastName,
        MobileNo = s.Patient.MobileNo,
        Email = s.Patient.Email,
        RoomNo = s.Patient.PatientAdmission.Bed.Room.RoomNumber,
        BedNo = s.Patient.PatientAdmission.Bed.BedNumber,
        RoomType = s.Patient.PatientAdmission.Bed.Room.RoomType,
        AdmissionDate = s.Patient.PatientAdmission.AdmissionDate,
        DischargeDate = s.Patient.PatientAdmission.DischargeDate
    }).ToListAsync();


            if (existingPatientList == null)
                throw new Exception("No Data found.");

            return existingPatientList;
        }

        public async Task<ICollection<GetAdmissionDTO>> GetAllAdmissionRecordsQuery()
        {
            var recordsList = await _context.PatientAdmissions.Include(p => p.Patient)
                                                              .Include(p => p.Bed)
                                                              .ThenInclude(p => p.Room)
                                                              .Select(s => new GetAdmissionDTO
                                                              {
                                                                  AdmissionId = s.AdmissionId,
                                                                  FirstName = s.Patient.FirstName,
                                                                  LastName = s.Patient.LastName,
                                                                  MobileNo = s.Patient.MobileNo,
                                                                  Email = s.Patient.Email,
                                                                  RoomType = s.Bed.Room.RoomType,
                                                                  RoomNo = s.Bed.Room.RoomNumber,
                                                                  BedNo = s.Bed.BedNumber,
                                                                  AdmissionDate = s.AdmissionDate,
                                                                  DischargeDate = s.DischargeDate
                                                              }).ToListAsync();

            if (recordsList == null)
                throw new Exception("No Records found.");

            return recordsList;
        }

        public async Task UpdateAdmissionRecordQuery(int AdmissionId, AdmissionUpdateDTO admission)
        {
            var existingRecords = await _context.PatientAdmissions.Include(s => s.Bed).FirstOrDefaultAsync(s => s.AdmissionId == AdmissionId);

            if (existingRecords == null)
                throw new Exception($"No records found with {AdmissionId}");

            existingRecords.BedId = admission.BedId;
            existingRecords.AdmissionDate = admission.AdmissionDate;
            existingRecords.DischargeDate = admission.DischargeDate;
            existingRecords.IsDischarged = admission.IsDischarged;

            _context.PatientAdmissions.Update(existingRecords);
            await _context.SaveChangesAsync();
        }
    }
}