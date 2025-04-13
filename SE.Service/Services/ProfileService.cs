using AutoMapper;
using SE.Common.DTO.Emergency;
using SE.Common;
using SE.Data.UnitOfWork;
using SE.Service.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SE.Common.DTO;
using SE.Service.Helper;
using SE.Common.Request.Account;
using SE.Common.Response.Profile;

namespace SE.Service.Services
{
    public interface IProfileService
    {
        Task<IBusinessResult> GetDetailProfile(int accountId);
        Task<IBusinessResult> EditDetailProfile(EditProfileRequest req);
        Task<IBusinessResult> GetElderlyProfile(int elderlyId);
        Task<IBusinessResult> EditElderlyProfile(EditElderlyProfile req);

    }

    public class ProfileService : IProfileService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProfileService (UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IBusinessResult> GetDetailProfile(int accountId)
        {
            try
            {
                var account = await _unitOfWork.AccountRepository.GetAccountAsync(accountId);

                if (account == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG);
                }

                var result = _mapper.Map<GetDetailProfile>(account);

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, $"An unexpected error occurred: {ex.Message}");
            }
        }        
        
        public async Task<IBusinessResult> EditDetailProfile(EditProfileRequest req)
        {
            try
            {
                var account = await _unitOfWork.AccountRepository.GetAccountAsync(req.AccountId);

                if (account == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist");
                }

                if (req.Avatar != null)
                {
                    var avatar = ("", "");
                    avatar = await CloudinaryHelper.UploadImageAsync(req.Avatar);
                    account.Avatar = avatar.Item2;
                }

                account.FullName = req.FullName;
                account.Gender = req.Gender;
                account.DateOfBirth = req.Dob;

                var updateResult = await _unitOfWork.AccountRepository.UpdateAsync(account);
                
                if (updateResult < 1)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                }

                return new BusinessResult(Const.SUCCESS_UPDATE, Const.SUCCESS_UPDATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, $"An unexpected error occurred: {ex.Message}");
            }
        }

        public async Task<IBusinessResult> GetElderlyProfile(int elderlyId)
        {
            try
            {
                var account = await _unitOfWork.AccountRepository.GetAccountAsync(elderlyId);

                if (account == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist");
                }

                var elderly = await _unitOfWork.ElderlyRepository.GetAccountByElderlyId(account.Elderly.ElderlyId);

                if (elderly == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist");
                }

                var result = _mapper.Map<GetElderlyProfile>(elderly);

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, $"An unexpected error occurred: {ex.Message}", new List<int>());
            }
        }

        public async Task<IBusinessResult> EditElderlyProfile(EditElderlyProfile req)
        {
            try
            {
                var account = await _unitOfWork.AccountRepository.GetAccountAsync(req.ElderlyId);

                if (account == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist");
                }

                var elderly = await _unitOfWork.ElderlyRepository.GetAccountByElderlyId(account.Elderly.ElderlyId);

                if (elderly == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist");
                }

                elderly.Allergy = req.Allergy;
                elderly.LivingSituation = req.LivingSituation;

                string medicalRecordsPassage = string.Join(".", req.MedicalRecord);

                elderly.MedicalRecord = medicalRecordsPassage;

                var updateResult = await _unitOfWork.ElderlyRepository.UpdateAsync(elderly);

                if (updateResult < 1)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                }

                return new BusinessResult(Const.SUCCESS_UPDATE, Const.SUCCESS_UPDATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, $"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}
