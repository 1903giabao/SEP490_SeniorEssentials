using AutoMapper;
using SE.Common.Request;
using SE.Common;
using SE.Data.Models;
using SE.Data.UnitOfWork;
using SE.Service.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SE.Common.DTO;
using SE.Common.Enums;

namespace SE.Service.Services
{
    public interface IFamilyTieService
    {
        Task<IBusinessResult> CreateFamilyTie(CreateFamilyTieRequest request);
        Task<IBusinessResult> GetAllFamilyTiesByElderlyId(int elderlyId);
        Task<IBusinessResult> UpdateFamilyTieNote(int familyFamilyTieId, string n);
        Task<IBusinessResult> UpdateFamilyTieStatus(int familyFamilyTieId);

    }

    public class FamilyTieService : IFamilyTieService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public FamilyTieService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<IBusinessResult> CreateFamilyTie(CreateFamilyTieRequest request)
        {
            try
            {
                if (request == null)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Request cannot be null.");
                }

                if (request.ElderlyId <= 0)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Invalid elderly ID.");
                }

                if (request.FamilyMemberId <= 0)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Invalid family member ID.");
                }

                var familyTie = _mapper.Map<FamilyTie>(request);

                await _unitOfWork.FamilyTieRepository.CreateAsync(familyTie);

                return new BusinessResult(Const.SUCCESS_CREATE, "Family tie created successfully.");
            }

            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetAllFamilyTiesByElderlyId(int elderlyId)
        {
            try
            {
                if (elderlyId <= 0)
                {
                    return new BusinessResult(Const.FAIL_READ, "Invalid elderly ID.");
                }

                var familyTies = _unitOfWork.FamilyTieRepository
                    .FindByCondition(ft => ft.ElderlyId == elderlyId)
                    .ToList();

                if (familyTies == null || !familyTies.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, "No family ties found for the given elderly ID.");
                }

                var result = _mapper.Map<List<FamilyTieDTO>>(familyTies);

                return new BusinessResult(Const.SUCCESS_READ, "Family ties retrieved successfully.", result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }


        public async Task<IBusinessResult> UpdateFamilyTieNote(int familyFamilyTieId, string note)
        {
            try
            {
                if (familyFamilyTieId <= 0)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Invalid family tie ID.");
                }


                var familyTie = _unitOfWork.FamilyTieRepository
                    .FindByCondition(ft => ft.FamilyFamilyTieId == familyFamilyTieId)
                    .FirstOrDefault();

                if (familyTie == null)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Family tie not found.");
                }

                familyTie.Note = note;

                await _unitOfWork.FamilyTieRepository.UpdateAsync(familyTie);

                return new BusinessResult(Const.SUCCESS_UPDATE, "Family tie note updated successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> UpdateFamilyTieStatus(int familyFamilyTieId)
        {
            try
            {
                if (familyFamilyTieId <= 0)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Invalid family tie ID.");
                }

                var familyTie = _unitOfWork.FamilyTieRepository
                    .FindByCondition(ft => ft.FamilyFamilyTieId == familyFamilyTieId)
                    .FirstOrDefault();

                if (familyTie == null)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Family tie not found.");
                }

                familyTie.Status = SD.GeneralStatus.INACTIVE;

                await _unitOfWork.FamilyTieRepository.UpdateAsync(familyTie);

                return new BusinessResult(Const.SUCCESS_UPDATE, "Family tie note updated successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, "An unexpected error occurred: " + ex.Message);
            }
        }

    }
}
