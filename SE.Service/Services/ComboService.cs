using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using SE.Common;
using SE.Common.DTO;
using SE.Common.Enums;
using SE.Data.Models;
using SE.Data.Repository;
using SE.Data.UnitOfWork;
using SE.Service.Base;

namespace SE.Service.Services
{
    public interface IComboService
    {
        Task<IBusinessResult> CreateCombo(CreateComboModel req);
        Task<IBusinessResult> UpdateCombo(int comboId, CreateComboModel req);
        Task<IBusinessResult> GetAllCombos();
        Task<IBusinessResult> GetComboById(int comboId);
        Task<IBusinessResult> UpdateComboStatus(int comboId);

    }

    public class ComboService : IComboService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ComboService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IBusinessResult> CreateCombo(CreateComboModel req)
        {
            try
            {
                if (req.Fee <= 0)
                {
                    return new BusinessResult(Const.FAIL_READ, "FEE MUST > 0");
                }            

                if (string.IsNullOrWhiteSpace(req.Name))
                {
                    return new BusinessResult(Const.FAIL_READ, "NAME MUST NOT BE EMPTY");
                }

                if (string.IsNullOrWhiteSpace(req.Description))
                {
                    return new BusinessResult(Const.FAIL_READ, "DESCRIPTION MUST NOT BE EMPTY");
                }

                var combo = _mapper.Map<Subscription>(req);

                combo.CreatedDate = DateTime.UtcNow.AddHours(7);
                combo.UpdatedDate = DateTime.UtcNow.AddHours(7);

                var result = await _unitOfWork.SubscriptionRepository.CreateAsync(combo);

                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, req);
                }

                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, ex.Message);
            }
        }

        public async Task<IBusinessResult> UpdateCombo(int comboId, CreateComboModel req)
        {
            try
            {
                if (req.Fee <= 0)
                {
                    return new BusinessResult(Const.FAIL_READ, "FEE MUST > 0");
                }

                if (string.IsNullOrWhiteSpace(req.Name))
                {
                    return new BusinessResult(Const.FAIL_READ, "NAME MUST NOT BE EMPTY");
                }

                if (string.IsNullOrWhiteSpace(req.Description))
                {
                    return new BusinessResult(Const.FAIL_READ, "DESCRIPTION MUST NOT BE EMPTY");
                }

                var combo = await _unitOfWork.SubscriptionRepository.GetByIdAsync(comboId);

                if (combo == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "CANNOT FIND COMBO");
                }

                combo.Name = req.Name;
                combo.Description = req.Description;
                combo.Fee = req.Fee;
                combo.UpdatedDate = DateTime.UtcNow.AddHours(7);

                var result = await _unitOfWork.SubscriptionRepository.UpdateAsync(combo);

                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, req);
                }

                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, ex.Message);
            }
        }

        public async Task<IBusinessResult> GetAllCombos()
        {
            try
            {
                var combos = await _unitOfWork.SubscriptionRepository.GetAllAsync(); 
                var comboDtos = _mapper.Map<List<ComboDto>>(combos); 

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, comboDtos);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, ex.Message);
            }
        }

        public async Task<IBusinessResult> GetComboById(int comboId)
        {
            try
            {
                if (comboId <= 0)
                {
                    return new BusinessResult(Const.FAIL_READ, "Invalid combo ID.");
                }

                var combo = await _unitOfWork.SubscriptionRepository.GetByIdAsync(comboId);
                if (combo == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "Combo not found.");
                }

                var comboDto = _mapper.Map<ComboDto>(combo);

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, comboDto);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, ex.Message);
            }
        }


        public async Task<IBusinessResult> UpdateComboStatus(int comboId)
        {
            try
            {
                var checkComboExisted = _unitOfWork.SubscriptionRepository.FindByCondition(c => c.SubscriptionId == comboId).FirstOrDefault();

                if (checkComboExisted == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "Combo not found.");
                }

                checkComboExisted.Status = SD.GeneralStatus.INACTIVE;

                var result = await _unitOfWork.SubscriptionRepository.UpdateAsync(checkComboExisted);

                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_UPDATE, "Combo status updated successfully.");
                }

                return new BusinessResult(Const.FAIL_UPDATE, "Failed to update combo status.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, ex.Message);
            }
        }

    }
}
