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
        // Create methods
        Task<IBusinessResult> CreateWeightHeight(CreateWeightHeightRequest request);
        Task<IBusinessResult> CreateBloodPressure(CreateBloodPressureRequest request);
        Task<IBusinessResult> CreateHeartRate(CreateHeartRateRequest request);
        Task<IBusinessResult> CreateBloodGlucose(CreateBloodGlucoseRequest request);
        Task<IBusinessResult> CreateLipidProfile(CreateLipidProfileRequest request);
        Task<IBusinessResult> CreateLiverEnzymes(CreateLiverEnzymesRequest request);
        Task<IBusinessResult> CreateKidneyFunction(CreateKidneyFunctionRequest request);

        // Get methods for individual health indicators
        Task<IBusinessResult> GetWeightHeightByElderlyId(int elderlyId);
        Task<IBusinessResult> GetBloodPressureByElderlyId(int elderlyId);
        Task<IBusinessResult> GetHeartRateByElderlyId(int elderlyId);
        Task<IBusinessResult> GetBloodGlucoseByElderlyId(int elderlyId);
        Task<IBusinessResult> GetLipidProfileByElderlyId(int elderlyId);
        Task<IBusinessResult> GetLiverEnzymesByElderlyId(int elderlyId);
        Task<IBusinessResult> GetKidneyFunctionByElderlyId(int elderlyId);

        // Get methods by id
        Task<IBusinessResult> GetWeightHeightById(int id);
        Task<IBusinessResult> GetBloodPressureById(int id);
        Task<IBusinessResult> GetHeartRateById(int id);
        Task<IBusinessResult> GetBloodGlucoseById(int id);
        Task<IBusinessResult> GetLipidProfileById(int id);
        Task<IBusinessResult> GetLiverEnzymesById(int id);
        Task<IBusinessResult> GetKidneyFunctionById(int id);


        // Update status methods for each health indicator
        Task<IBusinessResult> UpdateWeightHeightStatus(int weightHeightId, string status);
        Task<IBusinessResult> UpdateBloodPressureStatus(int bloodPressureId, string status);
        Task<IBusinessResult> UpdateHeartRateStatus(int heartRateId, string status);
        Task<IBusinessResult> UpdateBloodGlucoseStatus(int bloodGlucoseId, string status);
        Task<IBusinessResult> UpdateLipidProfileStatus(int lipidProfileId, string status);
        Task<IBusinessResult> UpdateLiverEnzymesStatus(int liverEnzymesId, string status);
        Task<IBusinessResult> UpdateKidneyFunctionStatus(int kidneyFunctionId, string status);
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

        public async Task<IBusinessResult> CreateWeightHeight(CreateWeightHeightRequest request)
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

                var weightHeight = _mapper.Map<WeightHeight>(request);
                weightHeight.DateRecorded = DateTime.UtcNow.AddHours(7);
                weightHeight.Status = SD.GeneralStatus.ACTIVE;

                await _unitOfWork.WeightHeightRepository.CreateAsync(weightHeight);

                return new BusinessResult(Const.SUCCESS_CREATE, "Weight and Height created successfully.");
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
                if (request == null)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Request cannot be null.");
                }

                if (request.ElderlyId <= 0)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Invalid elderly ID.");
                }

                var bloodPressure = _mapper.Map<BloodPressure>(request);
                bloodPressure.DateRecorded = DateTime.UtcNow.AddHours(7);
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
                if (request == null)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Request cannot be null.");
                }

                if (request.ElderlyId <= 0)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Invalid elderly ID.");
                }

                var heartRate = _mapper.Map<HeartRate>(request);
                heartRate.DateRecorded = DateTime.UtcNow.AddHours(7);
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
                if (request == null)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Request cannot be null.");
                }

                if (request.ElderlyId <= 0)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Invalid elderly ID.");
                }

                var bloodGlucose = _mapper.Map<BloodGlucose>(request);
                bloodGlucose.DateRecorded = DateTime.UtcNow.AddHours(7);
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
                if (request == null)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Request cannot be null.");
                }

                if (request.ElderlyId <= 0)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Invalid elderly ID.");
                }

                var lipidProfile = _mapper.Map<LipidProfile>(request);
                lipidProfile.DateRecorded = DateTime.UtcNow.AddHours(7);
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
                if (request == null)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Request cannot be null.");
                }

                if (request.ElderlyId <= 0)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Invalid elderly ID.");
                }

                var liverEnzyme = _mapper.Map<LiverEnzyme>(request);
                liverEnzyme.DateRecorded = DateTime.UtcNow.AddHours(7);
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
                if (request == null)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Request cannot be null.");
                }

                if (request.ElderlyId <= 0)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Invalid elderly ID.");
                }

                var kidneyFunction = _mapper.Map<KidneyFunction>(request);
                kidneyFunction.DateRecorded = DateTime.UtcNow.AddHours(7);
                kidneyFunction.Status = SD.GeneralStatus.ACTIVE;

                await _unitOfWork.KidneyFunctionRepository.CreateAsync(kidneyFunction);

                return new BusinessResult(Const.SUCCESS_CREATE, "Kidney Function created successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetWeightHeightByElderlyId(int elderlyId)
        {
            try
            {
                if (elderlyId <= 0)
                {
                    return new BusinessResult(Const.FAIL_READ, "Invalid elderly ID.");
                }

                var weightHeights = _unitOfWork.WeightHeightRepository.FindByCondition(wh => wh.ElderlyId == elderlyId).ToList();

                var weightHeightResults = _mapper.Map<List<CreateHealthIndicatorRequest>>(weightHeights);

                return new BusinessResult(Const.SUCCESS_READ, "Weight height retrieved successfully.", weightHeightResults);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetBloodPressureByElderlyId(int elderlyId)
        {
            try
            {
                if (elderlyId <= 0)
                {
                    return new BusinessResult(Const.FAIL_READ, "Invalid elderly ID.");
                }

                var bloodPressures = _unitOfWork.BloodPressureRepository.FindByCondition(bp => bp.ElderlyId == elderlyId).ToList();

                var bloodPressureResults = _mapper.Map<List<CreateBloodPressureRequest>>(bloodPressures);

                return new BusinessResult(Const.SUCCESS_READ, "Blood pressure retrieved successfully.", bloodPressureResults);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetHeartRateByElderlyId(int elderlyId)
        {
            try
            {
                if (elderlyId <= 0)
                {
                    return new BusinessResult(Const.FAIL_READ, "Invalid elderly ID.");
                }

                var heartRates = _unitOfWork.HeartRateRepository.FindByCondition(hr => hr.ElderlyId == elderlyId).ToList();

                var heartRateResults = _mapper.Map<List<CreateHeartRateRequest>>(heartRates);

                return new BusinessResult(Const.SUCCESS_READ, "Heart rate retrieved successfully.", heartRateResults);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetBloodGlucoseByElderlyId(int elderlyId)
        {
            try
            {
                if (elderlyId <= 0)
                {
                    return new BusinessResult(Const.FAIL_READ, "Invalid elderly ID.");
                }

                var bloodGlucoseRecords = _unitOfWork.BloodGlucoseRepository.FindByCondition(bg => bg.ElderlyId == elderlyId).ToList();

                var bloodGlucoseResults = _mapper.Map<List<CreateBloodGlucoseRequest>>(bloodGlucoseRecords);

                return new BusinessResult(Const.SUCCESS_READ, "Blood glucose retrieved successfully.", bloodGlucoseResults);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetLipidProfileByElderlyId(int elderlyId)
        {
            try
            {
                if (elderlyId <= 0)
                {
                    return new BusinessResult(Const.FAIL_READ, "Invalid elderly ID.");
                }

                var lipidProfiles = _unitOfWork.LipidProfileRepository.FindByCondition(lp => lp.ElderlyId == elderlyId).ToList();

                var lipidProfileResults = _mapper.Map<List<CreateLipidProfileRequest>>(lipidProfiles);

                return new BusinessResult(Const.SUCCESS_READ, "Lipid profile retrieved successfully.", lipidProfileResults);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetLiverEnzymesByElderlyId(int elderlyId)
        {
            try
            {
                if (elderlyId <= 0)
                {
                    return new BusinessResult(Const.FAIL_READ, "Invalid elderly ID.");
                }

                var liverEnzymes = _unitOfWork.LiverEnzymeRepository.FindByCondition(le => le.ElderlyId == elderlyId).ToList();

                var liverEnzymesResults = _mapper.Map<List<CreateLiverEnzymesRequest>>(liverEnzymes);

                return new BusinessResult(Const.SUCCESS_READ, "Liver enzymes retrieved successfully.", liverEnzymesResults);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetKidneyFunctionByElderlyId(int elderlyId)
        {
            try
            {
                if (elderlyId <= 0)
                {
                    return new BusinessResult(Const.FAIL_READ, "Invalid elderly ID.");
                }

                var kidneyFunctions = _unitOfWork.KidneyFunctionRepository.FindByCondition(kf => kf.ElderlyId == elderlyId).ToList();

                var kidneyFunctionResults = _mapper.Map<List<CreateKidneyFunctionRequest>>(kidneyFunctions);

                return new BusinessResult(Const.SUCCESS_READ, "Kidney function retrieved successfully.", kidneyFunctionResults);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetWeightHeightById(int id)
        {
            try
            {
                var weightHeight = _unitOfWork.WeightHeightRepository.GetByIdAsync(id);

                var weightHeightResult = _mapper.Map<CreateHealthIndicatorRequest>(weightHeight);

                return new BusinessResult(Const.SUCCESS_READ, "Weight height retrieved successfully.", weightHeightResult);
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

                var bloodPressureResult = _mapper.Map<CreateBloodPressureRequest>(bloodPressure);

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

                var heartRateResult = _mapper.Map<CreateHeartRateRequest>(heartRate);

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

                var bloodGlucoseResult = _mapper.Map<CreateHeartRateRequest>(bloodGlucose);

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

                var lipidProfileResult = _mapper.Map<CreateHeartRateRequest>(lipidProfile);

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

                var liverEnzymeResult = _mapper.Map<CreateHeartRateRequest>(liverEnzyme);

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

                var kidneyFunctionResult = _mapper.Map<CreateHeartRateRequest>(kidneyFunction);

                return new BusinessResult(Const.SUCCESS_READ, "Kidney Function retrieved successfully.", kidneyFunctionResult);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> UpdateWeightHeightStatus(int weightHeightId, string status)
        {
            try
            {
                if (weightHeightId <= 0)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Invalid weight/height ID.");
                }

                var weightHeight = await _unitOfWork.WeightHeightRepository.GetByIdAsync(weightHeightId);
                if (weightHeight == null)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Weight/height record not found.");
                }

                weightHeight.Status = status;
                var rs = await _unitOfWork.WeightHeightRepository.UpdateAsync(weightHeight);

                if (rs < 1)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                }

                return new BusinessResult(Const.SUCCESS_UPDATE, "Weight/height status updated successfully.");
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
    }
}
