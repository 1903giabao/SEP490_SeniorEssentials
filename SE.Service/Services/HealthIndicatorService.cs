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
using SE.Common.Enums;

namespace SE.Service.Services
{
    public interface IHealthIndicatorService
    {
        Task<IBusinessResult> CreateHealthIndicator(CreateHealthIndicatorRequest request);
        Task<IBusinessResult> GetAllHealthIndicatorsByElderlyId(int elderlyId);

        Task<IBusinessResult> GetHealthIndicatorById(int healthIndicatorId);
        Task<IBusinessResult> UpdateHealthIndicator(UpdateHealthIndicatorRequest request);
        Task<IBusinessResult> UpdateHealthIndicatorStatus(int HealthIndicatorId);
    }

    public class HealthIndicatorService : IHealthIndicatorService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public HealthIndicatorService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IBusinessResult> CreateHealthIndicator(CreateHealthIndicatorRequest request)
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

                var healthIndicator = _mapper.Map<HealthIndicator>(request);
                healthIndicator.DateRecorded = request.DateRecorded; 
                healthIndicator.Status = request.Status ?? "Active"; 

                await _unitOfWork.HealthIndicatorRepository.CreateAsync(healthIndicator);

                return new BusinessResult(Const.SUCCESS_CREATE, "Health indicator created successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }


        public async Task<IBusinessResult> GetAllHealthIndicatorsByElderlyId(int elderlyId)
        {
            try
            {
                // Validate the input
                if (elderlyId <= 0)
                {
                    return new BusinessResult(Const.FAIL_READ, "Invalid elderly ID.");
                }

                var healthIndicators = await _unitOfWork.HealthIndicatorRepository.GetByElderlyIdAsync(elderlyId);

                // Map the health indicators to CreateHealthIndicatorRequest
                var healthIndicatorRequests = _mapper.Map<List<CreateHealthIndicatorRequest>>(healthIndicators);

                return new BusinessResult(Const.SUCCESS_READ, "Health indicators retrieved successfully.", healthIndicatorRequests);
            }
            catch (Exception ex)
            {
                // Log the exception (logging not shown here)
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetHealthIndicatorById(int healthIndicatorId)
        {
            try
            {
                var healthIndicator = await _unitOfWork.HealthIndicatorRepository.GetByIdAsync(healthIndicatorId);
                if (healthIndicator == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "Health indicator not found.");
                }

                // Map the health indicator to CreateHealthIndicatorRequest
                var healthIndicatorRequest = _mapper.Map<CreateHealthIndicatorRequest>(healthIndicator);

                return new BusinessResult(Const.SUCCESS_READ, "Health indicator retrieved successfully.", healthIndicatorRequest);
            }
            catch (Exception ex)
            {
                // Log the exception (logging not shown here)
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> UpdateHealthIndicator(UpdateHealthIndicatorRequest request)
        {
            try
            {
                if (request == null)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Request cannot be null.");
                }

                var healthIndicator = await _unitOfWork.HealthIndicatorRepository.GetByIdAsync(request.HealthIndicatorId);
                if (healthIndicator == null)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Health indicator not found.");
                }

                _mapper.Map(request, healthIndicator);

                await _unitOfWork.HealthIndicatorRepository.UpdateAsync(healthIndicator);

                return new BusinessResult(Const.SUCCESS_UPDATE, "Health indicator updated successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> UpdateHealthIndicatorStatus(int HealthIndicatorId)
        {
            try
            {
                // Validate the input
                if (HealthIndicatorId == null)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Request cannot be null.");
                }

                var healthIndicator = await _unitOfWork.HealthIndicatorRepository.GetByIdAsync(HealthIndicatorId);
                if (healthIndicator == null)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Health indicator not found.");
                }

                // Update the status
                healthIndicator.Status = SD.GeneralStatus.INACTIVE;

                // Update the health indicator in the repository
                await _unitOfWork.HealthIndicatorRepository.UpdateAsync(healthIndicator);

                return new BusinessResult(Const.SUCCESS_UPDATE, "Health indicator status updated successfully.");
            }
            catch (Exception ex)
            {
                // Log the exception (logging not shown here)
                return new BusinessResult(Const.FAIL_UPDATE, "An unexpected error occurred: " + ex.Message);
            }
        }
    }
}
