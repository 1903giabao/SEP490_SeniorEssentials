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
                // Map the request to the Combo entity
                var combo = _mapper.Map<Combo>(req);

                // Set CreatedDate and UpdatedDate
                combo.CreatedDate = DateTime.UtcNow;
                combo.UpdatedDate = DateTime.UtcNow;

                // Use the repository to create the new Combo
                var result = await _unitOfWork.ComboRepository.CreateAsync(combo);

                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, req);
                }

                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here)
                return new BusinessResult(Const.FAIL_CREATE, ex.Message);
            }
        }


        public async Task<IBusinessResult> GetAllCombos()
        {
            try
            {
                var combos = await _unitOfWork.ComboRepository.GetAllAsync(); 
                var comboDtos = _mapper.Map<List<ComboDto>>(combos); 

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, comboDtos);
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here)
                return new BusinessResult(Const.FAIL_READ, ex.Message);
            }
        }

        // Method to get a combo by ID
        public async Task<IBusinessResult> GetComboById(int comboId)
        {
            try
            {
                if (comboId <= 0)
                {
                    return new BusinessResult(Const.FAIL_READ, "Invalid combo ID.");
                }

                var combo = await _unitOfWork.ComboRepository.GetByIdAsync(comboId); // Assuming this method exists
                if (combo == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "Combo not found.");
                }

                var comboDto = _mapper.Map<ComboDto>(combo); // Map to DTO

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, comboDto);
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here)
                return new BusinessResult(Const.FAIL_READ, ex.Message);
            }
        }


        public async Task<IBusinessResult> UpdateComboStatus(int comboId)
        {
            try
            {
                // Check if the combo exists
                var checkComboExisted = _unitOfWork.ComboRepository.FindByCondition(c => c.ComboId == comboId).FirstOrDefault();

                if (checkComboExisted == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "Combo not found.");
                }

                // Update the status to INACTIVE
                checkComboExisted.Status = SD.GeneralStatus.INACTIVE;

                // Update the combo in the repository
                var result = await _unitOfWork.ComboRepository.UpdateAsync(checkComboExisted);

                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_UPDATE, "Combo status updated successfully.");
                }

                return new BusinessResult(Const.FAIL_UPDATE, "Failed to update combo status.");
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here)
                return new BusinessResult(Const.FAIL_UPDATE, ex.Message);
            }
        }

    }
}
