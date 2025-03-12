using AutoMapper;
using SE.Common.Request;
using SE.Common.Request.HealthIndicator;
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
using SE.Common.DTO.HealthIndicator;
using Firebase.Auth;
using Microsoft.Identity.Client;
using SE.Common.Response;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Google.Type;

namespace SE.Service.Services
{
    public interface IHealthIndicatorService
    {
        // Create methods
        Task<IBusinessResult> CreateWeight(CreateWeightRequest request);
        Task<IBusinessResult> CreateHeight(CreateHeightRequest request);
        Task<IBusinessResult> CreateBloodPressure(CreateBloodPressureRequest request);
        Task<IBusinessResult> CreateHeartRate(CreateHeartRateRequest request);
        Task<IBusinessResult> CreateBloodGlucose(CreateBloodGlucoseRequest request);
        Task<IBusinessResult> CreateLipidProfile(CreateLipidProfileRequest request);
        Task<IBusinessResult> CreateLiverEnzymes(CreateLiverEnzymesRequest request);
        Task<IBusinessResult> CreateKidneyFunction(CreateKidneyFunctionRequest request);

        // Get methods by id
        Task<IBusinessResult> GetHeightById(int id);
        Task<IBusinessResult> GetWeightById(int id);
        Task<IBusinessResult> GetBloodPressureById(int id);
        Task<IBusinessResult> GetHeartRateById(int id);
        Task<IBusinessResult> GetBloodGlucoseById(int id);
        Task<IBusinessResult> GetLipidProfileById(int id);
        Task<IBusinessResult> GetLiverEnzymesById(int id);
        Task<IBusinessResult> GetKidneyFunctionById(int id);


        // Update status methods for each health indicator
        Task<IBusinessResult> UpdateWeightStatus(int weightId, string status);
        Task<IBusinessResult> UpdateHeightStatus(int heightId, string status);
        Task<IBusinessResult> UpdateBloodPressureStatus(int bloodPressureId, string status);
        Task<IBusinessResult> UpdateHeartRateStatus(int heartRateId, string status);
        Task<IBusinessResult> UpdateBloodGlucoseStatus(int bloodGlucoseId, string status);
        Task<IBusinessResult> UpdateLipidProfileStatus(int lipidProfileId, string status);
        Task<IBusinessResult> UpdateLiverEnzymesStatus(int liverEnzymesId, string status);
        Task<IBusinessResult> UpdateKidneyFunctionStatus(int kidneyFunctionId, string status);

        Task<IBusinessResult> GetWeightDetail(int accountId);

        //     Task<IBusinessResult> GetAllHealthIndicatorsByElderlyId(int elderlyId, string filter = null);
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

        public async Task<IBusinessResult> CreateWeight(CreateWeightRequest request)
        {
            try
            {
                var isExisted = await _unitOfWork.ElderlyRepository.GetByIdAsync(request.ElderlyId);

                if (isExisted == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                if (request.Weight <= 0)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Weight must be greater than 0.");
                }

                var weightEntity = _mapper.Map<Weight>(request);
                weightEntity.DateRecorded = System.DateTime.UtcNow.AddHours(7);
                weightEntity.Status = SD.GeneralStatus.ACTIVE;

                await _unitOfWork.WeightRepository.CreateAsync(weightEntity);

                return new BusinessResult(Const.SUCCESS_CREATE, "Weight created successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> CreateHeight(CreateHeightRequest request)
        {
            try
            {
                var isExisted = await _unitOfWork.ElderlyRepository.GetByIdAsync(request.ElderlyId);

                if (isExisted == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                if (request.Height <= 0)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Height must be greater than 0.");
                }

                var heightEntity = _mapper.Map<Height>(request);
                heightEntity.DateRecorded = System.DateTime.UtcNow.AddHours(7);
                heightEntity.Status = SD.GeneralStatus.ACTIVE;

                await _unitOfWork.HeightRepository.CreateAsync(heightEntity);

                return new BusinessResult(Const.SUCCESS_CREATE, "Height created successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> CreateBloodPressure(CreateBloodPressureRequest request)
        {
            try
            {
                var isExisted = await _unitOfWork.ElderlyRepository.GetByIdAsync(request.ElderlyId);

                if (isExisted == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                if (request.BloodPressureSystolic < 30 || request.BloodPressureSystolic > 300)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Systolic blood pressure must be in 30~300!");
                }

                if (request.BloodPressureDiastolic < 20 || request.BloodPressureDiastolic > 250)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Diastolic blood pressure must be in 20~250!");
                }

                var bloodPressure = _mapper.Map<BloodPressure>(request);
                bloodPressure.DateRecorded = System.DateTime.UtcNow.AddHours(7);
                bloodPressure.Status = SD.GeneralStatus.ACTIVE;

                await _unitOfWork.BloodPressureRepository.CreateAsync(bloodPressure);

                return new BusinessResult(Const.SUCCESS_CREATE, "Blood Pressure created successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> CreateHeartRate(CreateHeartRateRequest request)
        {
            try
            {
                var isExisted = await _unitOfWork.ElderlyRepository.GetByIdAsync(request.ElderlyId);

                if (isExisted == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                if (request.HeartRate < 40 || request.HeartRate > 300)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Heart rate must be in 40~300!");
                }

                var heartRate = _mapper.Map<HeartRate>(request);
                heartRate.DateRecorded = System.DateTime.UtcNow.AddHours(7);
                heartRate.Status = SD.GeneralStatus.ACTIVE;

                await _unitOfWork.HeartRateRepository.CreateAsync(heartRate);

                return new BusinessResult(Const.SUCCESS_CREATE, "Heart Rate created successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> CreateBloodGlucose(CreateBloodGlucoseRequest request)
        {
            try
            {
                var isExisted = await _unitOfWork.ElderlyRepository.GetByIdAsync(request.ElderlyId);

                if (isExisted == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                var bloodGlucose = _mapper.Map<BloodGlucose>(request);
                bloodGlucose.DateRecorded = System.DateTime.UtcNow.AddHours(7);
                bloodGlucose.Status = SD.GeneralStatus.ACTIVE;

                await _unitOfWork.BloodGlucoseRepository.CreateAsync(bloodGlucose);

                return new BusinessResult(Const.SUCCESS_CREATE, "Blood Glucose created successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> CreateLipidProfile(CreateLipidProfileRequest request)
        {
            try
            {
                var isExisted = await _unitOfWork.ElderlyRepository.GetByIdAsync(request.ElderlyId);

                if (isExisted == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                var lipidProfile = _mapper.Map<LipidProfile>(request);
                lipidProfile.DateRecorded = System.DateTime.UtcNow.AddHours(7);
                lipidProfile.Status = SD.GeneralStatus.ACTIVE;

                await _unitOfWork.LipidProfileRepository.CreateAsync(lipidProfile);

                return new BusinessResult(Const.SUCCESS_CREATE, "Lipid Profile created successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> CreateLiverEnzymes(CreateLiverEnzymesRequest request)
        {
            try
            {
                var isExisted = await _unitOfWork.ElderlyRepository.GetByIdAsync(request.ElderlyId);

                if (isExisted == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                var liverEnzyme = _mapper.Map<LiverEnzyme>(request);
                liverEnzyme.DateRecorded = System.DateTime.UtcNow.AddHours(7);
                liverEnzyme.Status = SD.GeneralStatus.ACTIVE;

                await _unitOfWork.LiverEnzymeRepository.CreateAsync(liverEnzyme);

                return new BusinessResult(Const.SUCCESS_CREATE, "Liver Enzymes created successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> CreateKidneyFunction(CreateKidneyFunctionRequest request)
        {
            try
            {
                var isExisted = await _unitOfWork.ElderlyRepository.GetByIdAsync(request.ElderlyId);

                if (isExisted == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                var kidneyFunction = _mapper.Map<KidneyFunction>(request);
                kidneyFunction.DateRecorded = System.DateTime.UtcNow.AddHours(7);
                kidneyFunction.Status = SD.GeneralStatus.ACTIVE;

                await _unitOfWork.KidneyFunctionRepository.CreateAsync(kidneyFunction);

                return new BusinessResult(Const.SUCCESS_CREATE, "Kidney Function created successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetWeightById(int id)
        {
            try
            {
                var weightHeight = _unitOfWork.WeightRepository.GetByIdAsync(id);

                var weightHeightResult = _mapper.Map<GetWeightDTO>(weightHeight);

                return new BusinessResult(Const.SUCCESS_READ, "Weight retrieved successfully.", weightHeightResult);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetHeightById(int id)
        {
            try
            {
                var weightHeight = _unitOfWork.HeightRepository.GetByIdAsync(id);

                var weightHeightResult = _mapper.Map<GetHeightDTO>(weightHeight);

                return new BusinessResult(Const.SUCCESS_READ, "height retrieved successfully.", weightHeightResult);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetBloodPressureById(int id)
        {
            try
            {
                var bloodPressure = _unitOfWork.BloodPressureRepository.GetByIdAsync(id);

                var bloodPressureResult = _mapper.Map<GetBloodPressureDTO>(bloodPressure);

                return new BusinessResult(Const.SUCCESS_READ, "Blood Pressure retrieved successfully.", bloodPressureResult);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetHeartRateById(int id)
        {
            try
            {
                var heartRate = _unitOfWork.HeartRateRepository.GetByIdAsync(id);

                var heartRateResult = _mapper.Map<GetHeartRateDTO>(heartRate);

                return new BusinessResult(Const.SUCCESS_READ, "Heart Rate retrieved successfully.", heartRateResult);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetBloodGlucoseById(int id)
        {
            try
            {
                var bloodGlucose = _unitOfWork.BloodGlucoseRepository.GetByIdAsync(id);

                var bloodGlucoseResult = _mapper.Map<GetBloodGlucoseDTO>(bloodGlucose);

                return new BusinessResult(Const.SUCCESS_READ, "Blood Glucose retrieved successfully.", bloodGlucoseResult);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetLipidProfileById(int id)
        {
            try
            {
                var lipidProfile = _unitOfWork.LipidProfileRepository.GetByIdAsync(id);

                var lipidProfileResult = _mapper.Map<GetHeartRateDTO>(lipidProfile);

                return new BusinessResult(Const.SUCCESS_READ, "Lipid Profile retrieved successfully.", lipidProfileResult);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetLiverEnzymesById(int id)
        {
            try
            {
                var liverEnzyme = _unitOfWork.LiverEnzymeRepository.GetByIdAsync(id);

                var liverEnzymeResult = _mapper.Map<GetLiverEnzymesDTO>(liverEnzyme);

                return new BusinessResult(Const.SUCCESS_READ, "Liver Enzyme retrieved successfully.", liverEnzymeResult);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetKidneyFunctionById(int id)
        {
            try
            {
                var kidneyFunction = _unitOfWork.KidneyFunctionRepository.GetByIdAsync(id);

                var kidneyFunctionResult = _mapper.Map<GetKidneyFunctionDTO>(kidneyFunction);

                return new BusinessResult(Const.SUCCESS_READ, "Kidney Function retrieved successfully.", kidneyFunctionResult);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> UpdateWeightStatus(int weightId, string status)
        {
            try
            {
                if (weightId <= 0)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Invalid weight ID.");
                }

                var weight = await _unitOfWork.WeightRepository.GetByIdAsync(weightId);
                if (weight == null)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Weight record not found.");
                }

                weight.Status = status;
                var rs = await _unitOfWork.WeightRepository.UpdateAsync(weight);

                if (rs < 1)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                }

                return new BusinessResult(Const.SUCCESS_UPDATE, "Weight status updated successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> UpdateHeightStatus(int heightId, string status)
        {
            try
            {
                if (heightId <= 0)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Invalid height ID.");
                }

                var height = await _unitOfWork.HeightRepository.GetByIdAsync(heightId);
                if (height == null)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Height record not found.");
                }

                height.Status = status;
                var rs = await _unitOfWork.HeightRepository.UpdateAsync(height);

                if (rs < 1)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                }

                return new BusinessResult(Const.SUCCESS_UPDATE, "Height status updated successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> UpdateBloodPressureStatus(int bloodPressureId, string status)
        {
            try
            {
                if (bloodPressureId <= 0)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Invalid blood pressure ID.");
                }

                var bloodPressure = await _unitOfWork.BloodPressureRepository.GetByIdAsync(bloodPressureId);
                if (bloodPressure == null)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Blood pressure record not found.");
                }

                bloodPressure.Status = status;
                var rs = await _unitOfWork.BloodPressureRepository.UpdateAsync(bloodPressure);

                if (rs < 1)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                }

                return new BusinessResult(Const.SUCCESS_UPDATE, "Blood pressure status updated successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> UpdateHeartRateStatus(int heartRateId, string status)
        {
            try
            {
                if (heartRateId <= 0)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Invalid heart rate ID.");
                }

                var heartRate = await _unitOfWork.HeartRateRepository.GetByIdAsync(heartRateId);
                if (heartRate == null)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Heart rate record not found.");
                }

                heartRate.Status = status;
                var rs = await _unitOfWork.HeartRateRepository.UpdateAsync(heartRate);

                if (rs < 1)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                }

                return new BusinessResult(Const.SUCCESS_UPDATE, "Heart rate status updated successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> UpdateBloodGlucoseStatus(int bloodGlucoseId, string status)
        {
            try
            {
                if (bloodGlucoseId <= 0)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Invalid blood glucose ID.");
                }

                var bloodGlucose = await _unitOfWork.BloodGlucoseRepository.GetByIdAsync(bloodGlucoseId);
                if (bloodGlucose == null)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Blood glucose record not found.");
                }

                bloodGlucose.Status = status;
                var rs = await _unitOfWork.BloodGlucoseRepository.UpdateAsync(bloodGlucose);

                if (rs < 1)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                }

                return new BusinessResult(Const.SUCCESS_UPDATE, "Blood glucose status updated successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> UpdateLipidProfileStatus(int lipidProfileId, string status)
        {
            try
            {
                if (lipidProfileId <= 0)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Invalid lipid profile ID.");
                }

                var lipidProfile = await _unitOfWork.LipidProfileRepository.GetByIdAsync(lipidProfileId);
                if (lipidProfile == null)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Lipid profile record not found.");
                }

                lipidProfile.Status = status;
                var rs = await _unitOfWork.LipidProfileRepository.UpdateAsync(lipidProfile);

                if (rs < 1)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                }

                return new BusinessResult(Const.SUCCESS_UPDATE, "Lipid profile status updated successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> UpdateLiverEnzymesStatus(int liverEnzymesId, string status)
        {
            try
            {
                if (liverEnzymesId <= 0)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Invalid liver enzymes ID.");
                }

                var liverEnzymes = await _unitOfWork.LiverEnzymeRepository.GetByIdAsync(liverEnzymesId);
                if (liverEnzymes == null)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Liver enzymes record not found.");
                }

                liverEnzymes.Status = status;
                var rs = await _unitOfWork.LiverEnzymeRepository.UpdateAsync(liverEnzymes);

                if (rs < 1)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                }

                return new BusinessResult(Const.SUCCESS_UPDATE, "Liver enzymes status updated successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> UpdateKidneyFunctionStatus(int kidneyFunctionId, string status)
        {
            try
            {
                if (kidneyFunctionId <= 0)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Invalid kidney function ID.");
                }

                var kidneyFunction = await _unitOfWork.KidneyFunctionRepository.GetByIdAsync(kidneyFunctionId);
                if (kidneyFunction == null)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Kidney function record not found.");
                }

                kidneyFunction.Status = status;
                var rs = await _unitOfWork.KidneyFunctionRepository.UpdateAsync(kidneyFunction);

                if (rs < 1)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                }

                return new BusinessResult(Const.SUCCESS_UPDATE, "Kidney function status updated successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        /*  public async Task<IBusinessResult> GetAllHealthIndicatorsByElderlyId(int elderlyId, string filter = null)
          {
              try
              {
                  if (elderlyId <= 0)
                  {
                      return new BusinessResult(Const.FAIL_READ, "Invalid elderly ID.");
                  }

                  var healthIndicators = new Dictionary<string, object>();

                  if (filter == null || filter.Equals("Weight", StringComparison.OrdinalIgnoreCase))
                  {
                      var weights = _unitOfWork.WeightRepository.FindByCondition(w => w.ElderlyId == elderlyId).OrderByDescending(kf => kf.DateRecorded).ToList();
                      var weightDTOs = _mapper.Map<List<GetWeightDTO>>(weights);
                      FormatDateRecorded(weightDTOs);
                      healthIndicators["Weight"] = weightDTOs;
                  }

                  if (filter == null || filter.Equals("Height", StringComparison.OrdinalIgnoreCase))
                  {
                      var heights = _unitOfWork.HeightRepository.FindByCondition(h => h.ElderlyId == elderlyId).OrderByDescending(kf => kf.DateRecorded).ToList();
                      var heightDTOs = _mapper.Map<List<GetHeightDTO>>(heights);
                      FormatDateRecorded(heightDTOs);
                      healthIndicators["Height"] = heightDTOs;
                  }

                  if (filter == null || filter.Equals("BloodPressure", StringComparison.OrdinalIgnoreCase))
                  {
                      var bloodPressures = _unitOfWork.BloodPressureRepository.FindByCondition(bp => bp.ElderlyId == elderlyId).OrderByDescending(kf => kf.DateRecorded).ToList();
                      var bloodPressureDTOs = _mapper.Map<List<GetBloodPressureDTO>>(bloodPressures);
                      FormatDateRecorded(bloodPressureDTOs);
                      healthIndicators["BloodPressure"] = bloodPressureDTOs;
                  }

                  if (filter == null || filter.Equals("HeartRate", StringComparison.OrdinalIgnoreCase))
                  {
                      var heartRates = _unitOfWork.HeartRateRepository.FindByCondition(hr => hr.ElderlyId == elderlyId).OrderByDescending(kf => kf.DateRecorded).ToList();
                      var heartRateDTOs = _mapper.Map<List<GetHeartRateDTO>>(heartRates);
                      FormatDateRecorded(heartRateDTOs);
                      healthIndicators["HeartRate"] = heartRateDTOs;
                  }

                  if (filter == null || filter.Equals("BloodGlucose", StringComparison.OrdinalIgnoreCase))
                  {
                      var bloodGlucoseRecords = _unitOfWork.BloodGlucoseRepository.FindByCondition(bg => bg.ElderlyId == elderlyId).OrderByDescending(kf => kf.DateRecorded).ToList();
                      var bloodGlucoseDTOs = _mapper.Map<List<GetBloodGlucoseDTO>>(bloodGlucoseRecords);
                      FormatDateRecorded(bloodGlucoseDTOs);
                      healthIndicators["BloodGlucose"] = bloodGlucoseDTOs;
                  }

                  if (filter == null || filter.Equals("LipidProfile", StringComparison.OrdinalIgnoreCase))
                  {
                      var lipidProfiles = _unitOfWork.LipidProfileRepository.FindByCondition(lp => lp.ElderlyId == elderlyId).OrderByDescending(kf => kf.DateRecorded).ToList();
                      var lipidProfileDTOs = _mapper.Map<List<GetLipidProfileDTO>>(lipidProfiles);
                      FormatDateRecorded(lipidProfileDTOs);
                      healthIndicators["LipidProfile"] = lipidProfileDTOs;
                  }

                  if (filter == null || filter.Equals("LiverEnzymes", StringComparison.OrdinalIgnoreCase))
                  {
                      var liverEnzymes = _unitOfWork.LiverEnzymeRepository.FindByCondition(le => le.ElderlyId == elderlyId).OrderByDescending(kf => kf.DateRecorded).ToList();
                      var liverEnzymesDTOs = _mapper.Map<List<GetLiverEnzymesDTO>>(liverEnzymes);
                      FormatDateRecorded(liverEnzymesDTOs);
                      healthIndicators["LiverEnzymes"] = liverEnzymesDTOs;
                  }

                  if (filter == null || filter.Equals("KidneyFunction", StringComparison.OrdinalIgnoreCase))
                  {
                      var kidneyFunctions = _unitOfWork.KidneyFunctionRepository.FindByCondition(kf => kf.ElderlyId == elderlyId).OrderByDescending(kf => kf.DateRecorded).ToList();
                      var kidneyFunctionDTOs = _mapper.Map<List<GetKidneyFunctionDTO>>(kidneyFunctions);
                      FormatDateRecorded(kidneyFunctionDTOs);
                      healthIndicators["KidneyFunction"] = kidneyFunctionDTOs;
                  }

                  return new BusinessResult(Const.SUCCESS_READ, "Health indicators retrieved successfully.", healthIndicators);
              }
              catch (Exception ex)
              {
                  return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
              }
          }
  */
        /* private void FormatDateRecorded<T>(List<T> dtos) where T : class
         {
             foreach (var dto in dtos)
             {
                 var dateRecordedProperty = dto.GetType().GetProperty("DateRecorded");
                 if (dateRecordedProperty != null && dateRecordedProperty.PropertyType == typeof(string))
                 {
                     var dateRecordedValue = dateRecordedProperty.GetValue(dto) as DateTime?;
                     if (dateRecordedValue.HasValue)
                     {
                         dateRecordedProperty.SetValue(dto, dateRecordedValue.Value.ToString("dd-MM-yyyy HH:mm:ss"));
                     }
                 }
             }
         }*/

        public async Task<IBusinessResult> GetWeightDetail(int accountId)
        {
            try
            {
                var elderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(accountId);

                if (elderly == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                // Get the newest height record for the elderly
                var newestHeightRecord = _unitOfWork.HeightRepository
                    .FindByCondition(h => h.ElderlyId == elderly.Elderly.ElderlyId && h.Status == SD.GeneralStatus.ACTIVE)
                    .OrderByDescending(h => h.DateRecorded)
                    .FirstOrDefault();

                if (newestHeightRecord == null || newestHeightRecord.Height1 == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "No valid height record found for the elderly.");
                }

                // Convert height from cm to meters for BMI calculation
                var heightInMeters = newestHeightRecord.Height1.Value / 100;

                // Fetch all active weight records for the elderly
                var weightRecords = await _unitOfWork.WeightRepository
                    .FindByConditionAsync(w => w.ElderlyId == elderly.Elderly.ElderlyId
                                    && w.Status == SD.GeneralStatus.ACTIVE);

                if (!weightRecords.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, "No weight records found for the elderly.");
                }

                // Get current date and time
                var today = System.DateTime.UtcNow.AddHours(7); // Adjust for your timezone if needed

                // Generate dates for the last 7 days (current date and 6 days before)
                var last7Days = Enumerable.Range(0, 7)
                    .Select(offset => today.AddDays(-offset).Date)
                    .OrderBy(date => date) // Sort dates in ascending order
                    .ToList();

                // Generate weeks for the last 6 weeks (current week and 5 weeks before)
                var last6Weeks = Enumerable.Range(0, 6)
                    .Select(offset => today.AddDays(-(offset * 7)))
                    .OrderBy(date => date) // Sort weeks in ascending order
                    .Select(date => new
                    {
                        StartOfWeek = date.AddDays(-(int)date.DayOfWeek + (int)System.DayOfWeek.Monday), // Start of the week (Monday)
                        EndOfWeek = date.AddDays(-(int)date.DayOfWeek + (int)System.DayOfWeek.Monday + 6), // End of the week (Sunday)
                        WeekLabel = $"Week {CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, System.DayOfWeek.Monday)}, {date.Year}"
                    })
                    .ToList();

                // Generate months for the last 4 months (current month and 3 months before)
                var last4Months = Enumerable.Range(0, 4)
                    .Select(offset => today.AddMonths(-offset))
                    .OrderBy(date => date) // Sort months in ascending order
                    .Select(date => new
                    {
                        StartOfMonth = new System.DateTime(date.Year, date.Month, 1), // Start of the month
                        EndOfMonth = new System.DateTime(date.Year, date.Month, System.DateTime.DaysInMonth(date.Year, date.Month)), // End of the month
                        MonthLabel = $"{CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(date.Month)} {date.Year}"
                    })
                    .ToList();

                // Group weight records by day for the last 7 days
                var dailyRecords = last7Days
                    .Select(date => new
                    {
                        Date = date,
                        Records = weightRecords
                            .Where(record => record.DateRecorded.HasValue && record.DateRecorded.Value.Date == date)
                            .ToList()
                    })
                    .Select(x => new ChartDatModel
                    {
                        Type = x.Date.DayOfWeek.ToString(), // Format: "Monday", "Tuesday", etc.
                        Indicator = x.Records.Any() ? (double?)Math.Round(x.Records.Average(w => w.Weight1.Value), 2) : null
                    })
                    .ToList(); // No need to reorder since last7Days is already in the correct order

                // Group weight records by week for the last 6 weeks
                var weeklyRecords = last6Weeks
                    .Select(week => new
                    {
                        Week = week,
                        Records = weightRecords
                            .Where(record => record.DateRecorded.HasValue &&
                                             record.DateRecorded.Value.Date >= week.StartOfWeek &&
                                             record.DateRecorded.Value.Date <= week.EndOfWeek)
                            .ToList()
                    })
                    .Select(x => new ChartDatModel
                    {
                        Type = x.Week.WeekLabel, // Format: "Week X, YYYY"
                        Indicator = x.Records.Any() ? (double?)Math.Round(x.Records.Average(w => w.Weight1.Value), 2) : null
                    })
                    .OrderBy(record => System.DateTime.Parse(record.Type.Split(',')[1].Trim() + "-" + record.Type.Split(' ')[1].TrimStart('0'))) // Sort by week in ascending order
                    .ToList();

                // Group weight records by month for the last 4 months
                var monthlyRecords = last4Months
                    .Select(month => new
                    {
                        Month = month,
                        Records = weightRecords
                            .Where(record => record.DateRecorded.HasValue &&
                                             record.DateRecorded.Value.Date >= month.StartOfMonth &&
                                             record.DateRecorded.Value.Date <= month.EndOfMonth)
                            .ToList()
                    })
                    .Select(x => new ChartDatModel
                    {
                        Type = x.Month.MonthLabel, // Format: "Month YYYY"
                        Indicator = x.Records.Any() ? (double?)Math.Round(x.Records.Average(w => w.Weight1.Value), 2) : null
                    })
                    .OrderBy(record => System.DateTime.ParseExact(record.Type, "MMMM yyyy", CultureInfo.InvariantCulture)) // Sort by month in ascending order
                    .ToList();

                // Group weight records by year
                var yearlyRecords = weightRecords
                    .GroupBy(w => w.DateRecorded.Value.Year)
                    .Select(g => new ChartDatModel
                    {
                        Type = g.Key.ToString(), // Format: "YYYY"
                        Indicator = (double?)Math.Round(g.Average(w => w.Weight1.Value), 2) // Use weight, not BMI
                    })
                    .OrderBy(record => int.Parse(record.Type)) // Sort by year in ascending order
                    .ToList();

                // Calculate BMI for the current period (current day, current week, current month, current year)
                var currentDay = today.Date; // Current day
                var currentWeek = new
                {
                    StartOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)System.DayOfWeek.Monday), // Start of the week (Monday)
                    EndOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)System.DayOfWeek.Monday + 6), // End of the week (Sunday)
                    WeekLabel = $"Week {CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(today, CalendarWeekRule.FirstDay, System.DayOfWeek.Monday)}, {today.Year}"
                };
                var currentMonth = new
                {
                    StartOfMonth = new System.DateTime(today.Year, today.Month, 1), // Start of the month
                    EndOfMonth = new System.DateTime(today.Year, today.Month, System.DateTime.DaysInMonth(today.Year, today.Month)), // End of the month
                    MonthLabel = $"{CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(today.Month)} {today.Year}"
                };
                var currentYear = today.Year; // Current year

                var currentDayWeight = weightRecords
                    .Where(w => w.DateRecorded.Value.Date == currentDay)
                    .DefaultIfEmpty() // Handle empty collection
                    .Average(w => w?.Weight1 ?? 0); // Use null-coalescing operator to handle null values

                var currentWeekWeight = currentWeek != null
                    ? weightRecords
                        .Where(w => w.DateRecorded.Value.Date >= currentWeek.StartOfWeek && w.DateRecorded.Value.Date <= currentWeek.EndOfWeek)
                        .DefaultIfEmpty()
                        .Average(w => w?.Weight1 ?? 0)
                    : 0;

                var currentMonthWeight = currentMonth != null
                    ? weightRecords
                        .Where(w => w.DateRecorded.Value.Date >= currentMonth.StartOfMonth && w.DateRecorded.Value.Date <= currentMonth.EndOfMonth)
                        .DefaultIfEmpty()
                        .Average(w => w?.Weight1 ?? 0)
                    : 0; // Handle case where currentMonth is null

                var currentYearWeight = weightRecords
                    .Where(w => w.DateRecorded.Value.Year == currentYear)
                    .DefaultIfEmpty()
                    .Average(w => w?.Weight1 ?? 0);

                var bmiForCurrentDay = CalculateBMI(currentDayWeight, heightInMeters);
                var bmiForCurrentWeek = CalculateBMI(currentWeekWeight, heightInMeters);
                var bmiForCurrentMonth = CalculateBMI(currentMonthWeight, heightInMeters);
                var bmiForCurrentYear = CalculateBMI(currentYearWeight, heightInMeters);

                // Create a list of responses for each tab
                var responseList = new List<GetWeightDetailReponse>
        {
            new GetWeightDetailReponse
            {
                Tabs = "Ngày",
                BMI = bmiForCurrentDay, // BMI for the current day
                ChartDatabase = dailyRecords
            },
            new GetWeightDetailReponse
            {
                Tabs = "Tuần",
                BMI = bmiForCurrentWeek, // BMI for the current week
                ChartDatabase = weeklyRecords
            },
            new GetWeightDetailReponse
            {
                Tabs = "Tháng",
                BMI = bmiForCurrentMonth, // BMI for the current month
                ChartDatabase = monthlyRecords
            },
            new GetWeightDetailReponse
            {
                Tabs = "Năm",
                BMI = bmiForCurrentYear, // BMI for the current year
                ChartDatabase = yearlyRecords
            }
        };

                return new BusinessResult(Const.SUCCESS_READ, "Weight details retrieved successfully.", responseList);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }
        private double CalculateBMI(decimal weight, decimal heightInMeters)
        {
            if (heightInMeters <= 0)
            {
                throw new ArgumentException("Height must be greater than 0.");
            }

            // BMI formula: weight (kg) / (height (m))^2
            var result = (double)(weight / (heightInMeters * heightInMeters));
            return Math.Floor(result * 100) / 100;
        }



    }
}
