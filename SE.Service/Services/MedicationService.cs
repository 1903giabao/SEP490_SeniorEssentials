using AutoMapper;
using SE.Common;
using SE.Common.DTO;
using SE.Common.Request;
using SE.Data.Models;
using SE.Data.UnitOfWork;
using SE.Service.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Service.Services
{
    public interface IMedicationService
    {

    }

    public class MedicationService : IMedicationService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MedicationService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IBusinessResult> CreateMedication(CreateMedicationRequest req)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(req.MedicationName))
                {
                    return new BusinessResult(Const.FAIL_READ, "MEDICATION NAME MUST NOT BE EMPTY");
                }

                if (req.StartDate > req.EndDate)
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "THỜI GIAN BẮT ĐẦU PHẢI TRƯỚC THỜI GIAN KẾT THÚC!");
                }

                var medication = _mapper.Map<Medication>(req);
                medication.CreatedDate = DateTime.UtcNow;

                var result = await _unitOfWork.MedicationRepository.CreateAsync(medication);
                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE, "Medication created successfully.", req);
                }

                return new BusinessResult(Const.FAIL_CREATE, "Failed to create medication.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, ex.Message);
            }
        }

        public async Task<IBusinessResult> UpdateMedication(int medicationId, UpdateMedicationRequest req)
        {
            try
            {
                var medication = await _unitOfWork.MedicationRepository.GetByIdAsync(medicationId);
                if (medication == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "CANNOT FIND MEDICATION");
                }

                medication.MedicationName = req.MedicationName;
                medication.Treatment = req.Treatment;
                medication.Shape = req.Shape;
                medication.Dosage = req.Dosage;
                medication.IsBeforeMeal = req.IsBeforeMeal;
                medication.FrequencyType = req.FrequencyType;
                medication.TimeFrequency = req.TimeFrequency;
                medication.DateFrequency = req.DateFrequency;
                medication.StartDate = req.StartDate;
                medication.EndDate = req.EndDate;
                medication.Note = req.Note;

                var result = await _unitOfWork.MedicationRepository.UpdateAsync(medication);
                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_UPDATE, "Medication updated successfully.", req);
                }

                return new BusinessResult(Const.FAIL_UPDATE, "Failed to update medication.");

            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, ex.Message);
            }
        }

        public async Task<IBusinessResult> GetAllMedications()
        {
            try
            {
                var medications = await _unitOfWork.MedicationRepository.GetAllAsync();
                var medicationDtos = _mapper.Map<List<MedicationModel>>(medications);

                return new BusinessResult(Const.SUCCESS_READ, "Medications retrieved successfully.", medicationDtos);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, ex.Message);
            }
        }

        public async Task<IBusinessResult> GetMedicationById(int medicationId)
        {
            try
            {
                if (medicationId <= 0)
                {
                    return new BusinessResult(Const.FAIL_READ, "Invalid medication ID.");
                }

                var medication = await _unitOfWork.MedicationRepository.GetByIdAsync(medicationId);
                if (medication == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "Medication not found.");
                }

                var medicationDto = _mapper.Map<MedicationModel>(medication);
                return new BusinessResult(Const.SUCCESS_READ, "Medication retrieved successfully.", medicationDto);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, ex.Message);
            }
        }

        public async Task<IBusinessResult> UpdateMedicationStatus(int medicationId, string status)
        {
            try
            {
                var medication = await _unitOfWork.MedicationRepository.GetByIdAsync(medicationId);
                if (medication == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "Medication not found.");
                }

                medication.Status = status; 

                var result = await _unitOfWork.MedicationRepository.UpdateAsync(medication);
                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_UPDATE, "Medication status updated successfully.");
                }

                return new BusinessResult(Const.FAIL_UPDATE, "Failed to update medication status.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, ex.Message);
            }
        }
    }
}

