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
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Google.Type;
using static Google.Cloud.Vision.V1.ProductSearchResults.Types;
using Microsoft.EntityFrameworkCore.Storage;
using SE.Common.Response.HealthIndicator;

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

        // Update status methods for each health indicator
        Task<IBusinessResult> UpdateWeightStatus(int weightId, string status);
        Task<IBusinessResult> UpdateHeightStatus(int heightId, string status);
        Task<IBusinessResult> UpdateBloodPressureStatus(int bloodPressureId, string status);
        Task<IBusinessResult> UpdateHeartRateStatus(int heartRateId, string status);
        Task<IBusinessResult> UpdateBloodGlucoseStatus(int bloodGlucoseId, string status);
        Task<IBusinessResult> UpdateLipidProfileStatus(int lipidProfileId, string status);
        Task<IBusinessResult> UpdateLiverEnzymesStatus(int liverEnzymesId, string status);
        Task<IBusinessResult> UpdateKidneyFunctionStatus(int kidneyFunctionId, string status);

        //update

        Task<IBusinessResult> UpdateKidneyFunction(int kidneyFunctionId, decimal creatinine, decimal bun, decimal eGfr, string type);
        Task<IBusinessResult> UpdateLiverEnzymes(int liverEnzymesId, decimal alt, decimal ast, decimal alp, decimal ggt, string type);
        Task<IBusinessResult> UpdateLipidProfile(int lipidProfileId, decimal totalCholesterol, decimal ldlcholesterol, decimal hdlcholesterol, decimal triglycerides, string type);
        Task<IBusinessResult> UpdateBloodGlucose(int bloodGlucoseId, decimal bloodGlucoseUpdate, string time, string type);
        Task<IBusinessResult> UpdateHeartRate(int heartRateId, int heartRateUpdate, string type);
        Task<IBusinessResult> UpdateBloodPressure(int bloodPressureId, int systolic, int diastolic, string type);
        Task<IBusinessResult> UpdateHeight(int heightId, int heightUpdate, string type);
        Task<IBusinessResult> UpdateWeight(int weightId, int weightUpdate, string type);


        //tivo
        Task<IBusinessResult> GetWeightDetail(int accountId);
        Task<IBusinessResult> GetHeightDetail(int accountId);
        Task<IBusinessResult> GetHeartRateDetail(int accountId);
        Task<IBusinessResult> GetBloodPressureDetail(int accountId);

        Task<IBusinessResult> GetBloodGlucoseDetail(int accountId);
        Task<IBusinessResult> GetLipidProfileDetail(int accountId);
        Task<IBusinessResult> GetLiverEnzymesDetail(int accountId);

        Task<IBusinessResult> GetKidneyFunctionDetail(int accountId);
        Task<IBusinessResult> EvaluateHealthIndicator(EvaluateHealthIndicatorRequest req);
        Task<IBusinessResult> GetAllHealthIndicators(int accountId);

        Task<IBusinessResult> EvaluateBloodPressure(int systolic, int diastolic);
        Task<IBusinessResult> EvaluateHeartRate(int? heartRate);
        Task<IBusinessResult> EvaluateBMI(decimal? height, decimal? weight, int accountId);
        Task<IBusinessResult> EvaluateLiverEnzymes(decimal? alt, decimal? ast, decimal? alp, decimal? ggt);
        Task<IBusinessResult> EvaluateLipidProfile(decimal? totalCholesterol, decimal? ldlCholesterol, decimal? hdlCholesterol, decimal? triglycerides);
        Task<IBusinessResult> EvaluateKidneyFunction(decimal creatinine, decimal BUN, decimal eGFR);
        Task<IBusinessResult> EvaluateBloodGlusose(decimal bloodGlucose, string time);

        Task<IBusinessResult> GetLogBookResponses(int accountId);

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
                var getElderly = new Account();
                var getFamily = new Account();
                var isExisted = new Elderly();
                if (request.ElderlyId != 0)
                {
                    //id nguoi gia
                    getElderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(request.ElderlyId);
                    getFamily = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(request.AccountId);
                
                }
                else
                {
                    getElderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(request.AccountId);
                }

                isExisted = await _unitOfWork.ElderlyRepository.GetByIdAsync(getElderly.Elderly.ElderlyId);

                if (isExisted == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                if (request.Weight1 <= 0)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Weight must be greater than 0.");
                }

                var weightEntity = _mapper.Map<Weight>(request);
                weightEntity.DateRecorded = System.DateTime.UtcNow.AddHours(7);
                weightEntity.Status = SD.GeneralStatus.ACTIVE;
                weightEntity.ElderlyId = getElderly.Elderly.ElderlyId;

                if (getFamily.FullName != null) weightEntity.CreatedBy = getFamily.FullName;
                else weightEntity.CreatedBy = getElderly.FullName;

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
                var getElderly = new Account();
                var getFamily = new Account();
                if (request.ElderlyId != 0)
                {
                    //id nguoi gia
                    getElderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(request.ElderlyId);
                    getFamily = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(request.AccountId);

                }
                else
                {
                    getElderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(request.AccountId);
                }
                var isExisted = await _unitOfWork.ElderlyRepository.GetByIdAsync(getElderly.Elderly.ElderlyId);

                if (isExisted == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                if (request.Height1 <= 0)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Height must be greater than 0.");
                }

                var heightEntity = _mapper.Map<Height>(request);
                heightEntity.DateRecorded = System.DateTime.UtcNow.AddHours(7);
                heightEntity.Status = SD.GeneralStatus.ACTIVE;
                heightEntity.ElderlyId = getElderly.Elderly.ElderlyId;
                if (getFamily.FullName != null) heightEntity.CreatedBy = getFamily.FullName;
                else heightEntity.CreatedBy = getElderly.FullName;

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
                var getElderly = new Account();
                var getFamily = new Account();
                if (request.ElderlyId != 0)
                {
                    //id nguoi gia
                    getElderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(request.ElderlyId);
                    getFamily = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(request.AccountId);

                }
                else
                {
                    getElderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(request.AccountId);
                }
                var isExisted = await _unitOfWork.ElderlyRepository.GetByIdAsync(getElderly.Elderly.ElderlyId);

                if (isExisted == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                if (request.Systolic < 30 || request.Systolic > 300)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Systolic blood pressure must be in 30~300!");
                }

                if (request.Diastolic < 20 || request.Diastolic > 250)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Diastolic blood pressure must be in 20~250!");
                }

                var bloodPressure = _mapper.Map<BloodPressure>(request);
                bloodPressure.DateRecorded = System.DateTime.UtcNow.AddHours(7);
                bloodPressure.Status = SD.GeneralStatus.ACTIVE;
                bloodPressure.ElderlyId = getElderly.Elderly.ElderlyId;
                if (getFamily.FullName != null) bloodPressure.CreatedBy = getFamily.FullName;
                else bloodPressure.CreatedBy = getElderly.FullName;
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
                var getElderly = new Account();
                var getFamily = new Account();
                if (request.ElderlyId != 0)
                {
                    //id nguoi gia
                    getElderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(request.ElderlyId);
                    getFamily = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(request.AccountId);

                }
                else
                {
                    getElderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(request.AccountId);
                }
                var isExisted = await _unitOfWork.ElderlyRepository.GetByIdAsync(getElderly.Elderly.ElderlyId);

                if (isExisted == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                if (request.HeartRate1 < 40 || request.HeartRate1 > 300)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Heart rate must be in 40~300!");
                }

                var heartRate = _mapper.Map<HeartRate>(request);
                heartRate.DateRecorded = System.DateTime.UtcNow.AddHours(7);
                heartRate.Status = SD.GeneralStatus.ACTIVE;
                heartRate.ElderlyId = getElderly.Elderly.ElderlyId;
                if (getFamily.FullName != null) heartRate.CreatedBy = getFamily.FullName;
                else heartRate.CreatedBy = getElderly.FullName;
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
                var getElderly = new Account();
                var getFamily = new Account();
                if (request.ElderlyId != 0)
                {
                    //id nguoi gia
                    getElderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(request.ElderlyId);
                    getFamily = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(request.AccountId);

                }
                else
                {
                    getElderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(request.AccountId);
                }
                var isExisted = await _unitOfWork.ElderlyRepository.GetByIdAsync(getElderly.Elderly.ElderlyId);

                if (isExisted == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                var bloodGlucose = _mapper.Map<BloodGlucose>(request);
                bloodGlucose.DateRecorded = System.DateTime.UtcNow.AddHours(7);
                bloodGlucose.Status = SD.GeneralStatus.ACTIVE;
                bloodGlucose.ElderlyId = getElderly.Elderly.ElderlyId;
                if (getFamily.FullName != null) bloodGlucose.CreatedBy = getFamily.FullName;
                else bloodGlucose.CreatedBy = getElderly.FullName;
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
                var getElderly = new Account();
                var getFamily = new Account();
                if (request.ElderlyId != 0)
                {
                    //id nguoi gia
                    getElderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(request.ElderlyId);
                    getFamily = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(request.AccountId);

                }
                else
                {
                    getElderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(request.AccountId);
                }
                var isExisted = await _unitOfWork.ElderlyRepository.GetByIdAsync(getElderly.Elderly.ElderlyId);

                if (isExisted == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                var lipidProfile = _mapper.Map<LipidProfile>(request);
                lipidProfile.DateRecorded = System.DateTime.UtcNow.AddHours(7);
                lipidProfile.Status = SD.GeneralStatus.ACTIVE;
                lipidProfile.ElderlyId = getElderly.Elderly.ElderlyId;
                if (getFamily.FullName != null) lipidProfile.CreatedBy = getFamily.FullName;
                else lipidProfile.CreatedBy = getElderly.FullName;
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
                var getElderly = new Account();
                var getFamily = new Account();
                if (request.ElderlyId != 0)
                {
                    //id nguoi gia
                    getElderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(request.ElderlyId);
                    getFamily = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(request.AccountId);

                }
                else
                {
                    getElderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(request.AccountId);
                }
                var isExisted = await _unitOfWork.ElderlyRepository.GetByIdAsync(getElderly.Elderly.ElderlyId);

                if (isExisted == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                var liverEnzyme = _mapper.Map<LiverEnzyme>(request);
                liverEnzyme.DateRecorded = System.DateTime.UtcNow.AddHours(7);
                liverEnzyme.Status = SD.GeneralStatus.ACTIVE;
                liverEnzyme.ElderlyId = getElderly.Elderly.ElderlyId;
                if (getFamily.FullName != null) liverEnzyme.CreatedBy = getFamily.FullName;
                else liverEnzyme.CreatedBy = getElderly.FullName;
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
                var getElderly = new Account();
                var getFamily = new Account();
                if (request.ElderlyId != 0)
                {
                    //id nguoi gia
                    getElderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(request.ElderlyId);
                    getFamily = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(request.AccountId);

                }
                else
                {
                    getElderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(request.AccountId);
                }
                var isExisted = await _unitOfWork.ElderlyRepository.GetByIdAsync(getElderly.Elderly.ElderlyId);

                if (isExisted == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                var kidneyFunction = _mapper.Map<KidneyFunction>(request);
                kidneyFunction.DateRecorded = System.DateTime.UtcNow.AddHours(7);
                kidneyFunction.Status = SD.GeneralStatus.ACTIVE;
                kidneyFunction.ElderlyId = getElderly.Elderly.ElderlyId;
                if (getFamily.FullName != null) kidneyFunction.CreatedBy = getFamily.FullName;
                else kidneyFunction.CreatedBy = getElderly.FullName;
                await _unitOfWork.KidneyFunctionRepository.CreateAsync(kidneyFunction);

                return new BusinessResult(Const.SUCCESS_CREATE, "Kidney Function created successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
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


        //update

        public async Task<IBusinessResult> UpdateWeight(int weightId, int weightUpdate, string createdBy)
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

                weight.Weight1 = weightUpdate;
                weight.CreatedBy = createdBy;
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

        public async Task<IBusinessResult> UpdateHeight(int heightId, int heightUpdate, string createdBy)
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

                height.Height1 = heightUpdate;
                height.CreatedBy = createdBy;

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

        public async Task<IBusinessResult> UpdateBloodPressure(int bloodPressureId, int systolic, int diastolic, string createdBy)
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

                bloodPressure.Systolic = systolic;
                bloodPressure.Diastolic = diastolic;
                bloodPressure.CreatedBy = createdBy;
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

        public async Task<IBusinessResult> UpdateHeartRate(int heartRateId, int heartRateUpdate, string createdBy)
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

                heartRate.HeartRate1 = heartRateUpdate;
                heartRate.CreatedBy = createdBy;
                var rs = await _unitOfWork.HeartRateRepository.UpdateAsync(heartRate);

                if (rs < 1)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                }

                return new BusinessResult(Const.SUCCESS_UPDATE, "Heart rate updated successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> UpdateBloodGlucose(int bloodGlucoseId, decimal bloodGlucoseUpdate, string time, string createdBy)
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

                bloodGlucose.BloodGlucose1 = bloodGlucoseUpdate;
                bloodGlucose.Time = time;
                bloodGlucose.CreatedBy = createdBy;
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

        public async Task<IBusinessResult> UpdateLipidProfile(int lipidProfileId, decimal totalCholesterol, decimal ldlcholesterol, decimal hdlcholesterol, decimal triglycerides, string createdBy)
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

                lipidProfile.TotalCholesterol = totalCholesterol;
                lipidProfile.Ldlcholesterol = ldlcholesterol;
                lipidProfile.Hdlcholesterol = hdlcholesterol;
                lipidProfile.Triglycerides = triglycerides;
                lipidProfile.CreatedBy = createdBy;

                var rs = await _unitOfWork.LipidProfileRepository.UpdateAsync(lipidProfile);

                if (rs < 1)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                }

                return new BusinessResult(Const.SUCCESS_UPDATE, "Lipid profile updated successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> UpdateLiverEnzymes(int liverEnzymesId, decimal alt, decimal ast, decimal alp, decimal ggt, string createdBy)
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

                liverEnzymes.Alt = alt;
                liverEnzymes.Ast = ast;
                liverEnzymes.Alp = alp;
                liverEnzymes.Ggt = ggt;
                liverEnzymes.CreatedBy = createdBy;

                var rs = await _unitOfWork.LiverEnzymeRepository.UpdateAsync(liverEnzymes);

                if (rs < 1)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                }

                return new BusinessResult(Const.SUCCESS_UPDATE, "Liver enzymes updated successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> UpdateKidneyFunction(int kidneyFunctionId, decimal creatinine, decimal bun, decimal eGfr, string createdBy)
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

                kidneyFunction.Creatinine = creatinine;
                kidneyFunction.Bun = bun;
                kidneyFunction.EGfr = eGfr;
                kidneyFunction.CreatedBy = createdBy;

                var rs = await _unitOfWork.KidneyFunctionRepository.UpdateAsync(kidneyFunction);

                if (rs < 1)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                }

                return new BusinessResult(Const.SUCCESS_UPDATE, "Kidney function updated successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        //tivo
        private string MapWeekToVietnamese(string week)
        {
            if (week.StartsWith("Week "))
            {
                string number = week.Substring(5); // Extract the number part
                return "T-" + number;
            }
            throw new ArgumentException("Invalid week format", nameof(week));
        }

        private string MapMonthToVietnamese(string month)
        {
            Dictionary<string, string> monthMapping = new Dictionary<string, string>
    {
        { "January", "Th1" }, { "February", "Th2" }, { "March", "Th3" }, { "April", "Th4" },
        { "May", "Th5" }, { "June", "Th6" }, { "July", "Th7" }, { "August", "Th8" },
        { "September", "Th9" }, { "October", "Th10" }, { "November", "Th11" }, { "December", "Th12" }
    };

            if (monthMapping.TryGetValue(month, out string vietnamese))
            {
                return vietnamese;
            }

            throw new ArgumentException("Invalid month name", nameof(month));
        }

        private string MapDayOfWeekToVietnamese(System.DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case System.DayOfWeek.Monday:
                    return "T2";
                case System.DayOfWeek.Tuesday:
                    return "T3";
                case System.DayOfWeek.Wednesday:
                    return "T4";
                case System.DayOfWeek.Thursday:
                    return "T5";
                case System.DayOfWeek.Friday:
                    return "T6";
                case System.DayOfWeek.Saturday:
                    return "T7";
                case System.DayOfWeek.Sunday:
                    return "CN";
                default:
                    throw new ArgumentOutOfRangeException(nameof(dayOfWeek), dayOfWeek, null);
            }
        }
        public async Task<IBusinessResult> GetWeightDetail(int accountId)
        {
            try
            {
                var elderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(accountId);

                if (elderly == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                var newestHeightRecord = _unitOfWork.HeightRepository
                    .FindByCondition(h => h.ElderlyId == elderly.Elderly.ElderlyId && h.Status == SD.GeneralStatus.ACTIVE)
                    .OrderByDescending(h => h.DateRecorded)
                    .FirstOrDefault();

                if (newestHeightRecord == null || newestHeightRecord.Height1 == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "No valid height record found for the elderly.");
                }

                var heightInMeters = newestHeightRecord.Height1.Value / 100;

                var newestWeightRecord = _unitOfWork.WeightRepository
                    .FindByCondition(h => h.ElderlyId == elderly.Elderly.ElderlyId && h.Status == SD.GeneralStatus.ACTIVE)
                    .OrderByDescending(h => h.DateRecorded)
                    .FirstOrDefault();

                var BMIToday = newestWeightRecord != null && newestWeightRecord.Weight1.HasValue
                    ? CalculateBMI(newestWeightRecord.Weight1.Value, heightInMeters)
                    : 0;

                var weightRecords = await _unitOfWork.WeightRepository
                    .FindByConditionAsync(w => w.ElderlyId == elderly.Elderly.ElderlyId
                                    && w.Status == SD.GeneralStatus.ACTIVE);

                if (!weightRecords.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, "No weight records found for the elderly.");
                }

                var today = System.DateTime.UtcNow.AddHours(7);

                var last7Days = Enumerable.Range(0, 7)
                    .Select(offset => today.AddDays(-offset).Date)
                    .OrderBy(date => date)
                    .ToList();

                var last6Weeks = Enumerable.Range(0, 6)
                    .Select(offset => today.AddDays(-(offset * 7)))
                    .OrderBy(date => date)
                    .Select(date => new
                    {
                        StartOfWeek = date.AddDays(-(int)date.DayOfWeek + (int)System.DayOfWeek.Monday),
                        EndOfWeek = date.AddDays(-(int)date.DayOfWeek + (int)System.DayOfWeek.Monday + 6),
                        WeekLabel = $"Week {CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, System.DayOfWeek.Monday)}"
                    })
                    .ToList();

                var last4Months = Enumerable.Range(0, 4)
                    .Select(offset => today.AddMonths(-offset))
                    .OrderBy(date => date)
                    .Select(date => new
                    {
                        StartOfMonth = new System.DateTime(date.Year, date.Month, 1),
                        EndOfMonth = new System.DateTime(date.Year, date.Month, System.DateTime.DaysInMonth(date.Year, date.Month)),
                        MonthLabel = $"{CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(date.Month)}"
                    })
                    .ToList();

                // Calculate dailyRecords with non-null Indicator values
                var dailyRecords = last7Days
                    .Select(date => new
                    {
                        Date = date,
                        Records = weightRecords
                            .Where(record => record.DateRecorded.HasValue && record.DateRecorded.Value.Date == date)
                            .ToList()
                    })
                    .Select(x => new ChartDataModel
                    {
                        Type = MapDayOfWeekToVietnamese(x.Date.DayOfWeek),
                        Indicator = x.Records.Any() ? (double?)Math.Round(x.Records.Average(w => w.Weight1 ?? 0), 2) : null
                    })
                    .ToList();

                // Calculate weeklyRecords with non-null Indicator values
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
                    .Select(x => new ChartDataModel
                    {
                        Type = MapWeekToVietnamese( x.Week.WeekLabel),
                        Indicator = x.Records.Any() ? (double?)Math.Round(x.Records.Average(w => w.Weight1 ?? 0), 2) : null
                    })
                    .ToList();

                // Calculate monthlyRecords with non-null Indicator values
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
                    .Select(x => new ChartDataModel
                    {
                        Type = MapMonthToVietnamese( x.Month.MonthLabel),
                        Indicator = x.Records.Any() ? (double?)Math.Round(x.Records.Average(w => w.Weight1 ?? 0), 2) : null
                    })
                    .ToList();

                // Calculate yearlyRecords with non-null Indicator values
                var yearlyRecords = weightRecords
                    .GroupBy(w => w.DateRecorded.Value.Year)
                    .Select(g => new ChartDataModel
                    {
                        Type = g.Key.ToString(),
                        Indicator = (double?)Math.Round(g.Average(w => w.Weight1 ?? 0), 2)
                    })
                    .OrderBy(record => int.Parse(record.Type))
                    .ToList();

                // Calculate averages for each tab (only non-null Indicators)
                var dailyAverage = dailyRecords
                    .Where(d => d.Indicator.HasValue)
                    .DefaultIfEmpty(new ChartDataModel { Indicator = 0 })
                    .Average(d => d.Indicator.Value);

                var weeklyAverage = weeklyRecords
                    .Where(w => w.Indicator.HasValue)
                    .DefaultIfEmpty(new ChartDataModel { Indicator = 0 })
                    .Average(w => w.Indicator.Value);

                var monthlyAverage = monthlyRecords
                    .Where(m => m.Indicator.HasValue)
                    .DefaultIfEmpty(new ChartDataModel { Indicator = 0 })
                    .Average(m => m.Indicator.Value);

                var yearlyAverage = yearlyRecords
                    .Where(y => y.Indicator.HasValue)
                    .DefaultIfEmpty(new ChartDataModel { Indicator = 0 })
                    .Average(y => y.Indicator.Value);

                // Calculate BMI for each period using the height
                var bmiForCurrentDay = CalculateBMI((decimal)dailyAverage, heightInMeters);
                var bmiForCurrentWeek = CalculateBMI((decimal)weeklyAverage, heightInMeters);
                var bmiForCurrentMonth = CalculateBMI((decimal)monthlyAverage, heightInMeters);
                var bmiForCurrentYear = CalculateBMI((decimal)yearlyAverage, heightInMeters);

                var responseList = new List<GetHealthIndicatorDetailReponse>
        {
            new GetHealthIndicatorDetailReponse
            {
                Tabs = "Ngày",
                Average = dailyAverage,
                Evaluation = bmiForCurrentDay.ToString(),
                ChartDatabase = dailyRecords
            },
            new GetHealthIndicatorDetailReponse
            {
                Tabs = "Tuần",
                Average = weeklyAverage,
                Evaluation = bmiForCurrentWeek.ToString(),
                ChartDatabase = weeklyRecords
            },
            new GetHealthIndicatorDetailReponse
            {
                Tabs = "Tháng",
                Average = monthlyAverage,
                Evaluation = bmiForCurrentMonth.ToString(),
                ChartDatabase = monthlyRecords
            },
            new GetHealthIndicatorDetailReponse
            {
                Tabs = "Năm",
                Average = yearlyAverage,
                Evaluation = bmiForCurrentYear.ToString(),
                ChartDatabase = yearlyRecords
            }
        };

                var result = new
                {
                    BMIToday = BMIToday,
                    responseList
                };

                return new BusinessResult(Const.SUCCESS_READ, "Weight details retrieved successfully.", result);
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

            var result = (double)(weight / (heightInMeters * heightInMeters));
            return Math.Round(result, 2); // Round to 2 decimal places
        }
        public async Task<IBusinessResult> GetHeightDetail(int accountId)
        {
            try
            {
                var elderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(accountId);

                if (elderly == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                // Fetch height records
                var heightRecords = await _unitOfWork.HeightRepository
                    .FindByConditionAsync(h => h.ElderlyId == elderly.Elderly.ElderlyId && h.Status == SD.GeneralStatus.ACTIVE);

                if (!heightRecords.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, "No height records found for the elderly.");
                }

                // Fetch the newest weight record
                var newestWeightRecord =  _unitOfWork.WeightRepository
                    .FindByCondition(w => w.ElderlyId == elderly.Elderly.ElderlyId && w.Status == SD.GeneralStatus.ACTIVE)
                    .OrderByDescending(w => w.DateRecorded)
                    .FirstOrDefault();

                if (newestWeightRecord == null || newestWeightRecord.Weight1 == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "No valid weight record found for the elderly.");
                }

                var newestWeight = newestWeightRecord.Weight1.Value;

                var today = System.DateTime.UtcNow.AddHours(7);

                var last7Days = Enumerable.Range(0, 7)
                    .Select(offset => today.AddDays(-offset).Date)
                    .OrderBy(date => date)
                    .ToList();

                var last6Weeks = Enumerable.Range(0, 6)
                    .Select(offset => today.AddDays(-(offset * 7)))
                    .OrderBy(date => date)
                    .Select(date => new
                    {
                        StartOfWeek = date.AddDays(-(int)date.DayOfWeek + (int)System.DayOfWeek.Monday),
                        EndOfWeek = date.AddDays(-(int)date.DayOfWeek + (int)System.DayOfWeek.Monday + 6),
                        WeekLabel = $"Week {CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, System.DayOfWeek.Monday)}"
                    })
                    .ToList();

                var last4Months = Enumerable.Range(0, 4)
                    .Select(offset => today.AddMonths(-offset))
                    .OrderBy(date => date)
                    .Select(date => new
                    {
                        StartOfMonth = new System.DateTime(date.Year, date.Month, 1),
                        EndOfMonth = new System.DateTime(date.Year, date.Month, System.DateTime.DaysInMonth(date.Year, date.Month)),
                        MonthLabel = $"{CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(date.Month)}"
                    })
                    .ToList();

                // Calculate dailyRecords with non-null Indicator values
                var dailyRecords = last7Days
                    .Select(date => new
                    {
                        Date = date,
                        Records = heightRecords
                            .Where(record => record.DateRecorded.HasValue && record.DateRecorded.Value.Date == date)
                            .ToList()
                    })
                    .Select(x => new ChartDataModel
                    {
                        Type = MapDayOfWeekToVietnamese(x.Date.DayOfWeek),
                        Indicator = x.Records.Any() ? (double?)x.Records.Average(h => h.Height1) : null // Keep null if no valid records
                    })
                    .ToList();

                // Calculate weeklyRecords with non-null Indicator values
                var weeklyRecords = last6Weeks
                    .Select(week => new
                    {
                        Week = week,
                        Records = heightRecords
                            .Where(record => record.DateRecorded.HasValue &&
                                             record.DateRecorded.Value.Date >= week.StartOfWeek &&
                                             record.DateRecorded.Value.Date <= week.EndOfWeek)
                            .ToList()
                    })
                    .Select(x => new ChartDataModel
                    {
                                                Type = MapWeekToVietnamese( x.Week.WeekLabel),
                        Indicator = x.Records.Any() ? (double?)x.Records.Average(h => h.Height1) : null // Keep null if no valid records
                    })

                    .ToList();

                // Calculate monthlyRecords with non-null Indicator values
                var monthlyRecords = last4Months
                    .Select(month => new
                    {
                        Month = month,
                        Records = heightRecords
                            .Where(record => record.DateRecorded.HasValue &&
                                             record.DateRecorded.Value.Date >= month.StartOfMonth &&
                                             record.DateRecorded.Value.Date <= month.EndOfMonth)
                            .ToList()
                    })
                    .Select(x => new ChartDataModel
                    {
                        Type = MapMonthToVietnamese( x.Month.MonthLabel),
                        Indicator = x.Records.Any() ? (double?)x.Records.Average(h => h.Height1) : null // Keep null if no valid records
                    })

                    .ToList();

                // Calculate yearlyRecords with non-null Indicator values
                var yearlyRecords = heightRecords
                    .GroupBy(h => h.DateRecorded.Value.Year)
                    .Select(g => new ChartDataModel
                    {
                        Type = g.Key.ToString(),
                        Indicator = g.Any() ? (double?)g.Average(h => h.Height1) : null // Keep null if no valid records
                    })
                    .OrderBy(record => int.Parse(record.Type))
                    .ToList();

                // Calculate averages for each tab (only non-null Indicators)
                var dailyAverage = dailyRecords
                    .Where(d => d.Indicator.HasValue)
                    .DefaultIfEmpty(new ChartDataModel { Indicator = 0 }) // Default to 0 if all are null
                    .Average(d => d.Indicator.Value);

                var weeklyAverage = weeklyRecords
                    .Where(w => w.Indicator.HasValue)
                    .DefaultIfEmpty(new ChartDataModel { Indicator = 0 }) // Default to 0 if all are null
                    .Average(w => w.Indicator.Value);

                var monthlyAverage = monthlyRecords
                    .Where(m => m.Indicator.HasValue)
                    .DefaultIfEmpty(new ChartDataModel { Indicator = 0 }) // Default to 0 if all are null
                    .Average(m => m.Indicator.Value);

                var yearlyAverage = yearlyRecords
                    .Where(y => y.Indicator.HasValue)
                    .DefaultIfEmpty(new ChartDataModel { Indicator = 0 }) // Default to 0 if all are null
                    .Average(y => y.Indicator.Value);

                // Calculate BMI for each period using the newest weight
                var bmiForCurrentDay = CalculateBMI(newestWeight, (decimal)dailyRecords
                    .Where(d => d.Indicator.HasValue)
                    .DefaultIfEmpty(new ChartDataModel { Indicator = 0 })
                    .Average(d => d.Indicator.Value) / 100);

                var bmiForCurrentWeek = CalculateBMI(newestWeight, (decimal)weeklyRecords
                    .Where(w => w.Indicator.HasValue)
                    .DefaultIfEmpty(new ChartDataModel { Indicator = 0 })
                    .Average(w => w.Indicator.Value) / 100);

                var bmiForCurrentMonth = CalculateBMI(newestWeight, (decimal)monthlyRecords
                    .Where(m => m.Indicator.HasValue)
                    .DefaultIfEmpty(new ChartDataModel { Indicator = 0 })
                    .Average(m => m.Indicator.Value) / 100);

                var bmiForCurrentYear = CalculateBMI(newestWeight, (decimal)yearlyRecords
                    .Where(y => y.Indicator.HasValue)
                    .DefaultIfEmpty(new ChartDataModel { Indicator = 0 })
                    .Average(y => y.Indicator.Value) / 100);

                var responseList = new List<GetHealthIndicatorDetailReponse>
        {
            new GetHealthIndicatorDetailReponse
            {
                Tabs = "Ngày",
                Average = dailyAverage,
                Evaluation = bmiForCurrentDay.ToString(),
                ChartDatabase = dailyRecords
            },
            new GetHealthIndicatorDetailReponse
            {
                Tabs = "Tuần",
                Average = weeklyAverage,
                Evaluation = bmiForCurrentWeek.ToString(),
                ChartDatabase = weeklyRecords
            },
            new GetHealthIndicatorDetailReponse
            {
                Tabs = "Tháng",
                Average = monthlyAverage,
                Evaluation = bmiForCurrentMonth.ToString(),
                ChartDatabase = monthlyRecords
            },
            new GetHealthIndicatorDetailReponse
            {
                Tabs = "Năm",
                Average = yearlyAverage,
                Evaluation = bmiForCurrentYear.ToString(),
                ChartDatabase = yearlyRecords
            }
        };

                return new BusinessResult(Const.SUCCESS_READ, "Height details retrieved successfully.", responseList);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetHeartRateDetail(int accountId)
        {
            try
            {
                var elderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(accountId);

                if (elderly == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                var heartRateRecords = await _unitOfWork.HeartRateRepository
                    .FindByConditionAsync(h => h.ElderlyId == elderly.Elderly.ElderlyId && h.Status == SD.GeneralStatus.ACTIVE);

                if (!heartRateRecords.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, "No heart rate records found for the elderly.");
                }

                var today = System.DateTime.UtcNow.AddHours(7);

                var last7Days = Enumerable.Range(0, 7)
                    .Select(offset => today.AddDays(-offset).Date)
                    .OrderBy(date => date)
                    .ToList();

                var last6Weeks = Enumerable.Range(0, 6)
                    .Select(offset => today.AddDays(-(offset * 7)))
                    .OrderBy(date => date)
                    .Select(date => new
                    {
                        StartOfWeek = date.AddDays(-(int)date.DayOfWeek + (int)System.DayOfWeek.Monday),
                        EndOfWeek = date.AddDays(-(int)date.DayOfWeek + (int)System.DayOfWeek.Monday + 6),
                        WeekLabel = $"Week {CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, System.DayOfWeek.Monday)}"
                    })
                    .ToList();

                var last4Months = Enumerable.Range(0, 4)
                    .Select(offset => today.AddMonths(-offset))
                    .OrderBy(date => date)
                    .Select(date => new
                    {
                        StartOfMonth = new System.DateTime(date.Year, date.Month, 1),
                        EndOfMonth = new System.DateTime(date.Year, date.Month, System.DateTime.DaysInMonth(date.Year, date.Month)),
                        MonthLabel = $"{CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(date.Month)}"
                    })
                    .ToList();

                var dailyRecords = last7Days
                    .Select(date => new
                    {
                        Date = date,
                        Records = heartRateRecords
                            .Where(record => record.DateRecorded.HasValue && record.DateRecorded.Value.Date == date)
                            .ToList()
                    })
                    .Select(x => new ChartDataModel
                    {
                        Type = MapDayOfWeekToVietnamese(x.Date.DayOfWeek),
                        Indicator = x.Records.Any() ? (double?)Math.Round(x.Records.Average(h => h.HeartRate1 ?? 0)) : null
                    })
                    .ToList();

                var weeklyRecords = last6Weeks
                    .Select(week => new
                    {
                        Week = week,
                        Records = heartRateRecords
                            .Where(record => record.DateRecorded.HasValue &&
                                             record.DateRecorded.Value.Date >= week.StartOfWeek &&
                                             record.DateRecorded.Value.Date <= week.EndOfWeek)
                            .ToList()
                    })
                    .Select(x => new ChartDataModel
                    {
                                                Type = MapWeekToVietnamese( x.Week.WeekLabel),
                        Indicator = x.Records.Any() ? (double?)Math.Round(x.Records.Average(h => h.HeartRate1 ?? 0),2) : null
                    })

                    .ToList();

                var monthlyRecords = last4Months
                    .Select(month => new
                    {
                        Month = month,
                        Records = heartRateRecords
                            .Where(record => record.DateRecorded.HasValue &&
                                             record.DateRecorded.Value.Date >= month.StartOfMonth &&
                                             record.DateRecorded.Value.Date <= month.EndOfMonth)
                            .ToList()
                    })
                    .Select(x => new ChartDataModel
                    {
                        Type = MapMonthToVietnamese( x.Month.MonthLabel),
                        Indicator = x.Records.Any() ? (double?)Math.Round(x.Records.Average(h => h.HeartRate1 ?? 0),2) : null
                    })

                    .ToList();

                var yearlyRecords = heartRateRecords
                    .GroupBy(h => h.DateRecorded.Value.Year)
                    .Select(g => new ChartDataModel
                    {
                        Type = g.Key.ToString(),
                        Indicator = (double?)Math.Round(g.Average(h => h.HeartRate1 ?? 0),2)
                    })
                    .OrderBy(record => int.Parse(record.Type))
                    .ToList();

                var dailyAverage = dailyRecords
                    .Where(d => d.Indicator.HasValue)
                    .DefaultIfEmpty(new ChartDataModel { Indicator = 0 })
                    .Average(d => d.Indicator.Value);

                var weeklyAverage = weeklyRecords
                    .Where(w => w.Indicator.HasValue)
                    .DefaultIfEmpty(new ChartDataModel { Indicator = 0 })
                    .Average(w => w.Indicator.Value);

                var monthlyAverage = monthlyRecords
                    .Where(m => m.Indicator.HasValue)
                    .DefaultIfEmpty(new ChartDataModel { Indicator = 0 })
                    .Average(m => m.Indicator.Value);

                var yearlyAverage = yearlyRecords
                    .Where(y => y.Indicator.HasValue)
                    .DefaultIfEmpty(new ChartDataModel { Indicator = 0 })
                    .Average(y => y.Indicator.Value);

                var dailyEvaluation = GetHeartRateEvaluation(dailyAverage);
                var weeklyEvaluation = GetHeartRateEvaluation(weeklyAverage);
                var monthlyEvaluation = GetHeartRateEvaluation(monthlyAverage);
                var yearlyEvaluation = GetHeartRateEvaluation(yearlyAverage);

                var responseList = new List<GetHealthIndicatorDetailReponse>
        {
            new GetHealthIndicatorDetailReponse
            {
                Tabs = "Ngày",
                Average = Math.Round(dailyAverage, 2),
                Evaluation = dailyEvaluation,
                ChartDatabase = dailyRecords
            },
            new GetHealthIndicatorDetailReponse
            {
                Tabs = "Tuần",
                Average = Math.Round(weeklyAverage, 2),
                Evaluation = weeklyEvaluation,
                ChartDatabase = weeklyRecords
            },
            new GetHealthIndicatorDetailReponse
            {
                Tabs = "Tháng",
                Average = Math.Round(monthlyAverage, 2),
                Evaluation = monthlyEvaluation,
                ChartDatabase = monthlyRecords
            },
            new GetHealthIndicatorDetailReponse
            {
                Tabs = "Năm",
                Average = Math.Round(yearlyAverage, 2),
                Evaluation = yearlyEvaluation,
                ChartDatabase = yearlyRecords
            }
        };

                return new BusinessResult(Const.SUCCESS_READ, "Heart rate details retrieved successfully.", responseList);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        private string GetHeartRateEvaluation(double averageHeartRate)

        {
            var baseHeal = _unitOfWork.HealthIndicatorBaseRepository.FindByCondition(x => x.Type == "HeartRate").FirstOrDefault();
            if ((decimal)averageHeartRate < baseHeal.MinValue)
            {
                return "Thấp";
            }
            else if ((decimal)averageHeartRate >= baseHeal.MinValue && (decimal)averageHeartRate <= baseHeal.MaxValue)
            {
                return "Trung bình";
            }
            else
            {
                return "Cao";
            }
        }

        public async Task<IBusinessResult> GetBloodPressureDetail(int accountId)
        {
            try
            {
                var elderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(accountId);

                if (elderly == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                var bloodPressureRecords = await _unitOfWork.BloodPressureRepository
                    .FindByConditionAsync(b => b.ElderlyId == elderly.Elderly.ElderlyId && b.Status == SD.GeneralStatus.ACTIVE);

                if (!bloodPressureRecords.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, "No blood pressure records found for the elderly.");
                }

                var today = System.DateTime.UtcNow.AddHours(7);

                var last7Days = Enumerable.Range(0, 7)
                    .Select(offset => today.AddDays(-offset).Date)
                    .OrderBy(date => date)
                    .ToList();

                var last6Weeks = Enumerable.Range(0, 6)
                    .Select(offset => today.AddDays(-(offset * 7)))
                    .OrderBy(date => date)
                    .Select(date => new
                    {
                        StartOfWeek = date.AddDays(-(int)date.DayOfWeek + (int)System.DayOfWeek.Monday),
                        EndOfWeek = date.AddDays(-(int)date.DayOfWeek + (int)System.DayOfWeek.Monday + 6),
                        WeekLabel = $"Week {CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, System.DayOfWeek.Monday)}"
                    })
                    .ToList();

                var last4Months = Enumerable.Range(0, 4)
                    .Select(offset => today.AddMonths(-offset))
                    .OrderBy(date => date)
                    .Select(date => new
                    {
                        StartOfMonth = new System.DateTime(date.Year, date.Month, 1),
                        EndOfMonth = new System.DateTime(date.Year, date.Month, System.DateTime.DaysInMonth(date.Year, date.Month)),
                        MonthLabel = $"{CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(date.Month)}"
                    })
                    .ToList();

                // Calculate dailyRecords with non-null Indicator values
                var dailyRecords = last7Days
                    .Select(date => new
                    {
                        Date = date,
                        Records = bloodPressureRecords
                            .Where(record => record.DateRecorded.HasValue && record.DateRecorded.Value.Date == date)
                            .ToList()
                    })
                    .Select(x => new ChartBloodPressureModel
                    {
                        Type = MapDayOfWeekToVietnamese(x.Date.DayOfWeek),
                        Indicator = x.Records.Any() ? $"{Math.Round(x.Records.Average(b => b.Systolic ?? 0), MidpointRounding.AwayFromZero)}/{Math.Round(x.Records.Average(b => b.Diastolic ?? 0), MidpointRounding.AwayFromZero)}" : null
                    })
                    .ToList();

                // Calculate weeklyRecords with non-null Indicator values
                var weeklyRecords = last6Weeks
                    .Select(week => new
                    {
                        Week = week,
                        Records = bloodPressureRecords
                            .Where(record => record.DateRecorded.HasValue &&
                                             record.DateRecorded.Value.Date >= week.StartOfWeek &&
                                             record.DateRecorded.Value.Date <= week.EndOfWeek)
                            .ToList()
                    })
                    .Select(x => new ChartBloodPressureModel
                    {
                                                Type = MapWeekToVietnamese( x.Week.WeekLabel),
                        Indicator = x.Records.Any() ? $"{Math.Round(x.Records.Average(b => b.Systolic ?? 0), MidpointRounding.AwayFromZero)}/{Math.Round(x.Records.Average(b => b.Diastolic ?? 0), MidpointRounding.AwayFromZero)}" : null
                    })

                    .ToList();

                // Calculate monthlyRecords with non-null Indicator values
                var monthlyRecords = last4Months
                    .Select(month => new
                    {
                        Month = month,
                        Records = bloodPressureRecords
                            .Where(record => record.DateRecorded.HasValue &&
                                             record.DateRecorded.Value.Date >= month.StartOfMonth &&
                                             record.DateRecorded.Value.Date <= month.EndOfMonth)
                            .ToList()
                    })
                    .Select(x => new ChartBloodPressureModel
                    {
                        Type = MapMonthToVietnamese( x.Month.MonthLabel),
                        Indicator = x.Records.Any() ? $"{Math.Round(x.Records.Average(b => b.Systolic ?? 0), MidpointRounding.AwayFromZero)}/{Math.Round(x.Records.Average(b => b.Diastolic ?? 0), MidpointRounding.AwayFromZero)}" : null
                    })

                    .ToList();

                // Calculate yearlyRecords with non-null Indicator values
                var yearlyRecords = bloodPressureRecords
                    .GroupBy(b => b.DateRecorded.Value.Year)
                    .Select(g => new ChartBloodPressureModel
                    {
                        Type = g.Key.ToString(),
                        Indicator = $"{Math.Round(g.Average(b => b.Systolic ?? 0), MidpointRounding.AwayFromZero)}/{Math.Round(g.Average(b => b.Diastolic ?? 0), MidpointRounding.AwayFromZero)}"
                    })
                    .OrderBy(record => int.Parse(record.Type))
                    .ToList();

                // Calculate averages for each tab (only non-null Indicators)
                var dailyAverageSystolic = dailyRecords
                    .Where(d => d.Indicator != null)
                    .DefaultIfEmpty(new ChartBloodPressureModel { Indicator = "0/0" })
                    .Average(d => double.Parse(d.Indicator.Split('/')[0]));

                var dailyAverageDiastolic = dailyRecords
                    .Where(d => d.Indicator != null)
                    .DefaultIfEmpty(new ChartBloodPressureModel { Indicator = "0/0" })
                    .Average(d => double.Parse(d.Indicator.Split('/')[1]));

                var weeklyAverageSystolic = weeklyRecords
                    .Where(w => w.Indicator != null)
                    .DefaultIfEmpty(new ChartBloodPressureModel { Indicator = "0/0" })
                    .Average(w => double.Parse(w.Indicator.Split('/')[0]));

                var weeklyAverageDiastolic = weeklyRecords
                    .Where(w => w.Indicator != null)
                    .DefaultIfEmpty(new ChartBloodPressureModel { Indicator = "0/0" })
                    .Average(w => double.Parse(w.Indicator.Split('/')[1]));

                var monthlyAverageSystolic = monthlyRecords
                    .Where(m => m.Indicator != null)
                    .DefaultIfEmpty(new ChartBloodPressureModel { Indicator = "0/0" })
                    .Average(m => double.Parse(m.Indicator.Split('/')[0]));

                var monthlyAverageDiastolic = monthlyRecords
                    .Where(m => m.Indicator != null)
                    .DefaultIfEmpty(new ChartBloodPressureModel { Indicator = "0/0" })
                    .Average(m => double.Parse(m.Indicator.Split('/')[1]));

                var yearlyAverageSystolic = yearlyRecords
                    .Where(y => y.Indicator != null)
                    .DefaultIfEmpty(new ChartBloodPressureModel { Indicator = "0/0" })
                    .Average(y => double.Parse(y.Indicator.Split('/')[0]));

                var yearlyAverageDiastolic = yearlyRecords
                    .Where(y => y.Indicator != null)
                    .DefaultIfEmpty(new ChartBloodPressureModel { Indicator = "0/0" })
                    .Average(y => double.Parse(y.Indicator.Split('/')[1]));

                // Format averages as "Systolic/Diastolic" with whole numbers
                var dailyAverage = $"{Math.Round(dailyAverageSystolic, MidpointRounding.AwayFromZero)}/{Math.Round(dailyAverageDiastolic, MidpointRounding.AwayFromZero)}";
                var weeklyAverage = $"{Math.Round(weeklyAverageSystolic, MidpointRounding.AwayFromZero)}/{Math.Round(weeklyAverageDiastolic, MidpointRounding.AwayFromZero)}";
                var monthlyAverage = $"{Math.Round(monthlyAverageSystolic, MidpointRounding.AwayFromZero)}/{Math.Round(monthlyAverageDiastolic, MidpointRounding.AwayFromZero)}";
                var yearlyAverage = $"{Math.Round(yearlyAverageSystolic, MidpointRounding.AwayFromZero)}/{Math.Round(yearlyAverageDiastolic, MidpointRounding.AwayFromZero)}";

                // Calculate Evaluation for each tab
                var dailyEvaluation = GetBloodPressureEvaluation(dailyAverageSystolic, dailyAverageDiastolic);
                var weeklyEvaluation = GetBloodPressureEvaluation(weeklyAverageSystolic, weeklyAverageDiastolic);
                var monthlyEvaluation = GetBloodPressureEvaluation(monthlyAverageSystolic, monthlyAverageDiastolic);
                var yearlyEvaluation = GetBloodPressureEvaluation(yearlyAverageSystolic, yearlyAverageDiastolic);

                var responseList = new List<GetBloodPressureDetail>
        {
            new GetBloodPressureDetail
            {
                Tabs = "Ngày",
                Average = dailyAverage,
                Evaluation = dailyEvaluation,
                ChartDatabase = dailyRecords
            },
            new GetBloodPressureDetail
            {
                Tabs = "Tuần",
                Average = weeklyAverage,
                Evaluation = weeklyEvaluation,
                ChartDatabase = weeklyRecords
            },
            new GetBloodPressureDetail
            {
                Tabs = "Tháng",
                Average = monthlyAverage,
                Evaluation = monthlyEvaluation,
                ChartDatabase = monthlyRecords
            },
            new GetBloodPressureDetail
            {
                Tabs = "Năm",
                Average = yearlyAverage,
                Evaluation = yearlyEvaluation,
                ChartDatabase = yearlyRecords
            }
        };

                return new BusinessResult(Const.SUCCESS_READ, "Blood pressure details retrieved successfully.", responseList);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        private string GetBloodPressureEvaluation(double averageSystolic, double averageDiastolic)
        {

            var baseSystolic = _unitOfWork.HealthIndicatorBaseRepository.
                                                   FindByCondition(i => i.Type == "Systolic")
                                                   .FirstOrDefault();
            var baseDiastolic = _unitOfWork.HealthIndicatorBaseRepository.
                                                FindByCondition(i => i.Type == "Diastolic")
                                                .FirstOrDefault();
            string result;
            if ((decimal)averageSystolic < baseSystolic.MaxValue && (decimal)averageDiastolic < baseDiastolic.MaxValue)
            {
                result = "Bình thường";
            }
            else if ((decimal)averageSystolic < baseSystolic.MinValue && (decimal)averageDiastolic < baseDiastolic.MinValue)
            {
                result = "Thấp";
            }
            else
            {
                result = "Cao";
            }
            return result;
        }

        public async Task<IBusinessResult> GetBloodGlucoseDetail(int accountId)
        {
            try
            {
                var elderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(accountId);

                if (elderly == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                var bloodGlucoseRecords = new List<BloodGlucose>();
                bloodGlucoseRecords = _unitOfWork.BloodGlucoseRepository
                    .FindByCondition(b => b.ElderlyId == elderly.Elderly.ElderlyId && b.Status == SD.GeneralStatus.ACTIVE).ToList();

                if (!bloodGlucoseRecords.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, "No blood glucose records found for the elderly.");
                }

                // Fetch all HealthIndicatorBase records for BloodGlucose
                var healthIndicators = _unitOfWork.HealthIndicatorBaseRepository
                    .FindByCondition(h => h.Type == "BloodGlucose" && h.Status == SD.GeneralStatus.ACTIVE)
                    .ToList();

                if (!healthIndicators.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, "No health indicators found for BloodGlucose.");
                }

                var today = System.DateTime.UtcNow.AddHours(7);

                var last7Days = Enumerable.Range(0, 7)
                    .Select(offset => today.AddDays(-offset).Date)
                    .OrderBy(date => date)
                    .ToList();

                var last6Weeks = Enumerable.Range(0, 6)
                    .Select(offset => today.AddDays(-(offset * 7)))
                    .OrderBy(date => date)
                    .Select(date => new
                    {
                        StartOfWeek = date.AddDays(-(int)date.DayOfWeek + (int)System.DayOfWeek.Monday),
                        EndOfWeek = date.AddDays(-(int)date.DayOfWeek + (int)System.DayOfWeek.Monday + 6),
                        WeekLabel = $"Week {CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, System.DayOfWeek.Monday)}"
                    })
                    .ToList();

                var last4Months = Enumerable.Range(0, 4)
                    .Select(offset => today.AddMonths(-offset))
                    .OrderBy(date => date)
                    .Select(date => new
                    {
                        StartOfMonth = new System.DateTime(date.Year, date.Month, 1),
                        EndOfMonth = new System.DateTime(date.Year, date.Month, System.DateTime.DaysInMonth(date.Year, date.Month)),
                        MonthLabel = $"{CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(date.Month)}"
                    })
                    .ToList();
                // Function to calculate evaluation percentages
                (double LowPercent, double NormalPercent, double HighPercent) CalculateEvaluationPercentages(List<BloodGlucose> records, List<HealthIndicatorBase> indicators)
                {
                    int totalRecords = records.Count;
                    if (totalRecords == 0)
                        return (0, 0, 0);

                    int lowCount = 0, normalCount = 0, highCount = 0;

                    foreach (var record in records)
                    {
                        var indicator = indicators.FirstOrDefault(h => h.Time == record.Time);
                        if (indicator == null)
                            continue;

                        if (record.BloodGlucose1 < indicator.MinValue)
                            lowCount++;
                        else if (record.BloodGlucose1 >= indicator.MinValue && record.BloodGlucose1 <= indicator.MaxValue)
                            normalCount++;
                        else if (record.BloodGlucose1 > indicator.MaxValue)
                            highCount++;
                    }

                    // Calculate percentages
                    double lowPercent = (double)lowCount / totalRecords * 100;
                    double normalPercent = (double)normalCount / totalRecords * 100;
                    double highPercent = (double)highCount / totalRecords * 100;

                    // Ensure the sum is 100% (due to rounding errors)
                    double sum = lowPercent + normalPercent + highPercent;
                    if (sum != 100.0)
                    {
                        // Adjust the largest percentage to make the sum 100%
                        if (lowPercent >= normalPercent && lowPercent >= highPercent)
                            lowPercent += 100.0 - sum;
                        else if (normalPercent >= lowPercent && normalPercent >= highPercent)
                            normalPercent += 100.0 - sum;
                        else
                            highPercent += 100.0 - sum;
                    }

                    return (Math.Round( lowPercent,2), Math.Round(normalPercent,2), Math.Round(highPercent,2));
                }
                // Function to calculate highest, lowest, and average
                (double Highest, double Lowest, double Average) CalculateStatistics(List<BloodGlucose> records)
                {
                    if (!records.Any())
                        return (0, 0, 0);

                    var values = records.Select(r => (double)r.BloodGlucose1).ToList();
                    return (
                        Highest: Math.Round(values.Max(), MidpointRounding.AwayFromZero),
                        Lowest: Math.Round(values.Min(), MidpointRounding.AwayFromZero),
                        Average: Math.Round(values.Average(), MidpointRounding.AwayFromZero)
                    );
                }

                // Calculate dailyRecords
                var dailyRecords = last7Days
                    .Select(date => new
                    {
                        Date = date,
                        Records = bloodGlucoseRecords
                            .Where(record => record.DateRecorded.HasValue && record.DateRecorded.Value.Date == date)
                            .ToList()
                    })
                    .Select(x => new ChartBloodGlucoseModel
                    {
                        Type = MapDayOfWeekToVietnamese(x.Date.DayOfWeek),
                        Indicator = x.Records.Any() ? (double?)Math.Round(x.Records.Average(b => b.BloodGlucose1 ?? 0), MidpointRounding.AwayFromZero) : null
                    })
                    .ToList();

                var dailyStats = CalculateStatistics(bloodGlucoseRecords.Where(r => last7Days.Contains(r.DateRecorded.Value.Date)).ToList());
                var dailyEvaluation = CalculateEvaluationPercentages(bloodGlucoseRecords.Where(r => last7Days.Contains(r.DateRecorded.Value.Date)).ToList(), healthIndicators);

                // Calculate weeklyRecords
                var weeklyRecords = last6Weeks
                    .Select(week => new
                    {
                        Week = week,
                        Records = bloodGlucoseRecords
                            .Where(record => record.DateRecorded.HasValue &&
                                             record.DateRecorded.Value.Date >= week.StartOfWeek &&
                                             record.DateRecorded.Value.Date <= week.EndOfWeek)
                            .ToList()
                    })
                    .Select(x => new ChartBloodGlucoseModel
                    {
                                                Type = MapWeekToVietnamese( x.Week.WeekLabel),
                        Indicator = x.Records.Any() ? (double?)Math.Round(x.Records.Average(b => b.BloodGlucose1 ?? 0), MidpointRounding.AwayFromZero) : null
                    })

                    .ToList();

                var weeklyStats = CalculateStatistics(bloodGlucoseRecords.Where(r => last6Weeks.Any(w => r.DateRecorded.Value.Date >= w.StartOfWeek && r.DateRecorded.Value.Date <= w.EndOfWeek)).ToList());
                var weeklyEvaluation = CalculateEvaluationPercentages(bloodGlucoseRecords.Where(r => last6Weeks.Any(w => r.DateRecorded.Value.Date >= w.StartOfWeek && r.DateRecorded.Value.Date <= w.EndOfWeek)).ToList(), healthIndicators);

                // Calculate monthlyRecords
                var monthlyRecords = last4Months
                    .Select(month => new
                    {
                        Month = month,
                        Records = bloodGlucoseRecords
                            .Where(record => record.DateRecorded.HasValue &&
                                             record.DateRecorded.Value.Date >= month.StartOfMonth &&
                                             record.DateRecorded.Value.Date <= month.EndOfMonth)
                            .ToList()
                    })
                    .Select(x => new ChartBloodGlucoseModel
                    {
                        Type = MapMonthToVietnamese( x.Month.MonthLabel),
                        Indicator = x.Records.Any() ? (double?)Math.Round(x.Records.Average(b => b.BloodGlucose1 ?? 0), MidpointRounding.AwayFromZero) : null
                    })

                    .ToList();

                var monthlyStats = CalculateStatistics(bloodGlucoseRecords.Where(r => last4Months.Any(m => r.DateRecorded.Value.Date >= m.StartOfMonth && r.DateRecorded.Value.Date <= m.EndOfMonth)).ToList());
                var monthlyEvaluation = CalculateEvaluationPercentages(bloodGlucoseRecords.Where(r => last4Months.Any(m => r.DateRecorded.Value.Date >= m.StartOfMonth && r.DateRecorded.Value.Date <= m.EndOfMonth)).ToList(), healthIndicators);

                // Calculate yearlyRecords
                var yearlyRecords = bloodGlucoseRecords
                    .GroupBy(b => b.DateRecorded.Value.Year)
                    .Select(g => new ChartBloodGlucoseModel
                    {
                        Type = g.Key.ToString(),
                        Indicator = (double?)Math.Round(g.Average(b => b.BloodGlucose1 ?? 0), MidpointRounding.AwayFromZero)
                    })
                    .OrderBy(record => int.Parse(record.Type))
                    .ToList();

                var yearlyStats = CalculateStatistics(bloodGlucoseRecords);
                var yearlyEvaluation = CalculateEvaluationPercentages(bloodGlucoseRecords, healthIndicators);

                var responseList = new List<GetBloodGlucoseResponse>
        {
            new GetBloodGlucoseResponse
            {
                Tabs = "Ngày",
                Highest = dailyStats.Highest,
                Lowest = dailyStats.Lowest,
                Average = dailyStats.Average,
                HighestPercent = dailyEvaluation.HighPercent,
                LowestPercent = dailyEvaluation.LowPercent,
                NormalPercent = dailyEvaluation.NormalPercent,
                ChartDatabase = dailyRecords
            },
            new GetBloodGlucoseResponse
            {
                Tabs = "Tuần",
                Highest = weeklyStats.Highest,
                Lowest = weeklyStats.Lowest,
                Average = weeklyStats.Average,
                HighestPercent = weeklyEvaluation.HighPercent,
                LowestPercent = weeklyEvaluation.LowPercent,
                NormalPercent = weeklyEvaluation.NormalPercent,
                ChartDatabase = weeklyRecords
            },
            new GetBloodGlucoseResponse
            {
                Tabs = "Tháng",
                Highest = monthlyStats.Highest,
                Lowest = monthlyStats.Lowest,
                Average = monthlyStats.Average,
                HighestPercent = monthlyEvaluation.HighPercent,
                LowestPercent = monthlyEvaluation.LowPercent,
                NormalPercent = monthlyEvaluation.NormalPercent,
                ChartDatabase = monthlyRecords
            },
            new GetBloodGlucoseResponse
            {
                Tabs = "Năm",
                Highest = yearlyStats.Highest,
                Lowest = yearlyStats.Lowest,
                Average = yearlyStats.Average,
                HighestPercent = yearlyEvaluation.HighPercent,
                LowestPercent = yearlyEvaluation.LowPercent,
                NormalPercent = yearlyEvaluation.NormalPercent,
                ChartDatabase = yearlyRecords
            }
        };

                return new BusinessResult(Const.SUCCESS_READ, "Blood glucose details retrieved successfully.", responseList);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }
        public async Task<IBusinessResult> GetLipidProfileDetail(int accountId)
        {
            try
            {
                var elderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(accountId);

                if (elderly == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                var lipidProfileRecords = await _unitOfWork.LipidProfileRepository
                    .FindByConditionAsync(l => l.ElderlyId == elderly.Elderly.ElderlyId && l.Status == SD.GeneralStatus.ACTIVE);

                if (!lipidProfileRecords.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, "No lipid profile records found for the elderly.");
                }

                var today = System.DateTime.UtcNow.AddHours(7);

                var last7Days = Enumerable.Range(0, 7)
                    .Select(offset => today.AddDays(-offset).Date)
                    .OrderBy(date => date)
                    .ToList();

                var last6Weeks = Enumerable.Range(0, 6)
                    .Select(offset => today.AddDays(-(offset * 7)))
                    .OrderBy(date => date)
                    .Select(date => new
                    {
                        StartOfWeek = date.AddDays(-(int)date.DayOfWeek + (int)System.DayOfWeek.Monday),
                        EndOfWeek = date.AddDays(-(int)date.DayOfWeek + (int)System.DayOfWeek.Monday + 6),
                        WeekLabel = $"Week {CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, System.DayOfWeek.Monday)}"
                    })
                    .ToList();

                var last4Months = Enumerable.Range(0, 4)
                    .Select(offset => today.AddMonths(-offset))
                    .OrderBy(date => date)
                    .Select(date => new
                    {
                        StartOfMonth = new System.DateTime(date.Year, date.Month, 1),
                        EndOfMonth = new System.DateTime(date.Year, date.Month, System.DateTime.DaysInMonth(date.Year, date.Month)),
                        MonthLabel = $"{CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(date.Month)}"
                    })
                    .ToList();

                // Calculate dailyRecords
                var dailyRecords = last7Days
                    .Select(date => new
                    {
                        Date = date,
                        Records = lipidProfileRecords
                            .Where(record => record.DateRecorded.HasValue && record.DateRecorded.Value.Date == date)
                            .ToList()
                    })
                    .Select(x => new CharLipidProfileModel
                    {
                        Type = MapDayOfWeekToVietnamese(x.Date.DayOfWeek),
                        TotalCholesterol = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.TotalCholesterol ?? 0), 2) : null,
                        Ldlcholesterol = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Ldlcholesterol ?? 0), 2) : null,
                        Hdlcholesterol = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Hdlcholesterol ?? 0), 2) : null,
                        Triglycerides = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Triglycerides ?? 0), 2) : null
                    })
                    .ToList();

                // Calculate weeklyRecords
                var weeklyRecords = last6Weeks
                    .Select(week => new
                    {
                        Week = week,
                        Records = lipidProfileRecords
                            .Where(record => record.DateRecorded.HasValue &&
                                             record.DateRecorded.Value.Date >= week.StartOfWeek &&
                                             record.DateRecorded.Value.Date <= week.EndOfWeek)
                            .ToList()
                    })
                    .Select(x => new CharLipidProfileModel
                    {
                                                Type = MapWeekToVietnamese( x.Week.WeekLabel),
                        TotalCholesterol = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.TotalCholesterol ?? 0), 2) : null,
                        Ldlcholesterol = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Ldlcholesterol ?? 0), 2) : null,
                        Hdlcholesterol = x.Records.Any() ? (decimal?)x.Records.Average(l => l.Hdlcholesterol ?? 0) : null,
                        Triglycerides = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Triglycerides ?? 0), 2) : null
                    })

                    .ToList();

                // Calculate monthlyRecords
                var monthlyRecords = last4Months
                    .Select(month => new
                    {
                        Month = month,
                        Records = lipidProfileRecords
                            .Where(record => record.DateRecorded.HasValue &&
                                             record.DateRecorded.Value.Date >= month.StartOfMonth &&
                                             record.DateRecorded.Value.Date <= month.EndOfMonth)
                            .ToList()
                    })
                    .Select(x => new CharLipidProfileModel
                    {
                        Type = MapMonthToVietnamese( x.Month.MonthLabel),
                        TotalCholesterol = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.TotalCholesterol ?? 0), 2) : null,
                        Ldlcholesterol = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Ldlcholesterol ?? 0), 2) : null,
                        Hdlcholesterol = x.Records.Any() ? (decimal?)x.Records.Average(l => l.Hdlcholesterol ?? 0) : null,
                        Triglycerides = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Triglycerides ?? 0), 2) : null
                    })

                    .ToList();

                // Calculate yearlyRecords
                var yearlyRecords = lipidProfileRecords
                    .GroupBy(l => l.DateRecorded.Value.Year)
                    .Select(g => new CharLipidProfileModel
                    {
                        Type = g.Key.ToString(),
                        TotalCholesterol = (decimal?)Math.Round(g.Average(l => l.TotalCholesterol ?? 0), 2),
                        Ldlcholesterol = (decimal?)Math.Round(g.Average(l => l.Ldlcholesterol ?? 0), 2),
                        Hdlcholesterol = (decimal?)g.Average(l => l.Hdlcholesterol ?? 0),
                        Triglycerides = (decimal?)Math.Round(g.Average(l => l.Triglycerides ?? 0), 2)
                    })
                    .OrderBy(record => int.Parse(record.Type))
                    .ToList();

                // Calculate percentages for each tab
                var dailyPercentages = CalculatePercentages(dailyRecords);
                var weeklyPercentages = CalculatePercentages(weeklyRecords);
                var monthlyPercentages = CalculatePercentages(monthlyRecords);
                var yearlyPercentages = CalculatePercentages(yearlyRecords);

                // Prepare response
                var responseList = new List<GetLipidProfileDetail>
        {
            new GetLipidProfileDetail
            {
                Tabs = "Ngày",
                TotalCholesterolAverage = dailyRecords.Average(d => d.TotalCholesterol ?? 0),
                LdlcholesterolAverage = dailyRecords.Average(d => d.Ldlcholesterol ?? 0),
                HdlcholesterolAverage = dailyRecords.Average(d => d.Hdlcholesterol ?? 0),
                TriglyceridesAverage = dailyRecords.Average(d => d.Triglycerides ?? 0),
                HighestPercent = dailyPercentages.HighestPercent,
                LowestPercent = dailyPercentages.LowestPercent,
                NormalPercent = dailyPercentages.NormalPercent,
                ChartDatabase = dailyRecords
            },
            new GetLipidProfileDetail
            {
                Tabs = "Tuần",
                TotalCholesterolAverage = weeklyRecords.Average(w => w.TotalCholesterol ?? 0),
                LdlcholesterolAverage = weeklyRecords.Average(w => w.Ldlcholesterol ?? 0),
                HdlcholesterolAverage = weeklyRecords.Average(w => w.Hdlcholesterol ?? 0),
                TriglyceridesAverage = weeklyRecords.Average(w => w.Triglycerides ?? 0),
                HighestPercent = weeklyPercentages.HighestPercent,
                LowestPercent = weeklyPercentages.LowestPercent,
                NormalPercent = weeklyPercentages.NormalPercent,
                ChartDatabase = weeklyRecords
            },
            new GetLipidProfileDetail
            {
                Tabs = "Tháng",
                TotalCholesterolAverage = monthlyRecords.Average(m => m.TotalCholesterol ?? 0),
                LdlcholesterolAverage = monthlyRecords.Average(m => m.Ldlcholesterol ?? 0),
                HdlcholesterolAverage = monthlyRecords.Average(m => m.Hdlcholesterol ?? 0),
                TriglyceridesAverage = monthlyRecords.Average(m => m.Triglycerides ?? 0),
                HighestPercent = monthlyPercentages.HighestPercent,
                LowestPercent = monthlyPercentages.LowestPercent,
                NormalPercent = monthlyPercentages.NormalPercent,
                ChartDatabase = monthlyRecords
            },
            new GetLipidProfileDetail
            {
                Tabs = "Năm",
                TotalCholesterolAverage = yearlyRecords.Average(y => y.TotalCholesterol ?? 0),
                LdlcholesterolAverage = yearlyRecords.Average(y => y.Ldlcholesterol ?? 0),
                HdlcholesterolAverage = yearlyRecords.Average(y => y.Hdlcholesterol ?? 0),
                TriglyceridesAverage = yearlyRecords.Average(y => y.Triglycerides ?? 0),
                HighestPercent = yearlyPercentages.HighestPercent,
                LowestPercent = yearlyPercentages.LowestPercent,
                NormalPercent = yearlyPercentages.NormalPercent,
                ChartDatabase = yearlyRecords
            }
        };

                return new BusinessResult(Const.SUCCESS_READ, "Lipid profile details retrieved successfully.", responseList);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        private (double HighestPercent, double LowestPercent, double NormalPercent) CalculatePercentages(List<CharLipidProfileModel> records)
        {
            int totalRecords = records.Count;
            if (totalRecords == 0)
                return (0, 0, 0);

            int highestCount = 0;
            int lowestCount = 0;
            int normalCount = 0;

            foreach (var record in records)
            {
                // Evaluate Total Cholesterol
                var totalCholesterolEvaluation = GetTotalCholesterolEvaluation(record.TotalCholesterol ?? 0);
                if (totalCholesterolEvaluation == "Cao") highestCount++;
                else if (totalCholesterolEvaluation == "Thấp") lowestCount++;
                else if (totalCholesterolEvaluation == "Bình thường") normalCount++;

                // Evaluate LDL Cholesterol
                var ldlCholesterolEvaluation = GetLdlcholesterolEvaluation(record.Ldlcholesterol ?? 0);
                if (ldlCholesterolEvaluation == "Cao") highestCount++;
                else if (ldlCholesterolEvaluation == "Thấp") lowestCount++;
                else if (ldlCholesterolEvaluation == "Bình thường") normalCount++;

                // Evaluate HDL Cholesterol
                var hdlCholesterolEvaluation = GetHdlcholesterolEvaluation(record.Hdlcholesterol ?? 0);
                if (hdlCholesterolEvaluation == "Cao") highestCount++;
                else if (hdlCholesterolEvaluation == "Thấp") lowestCount++;
                else if (hdlCholesterolEvaluation == "Bình thường") normalCount++;

                // Evaluate Triglycerides
                var triglyceridesEvaluation = GetTriglyceridesEvaluation(record.Triglycerides ?? 0);
                if (triglyceridesEvaluation == "Cao") highestCount++;
                else if (triglyceridesEvaluation == "Thấp") lowestCount++;
                else if (triglyceridesEvaluation == "Bình thường") normalCount++;
            }

            // Total evaluations = totalRecords * 4 (since there are 4 indicators)
            int totalEvaluations = totalRecords * 4;

            // Calculate percentages
            double highestPercent = (double)highestCount / totalEvaluations * 100;
            double lowestPercent = (double)lowestCount / totalEvaluations * 100;
            double normalPercent = (double)normalCount / totalEvaluations * 100;

            // Ensure the sum is 100% (due to rounding errors)
            double sum = highestPercent + lowestPercent + normalPercent;
            if (sum != 100.0)
            {
                // Adjust the largest percentage to make the sum 100%
                if (highestPercent >= lowestPercent && highestPercent >= normalPercent)
                    highestPercent += 100.0 - sum;
                else if (lowestPercent >= highestPercent && lowestPercent >= normalPercent)
                    lowestPercent += 100.0 - sum;
                else
                    normalPercent += 100.0 - sum;
            }

            return (Math.Round(highestPercent,2), Math.Round(lowestPercent,2), Math.Round(normalPercent,2));
        }
        private string GetTotalCholesterolEvaluation(decimal averageTotalCholesterol)
        {
            var baseHealth = _unitOfWork.HealthIndicatorBaseRepository
                   .FindByCondition(i => i.Type == "TotalCholesterol")
                   .FirstOrDefault();

            if (averageTotalCholesterol < baseHealth.MinValue)
            {
                return "Thấp";
            }
            else if (averageTotalCholesterol >= baseHealth.MinValue && averageTotalCholesterol < baseHealth.MaxValue)
            {
                return "Bình thường";
            }
            else if (averageTotalCholesterol >= baseHealth.MaxValue )
            {
                return "Cao";
            }
            return "Không xác định";
        }

        private string GetLdlcholesterolEvaluation(decimal averageLdlcholesterol)
        {
            var baseHealth = _unitOfWork.HealthIndicatorBaseRepository
                   .FindByCondition(i => i.Type == "LDLCholesterol")
                   .FirstOrDefault();
            if (averageLdlcholesterol < baseHealth.MinValue)
            {
                return "Thấp";
            }
            else if (averageLdlcholesterol >= baseHealth.MinValue && averageLdlcholesterol < baseHealth.MaxValue)
            {
                return "Bình thường";
            }
            else if (averageLdlcholesterol >= baseHealth.MaxValue)
            {
                return "Cao";
            }
          
            else
            {
                return "Không xác định";
            }
        }

        private string GetHdlcholesterolEvaluation(decimal averageHdlcholesterol)
        {
            var baseHealth = _unitOfWork.HealthIndicatorBaseRepository
                   .FindByCondition(i => i.Type == "HDLCholesterol")
                   .FirstOrDefault();
            if (averageHdlcholesterol < baseHealth.MinValue)
            {
                return "Thấp";
            }
            else if (averageHdlcholesterol >= baseHealth.MinValue && averageHdlcholesterol < baseHealth.MaxValue)
            {
                return "Bình thường";
            }
            else if (averageHdlcholesterol >= baseHealth.MaxValue)
            {
                return "Cao tốt";
            }
            else
            {
                return "Không xác định";
            }
        }

        private string GetTriglyceridesEvaluation(decimal averageTriglycerides)
        {
            var baseHealth = _unitOfWork.HealthIndicatorBaseRepository
                   .FindByCondition(i => i.Type == "Triglycerides")
                   .FirstOrDefault();
            if (averageTriglycerides < baseHealth.MinValue)
            {
                return "Thấp";
            }
            else if (averageTriglycerides >= baseHealth.MinValue && averageTriglycerides < baseHealth.MaxValue)
            {
                return "Bình thường";
            }
            else if (averageTriglycerides >= baseHealth.MaxValue)
            {
                return "Cao";
            }
            else
            {
                return "Không xác định";
            }
        }

        public async Task<IBusinessResult> GetLiverEnzymesDetail(int accountId)
        {
            try
            {
                var elderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(accountId);

                if (elderly == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                var liverEnzymeRecords = await _unitOfWork.LiverEnzymeRepository
                    .FindByConditionAsync(l => l.ElderlyId == elderly.Elderly.ElderlyId && l.Status == SD.GeneralStatus.ACTIVE);

                if (!liverEnzymeRecords.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, "No liver enzyme records found for the elderly.");
                }

                var today = System.DateTime.UtcNow.AddHours(7);

                var last7Days = Enumerable.Range(0, 7)
                    .Select(offset => today.AddDays(-offset).Date)
                    .OrderBy(date => date)
                    .ToList();

                var last6Weeks = Enumerable.Range(0, 6)
                    .Select(offset => today.AddDays(-(offset * 7)))
                    .OrderBy(date => date)
                    .Select(date => new
                    {
                        StartOfWeek = date.AddDays(-(int)date.DayOfWeek + (int)System.DayOfWeek.Monday),
                        EndOfWeek = date.AddDays(-(int)date.DayOfWeek + (int)System.DayOfWeek.Monday + 6),
                        WeekLabel = $"Week {CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, System.DayOfWeek.Monday)}"
                    })
                    .ToList();

                var last4Months = Enumerable.Range(0, 4)
                    .Select(offset => today.AddMonths(-offset))
                    .OrderBy(date => date)
                    .Select(date => new
                    {
                        StartOfMonth = new System.DateTime(date.Year, date.Month, 1),
                        EndOfMonth = new System.DateTime(date.Year, date.Month, System.DateTime.DaysInMonth(date.Year, date.Month)),
                        MonthLabel = $"{CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(date.Month)}"
                    })
                    .ToList();

                // Calculate dailyRecords
                var dailyRecords = last7Days
                    .Select(date => new
                    {
                        Date = date,
                        Records = liverEnzymeRecords
                            .Where(record => record.DateRecorded.HasValue && record.DateRecorded.Value.Date == date)
                            .ToList()
                    })
                    .Select(x => new CharLiverEnzymesModel
                    {
                        Type = MapDayOfWeekToVietnamese(x.Date.DayOfWeek),
                        Alt = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Alt ?? 0), 2) : null,
                        Ast = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Ast ?? 0), 2) : null,
                        Alp = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Alp ?? 0), 2) : null,
                        Ggt = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Ggt ?? 0), 2) : null
                    })
                    .ToList();

                // Calculate weeklyRecords
                var weeklyRecords = last6Weeks
                    .Select(week => new
                    {
                        Week = week,
                        Records = liverEnzymeRecords
                            .Where(record => record.DateRecorded.HasValue &&
                                             record.DateRecorded.Value.Date >= week.StartOfWeek &&
                                             record.DateRecorded.Value.Date <= week.EndOfWeek)
                            .ToList()
                    })
                    .Select(x => new CharLiverEnzymesModel
                    {
                                                Type = MapWeekToVietnamese( x.Week.WeekLabel),
                        Alt = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Alt ?? 0), 2) : null,
                        Ast = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Ast ?? 0), 2) : null,
                        Alp = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Alp ?? 0), 2) : null,
                        Ggt = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Ggt ?? 0), 2) : null
                    })

                    .ToList();

                // Calculate monthlyRecords
                var monthlyRecords = last4Months
                    .Select(month => new
                    {
                        Month = month,
                        Records = liverEnzymeRecords
                            .Where(record => record.DateRecorded.HasValue &&
                                             record.DateRecorded.Value.Date >= month.StartOfMonth &&
                                             record.DateRecorded.Value.Date <= month.EndOfMonth)
                            .ToList()
                    })
                    .Select(x => new CharLiverEnzymesModel
                    {
                        Type = MapMonthToVietnamese( x.Month.MonthLabel),
                        Alt = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Alt ?? 0), 2) : null,
                        Ast = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Ast ?? 0), 2) : null,
                        Alp = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Alp ?? 0), 2) : null,
                        Ggt = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Ggt ?? 0), 2) : null
                    })

                    .ToList();

                // Calculate yearlyRecords
                var yearlyRecords = liverEnzymeRecords
                    .GroupBy(l => l.DateRecorded.Value.Year)
                    .Select(g => new CharLiverEnzymesModel
                    {
                        Type = g.Key.ToString(),
                        Alt = (decimal?)Math.Round(g.Average(l => l.Alt ?? 0), 2),
                        Ast = (decimal?)Math.Round(g.Average(l => l.Ast ?? 0), 2),
                        Alp = (decimal?)Math.Round(g.Average(l => l.Alp ?? 0), 2),
                        Ggt = (decimal?)Math.Round(g.Average(l => l.Ggt ?? 0), 2)
                    })
                    .OrderBy(record => int.Parse(record.Type))
                    .ToList();

                // Calculate percentages for each tab
                var dailyPercentages = CalculatePercentages(dailyRecords);
                var weeklyPercentages = CalculatePercentages(weeklyRecords);
                var monthlyPercentages = CalculatePercentages(monthlyRecords);
                var yearlyPercentages = CalculatePercentages(yearlyRecords);

                // Prepare response
                var responseList = new List<GetLiverEnzymesDetail>
        {
            new GetLiverEnzymesDetail
            {
                Tabs = "Ngày",
                AltAverage = dailyRecords.Average(d => d.Alt ?? 0),
                AstAverage = dailyRecords.Average(d => d.Ast ?? 0),
                AlpAverage = dailyRecords.Average(d => d.Alp ?? 0),
                GgtAverage = dailyRecords.Average(d => d.Ggt ?? 0),
              
                HighestPercent = dailyPercentages.HighestPercent,
                LowestPercent = dailyPercentages.LowestPercent,
                NormalPercent = dailyPercentages.NormalPercent,
                ChartDatabase = dailyRecords
            },
            new GetLiverEnzymesDetail
            {
                Tabs = "Tuần",
                AltAverage = weeklyRecords.Average(w => w.Alt ?? 0),
                AstAverage = weeklyRecords.Average(w => w.Ast ?? 0),
                AlpAverage = weeklyRecords.Average(w => w.Alp ?? 0),
                GgtAverage = weeklyRecords.Average(w => w.Ggt ?? 0),
           
                HighestPercent = weeklyPercentages.HighestPercent,
                LowestPercent = weeklyPercentages.LowestPercent,
                NormalPercent = weeklyPercentages.NormalPercent,
                ChartDatabase = weeklyRecords
            },
            new GetLiverEnzymesDetail
            {
                Tabs = "Tháng",
                AltAverage = monthlyRecords.Average(m => m.Alt ?? 0),
                AstAverage = monthlyRecords.Average(m => m.Ast ?? 0),
                AlpAverage = monthlyRecords.Average(m => m.Alp ?? 0),
                GgtAverage = monthlyRecords.Average(m => m.Ggt ?? 0),
         
                HighestPercent = monthlyPercentages.HighestPercent,
                LowestPercent = monthlyPercentages.LowestPercent,
                NormalPercent = monthlyPercentages.NormalPercent,
                ChartDatabase = monthlyRecords
            },
            new GetLiverEnzymesDetail
            {
                Tabs = "Năm",
                AltAverage = yearlyRecords.Average(y => y.Alt ?? 0),
                AstAverage = yearlyRecords.Average(y => y.Ast ?? 0),
                AlpAverage = yearlyRecords.Average(y => y.Alp ?? 0),
                GgtAverage = yearlyRecords.Average(y => y.Ggt ?? 0),
              
                HighestPercent = yearlyPercentages.HighestPercent,
                LowestPercent = yearlyPercentages.LowestPercent,
                NormalPercent = yearlyPercentages.NormalPercent,
                ChartDatabase = yearlyRecords
            }
        };

                return new BusinessResult(Const.SUCCESS_READ, "Liver enzymes details retrieved successfully.", responseList);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        private (double HighestPercent, double LowestPercent, double NormalPercent) CalculatePercentages(List<CharLiverEnzymesModel> records)
        {
            int totalRecords = records.Count;
            if (totalRecords == 0)
                return (0, 0, 0);

            int highestCount = 0;
            int lowestCount = 0;
            int normalCount = 0;

            foreach (var record in records)
            {
                // Evaluate ALT
                var altEvaluation = GetAltEvaluation(record.Alt ?? 0);
                if (altEvaluation == "Cao") highestCount++;
                else if (altEvaluation == "Thấp") lowestCount++;
                else if (altEvaluation == "Bình thường") normalCount++;

                // Evaluate AST
                var astEvaluation = GetAstEvaluation(record.Ast ?? 0);
                if (astEvaluation == "Cao") highestCount++;
                else if (astEvaluation == "Thấp") lowestCount++;
                else if (astEvaluation == "Bình thường") normalCount++;

                // Evaluate ALP
                var alpEvaluation = GetAlpEvaluation(record.Alp ?? 0);
                if (alpEvaluation == "Cao") highestCount++;
                else if (alpEvaluation == "Thấp") lowestCount++;
                else if (alpEvaluation == "Bình thường") normalCount++;

                // Evaluate GGT
                var ggtEvaluation = GetGgtEvaluation(record.Ggt ?? 0);
                if (ggtEvaluation == "Cao") highestCount++;
                else if (ggtEvaluation == "Thấp") lowestCount++;
                else if (ggtEvaluation == "Bình thường") normalCount++;
            }

            // Total evaluations = totalRecords * 4 (since there are 4 indicators)
            int totalEvaluations = totalRecords * 4;

            // Calculate percentages
            double highestPercent = (double)highestCount / totalEvaluations * 100;
            double lowestPercent = (double)lowestCount / totalEvaluations * 100;
            double normalPercent = (double)normalCount / totalEvaluations * 100;

            // Ensure the sum is 100% (due to rounding errors)
            double sum = highestPercent + lowestPercent + normalPercent;
            if (sum != 100.0)
            {
                // Adjust the largest percentage to make the sum 100%
                if (highestPercent >= lowestPercent && highestPercent >= normalPercent)
                    highestPercent += 100.0 - sum;
                else if (lowestPercent >= highestPercent && lowestPercent >= normalPercent)
                    lowestPercent += 100.0 - sum;
                else
                    normalPercent += 100.0 - sum;
            }

            return (Math.Round(highestPercent,2), Math.Round(lowestPercent,2), Math.Round(normalPercent,2));
        }
        private string GetAltEvaluation(decimal averageAlt)
        {
            var baseHealth = _unitOfWork.HealthIndicatorBaseRepository
                   .FindByCondition(i => i.Type == "ALT")
                   .FirstOrDefault();
            if (averageAlt >= baseHealth.MinValue && averageAlt <= baseHealth.MaxValue)
            {
                return "Bình thường";
            }
            else if (averageAlt > baseHealth.MaxValue)
            {
                return "Cao";
            }
            else
            {
                return "Không xác định";
            }
        }

        private string GetAstEvaluation(decimal averageAst)
        {
            var baseHealth = _unitOfWork.HealthIndicatorBaseRepository
                   .FindByCondition(i => i.Type == "AST")
                   .FirstOrDefault();
            if (averageAst >= baseHealth.MinValue && averageAst <= baseHealth.MaxValue)
            {
                return "Bình thường";
            }
            else if (averageAst > baseHealth.MaxValue)
            {
                return "Cao";
            }
            else
            {
                return "Không xác định";
            }
        }

        private string GetAlpEvaluation(decimal averageAlp)
        {
            var baseHealth = _unitOfWork.HealthIndicatorBaseRepository
                   .FindByCondition(i => i.Type == "ALP")
                   .FirstOrDefault();
            if (averageAlp >= baseHealth.MinValue && averageAlp <= baseHealth.MaxValue)
            {
                return "Bình thường";
            }
            else if (averageAlp > baseHealth.MaxValue)
            {
                return "Cao";
            }
            else
            {
                return "Không xác định";
            }
        }

        private string GetGgtEvaluation(decimal averageGgt)
        {
            var baseHealth = _unitOfWork.HealthIndicatorBaseRepository
                   .FindByCondition(i => i.Type == "GGT")
                   .FirstOrDefault();
            if (averageGgt >= baseHealth.MinValue && averageGgt <= baseHealth.MaxValue)
            {
                return "Bình thường";
            }
            else if (averageGgt > baseHealth.MaxValue)
            {
                return "Cao";
            }
            else
            {
                return "Không xác định";
            }
        }
        public async Task<IBusinessResult> GetKidneyFunctionDetail(int accountId)
        {
            try
            {
                var elderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(accountId);

                if (elderly == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                var kidneyFunctionRecords = await _unitOfWork.KidneyFunctionRepository
                    .FindByConditionAsync(k => k.ElderlyId == elderly.Elderly.ElderlyId && k.Status == SD.GeneralStatus.ACTIVE);

                if (!kidneyFunctionRecords.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, "No kidney function records found for the elderly.");
                }

                var today = System.DateTime.UtcNow.AddHours(7);

                var last7Days = Enumerable.Range(0, 7)
                    .Select(offset => today.AddDays(-offset).Date)
                    .OrderBy(date => date)
                    .ToList();

                var last6Weeks = Enumerable.Range(0, 6)
                    .Select(offset => today.AddDays(-(offset * 7)))
                    .OrderBy(date => date)
                    .Select(date => new
                    {
                        StartOfWeek = date.AddDays(-(int)date.DayOfWeek + (int)System.DayOfWeek.Monday),
                        EndOfWeek = date.AddDays(-(int)date.DayOfWeek + (int)System.DayOfWeek.Monday + 6),
                        WeekLabel = $"Week {CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, System.DayOfWeek.Monday)}"
                    })
                    .ToList();

                var last4Months = Enumerable.Range(0, 4)
                    .Select(offset => today.AddMonths(-offset))
                    .OrderBy(date => date)
                    .Select(date => new
                    {
                        StartOfMonth = new System.DateTime(date.Year, date.Month, 1),
                        EndOfMonth = new System.DateTime(date.Year, date.Month, System.DateTime.DaysInMonth(date.Year, date.Month)),
                        MonthLabel = $"{CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(date.Month)}"
                    })
                    .ToList();

                // Calculate dailyRecords
                var dailyRecords = last7Days
                    .Select(date => new
                    {
                        Date = date,
                        Records = kidneyFunctionRecords
                            .Where(record => record.DateRecorded.HasValue && record.DateRecorded.Value.Date == date)
                            .ToList()
                    })
                    .Select(x => new CharKidneyFunctionModel
                    {
                        Type = MapDayOfWeekToVietnamese(x.Date.DayOfWeek),
                        Creatinine = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(k => k.Creatinine ?? 0), 2) : null,
                        Bun = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(k => k.Bun ?? 0), 2) : null,
                        EGfr = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(k => k.EGfr ?? 0), 2) : null
                    })
                    .ToList();

                // Calculate weeklyRecords
                var weeklyRecords = last6Weeks
                    .Select(week => new
                    {
                        Week = week,
                        Records = kidneyFunctionRecords
                            .Where(record => record.DateRecorded.HasValue &&
                                             record.DateRecorded.Value.Date >= week.StartOfWeek &&
                                             record.DateRecorded.Value.Date <= week.EndOfWeek)
                            .ToList()
                    })
                    .Select(x => new CharKidneyFunctionModel
                    {
                                                Type = MapWeekToVietnamese( x.Week.WeekLabel),
                        Creatinine = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(k => k.Creatinine ?? 0), 2) : null,
                        Bun = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(k => k.Bun ?? 0), 2) : null,
                        EGfr = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(k => k.EGfr ?? 0), 2) : null
                    })

                    .ToList();

                // Calculate monthlyRecords
                var monthlyRecords = last4Months
                    .Select(month => new
                    {
                        Month = month,
                        Records = kidneyFunctionRecords
                            .Where(record => record.DateRecorded.HasValue &&
                                             record.DateRecorded.Value.Date >= month.StartOfMonth &&
                                             record.DateRecorded.Value.Date <= month.EndOfMonth)
                            .ToList()
                    })
                    .Select(x => new CharKidneyFunctionModel
                    {
                        Type = MapMonthToVietnamese( x.Month.MonthLabel),
                        Creatinine = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(k => k.Creatinine ?? 0), 2) : null,
                        Bun = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(k => k.Bun ?? 0), 2) : null,
                        EGfr = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(k => k.EGfr ?? 0), 2) : null
                    })

                    .ToList();

                // Calculate yearlyRecords
                var yearlyRecords = kidneyFunctionRecords
                    .GroupBy(k => k.DateRecorded.Value.Year)
                    .Select(g => new CharKidneyFunctionModel
                    {
                        Type = g.Key.ToString(),
                        Creatinine = (decimal?)Math.Round(g.Average(k => k.Creatinine ?? 0), 2),
                        Bun = (decimal?)Math.Round(g.Average(k => k.Bun ?? 0), 2),
                        EGfr = (decimal?)Math.Round(g.Average(k => k.EGfr ?? 0), 2)
                    })
                    .OrderBy(record => int.Parse(record.Type))
                    .ToList();

                // Calculate percentages for each tab
                var dailyPercentages = CalculatePercentages(dailyRecords);
                var weeklyPercentages = CalculatePercentages(weeklyRecords);
                var monthlyPercentages = CalculatePercentages(monthlyRecords);
                var yearlyPercentages = CalculatePercentages(yearlyRecords);

                // Prepare response
                var responseList = new List<GetKidneyFunctionDetail>
        {
            new GetKidneyFunctionDetail
            {
                Tabs = "Ngày",
                CreatinineAverage = dailyRecords.Average(d => d.Creatinine ?? 0),
                BunAverage = dailyRecords.Average(d => d.Bun ?? 0),
                EGfrAverage = dailyRecords.Average(d => d.EGfr ?? 0),
                
                HighestPercent = dailyPercentages.HighestPercent,
                LowestPercent = dailyPercentages.LowestPercent,
                NormalPercent = dailyPercentages.NormalPercent,
                ChartDatabase = dailyRecords
            },
            new GetKidneyFunctionDetail
            {
                Tabs = "Tuần",
                CreatinineAverage = weeklyRecords.Average(w => w.Creatinine ?? 0),
                BunAverage = weeklyRecords.Average(w => w.Bun ?? 0),
                EGfrAverage = weeklyRecords.Average(w => w.EGfr ?? 0),

                HighestPercent = weeklyPercentages.HighestPercent,
                LowestPercent = weeklyPercentages.LowestPercent,
                NormalPercent = weeklyPercentages.NormalPercent,
                ChartDatabase = weeklyRecords
            },
            new GetKidneyFunctionDetail
            {
                Tabs = "Tháng",
                CreatinineAverage = monthlyRecords.Average(m => m.Creatinine ?? 0),
                BunAverage = monthlyRecords.Average(m => m.Bun ?? 0),
                EGfrAverage = monthlyRecords.Average(m => m.EGfr ?? 0),
           
                HighestPercent = monthlyPercentages.HighestPercent,
                LowestPercent = monthlyPercentages.LowestPercent,
                NormalPercent = monthlyPercentages.NormalPercent,
                ChartDatabase = monthlyRecords
            },
            new GetKidneyFunctionDetail
            {
                Tabs = "Năm",
                CreatinineAverage = yearlyRecords.Average(y => y.Creatinine ?? 0),
                BunAverage = yearlyRecords.Average(y => y.Bun ?? 0),
                EGfrAverage = yearlyRecords.Average(y => y.EGfr ?? 0),
              
                HighestPercent = yearlyPercentages.HighestPercent,
                LowestPercent = yearlyPercentages.LowestPercent,
                NormalPercent = yearlyPercentages.NormalPercent,
                ChartDatabase = yearlyRecords
            }
        };

                return new BusinessResult(Const.SUCCESS_READ, "Kidney function details retrieved successfully.", responseList);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        private (double HighestPercent, double LowestPercent, double NormalPercent) CalculatePercentages(List<CharKidneyFunctionModel> records)
        {
            int totalRecords = records.Count;
            if (totalRecords == 0)
                return (0, 0, 0);

            int highestCount = 0;
            int lowestCount = 0;
            int normalCount = 0;

            foreach (var record in records)
            {
                // Evaluate Creatinine
                var creatinineEvaluation = GetCreatinineEvaluation(record.Creatinine ?? 0);
                if (creatinineEvaluation == "Cao") highestCount++;
                else if (creatinineEvaluation == "Thấp") lowestCount++;
                else if (creatinineEvaluation == "Bình thường") normalCount++;

                // Evaluate Bun
                var bunEvaluation = GetBunEvaluation(record.Bun ?? 0);
                if (bunEvaluation == "Cao") highestCount++;
                else if (bunEvaluation == "Thấp") lowestCount++;
                else if (bunEvaluation == "Bình thường") normalCount++;

                // Evaluate EGfr
                var egfrEvaluation = GetEGfrEvaluation(record.EGfr ?? 0);
                if (egfrEvaluation == "Cao") highestCount++;
                else if (egfrEvaluation == "Thấp") lowestCount++;
                else if (egfrEvaluation == "Bình thường") normalCount++;
            }

            // Total evaluations = totalRecords * 3 (since there are 3 indicators)
            int totalEvaluations = totalRecords * 3;

            // Calculate percentages
            double highestPercent = (double)highestCount / totalEvaluations * 100;
            double lowestPercent = (double)lowestCount / totalEvaluations * 100;
            double normalPercent = (double)normalCount / totalEvaluations * 100;

            // Ensure the sum is 100% (due to rounding errors)
            double sum = highestPercent + lowestPercent + normalPercent;
            if (sum != 100.0)
            {
                // Adjust the largest percentage to make the sum 100%
                if (highestPercent >= lowestPercent && highestPercent >= normalPercent)
                    highestPercent += 100.0 - sum;
                else if (lowestPercent >= highestPercent && lowestPercent >= normalPercent)
                    lowestPercent += 100.0 - sum;
                else
                    normalPercent += 100.0 - sum;
            }

            return (Math.Round(highestPercent,2), Math.Round(lowestPercent,2), Math.Round(normalPercent,2));
        }
        private string GetCreatinineEvaluation(decimal averageCreatinine)
        {
            var baseHealth = _unitOfWork.HealthIndicatorBaseRepository
                   .FindByCondition(i => i.Type == "Creatinine")
                   .FirstOrDefault();
            if (averageCreatinine >= baseHealth.MinValue && averageCreatinine <= baseHealth.MaxValue)
            {
                return "Bình thường";
            }
            else if (averageCreatinine > baseHealth.MaxValue)
            {
                return "Cao";
            }
            else
            {
                return "Không xác định";
            }
        }

        private string GetBunEvaluation(decimal averageBun)
        {
            var baseHealth = _unitOfWork.HealthIndicatorBaseRepository
                   .FindByCondition(i => i.Type == "Bun")
                   .FirstOrDefault();
            if (averageBun >= baseHealth.MinValue && averageBun <= baseHealth.MaxValue)
            {
                return "Bình thường";
            }
            else if (averageBun > baseHealth.MaxValue)
            {
                return "Cao";
            }
            else
            {
                return "Không xác định";
            }
        }

        private string GetEGfrEvaluation(decimal averageEGfr)
        {
            var baseHealth = _unitOfWork.HealthIndicatorBaseRepository
                   .FindByCondition(i => i.Type == "EGfr")
                   .FirstOrDefault();
            if (averageEGfr >= baseHealth.MinValue && averageEGfr <= baseHealth.MaxValue)
            {
                return "Bình thường";
            }
            else if (averageEGfr > baseHealth.MaxValue)
            {
                return "Cao";
            }
            else
            {
                return "Không xác định";
            }
        }

        public async Task<IBusinessResult> EvaluateHealthIndicator(EvaluateHealthIndicatorRequest req)
        {
            try
            {
                string result ="Không xác định";

                if (req.Indicators == null || !req.Indicators.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, "INDICATORS MUST NOT BE EMPTY");
                }

                var healthIndicatorBases = await _unitOfWork.HealthIndicatorBaseRepository.GetAllAsync();

                
                foreach (var indicator in req.Indicators)
                {
                    var baseIndicator = _unitOfWork.HealthIndicatorBaseRepository.
                                                    FindByCondition(i => i.Type == indicator.Type && indicator.Time != null && i.Time == indicator.Time)
                                                    .FirstOrDefault();
                    if (indicator.Value > baseIndicator.MaxValue)
                    {
                        result = "Cao";
                    }
                    else if (indicator.Value < baseIndicator.MinValue)
                    {
                        result = "Thấp";
                    }
                    else
                    {
                        result = "Bình thường";
                    }
                }

                return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, ex.Message);
            }
        }

        public async Task<IBusinessResult> GetAllHealthIndicators(int accountId)
        {
            try
            {
                var elderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(accountId);

                if (elderly == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                var healthIndicators = _unitOfWork.HealthIndicatorBaseRepository
                    .FindByCondition(h => h.Status == SD.GeneralStatus.ACTIVE)
                    .ToList();

                if (!healthIndicators.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, "No health indicators found.");
                }

                // Function to calculate BMI
                double CalculateBMI(decimal weight, decimal height)
                {
                    if (height == 0)
                        return 0;

                    // Convert height from cm to meters
                    double heightInMeters = (double)height / 100;
                    return (double)weight / (heightInMeters * heightInMeters);
                }

                // Function to get the latest record and average for a specific type
                GetAllHealthIndicatorReponse GetIndicatorResponse<T>(
                    List<T> records,
                    Func<T, System.DateTime?> getDateRecorded,
                    Func<T, decimal?> getIndicatorValue,
                    string type,
                    List<HealthIndicatorBase> indicators,
                    bool calculateDifference = false, // Add a flag for weight and height
                    double? bmi = null) // Add BMI for evaluation
                    where T : class
                {
                    var latestRecord = records
                        .OrderByDescending(r => getDateRecorded(r))
                        .FirstOrDefault();

                    if (latestRecord == null)
                        return null;

                    var latestDate = getDateRecorded(latestRecord);
                    var latestIndicator = getIndicatorValue(latestRecord);

                    if (!latestDate.HasValue || !latestIndicator.HasValue)
                        return null;
                    var indicatorBase = new HealthIndicatorBase();
                    // Get the HealthIndicatorBase for the type
                    if (type == "KidneyFunction")
                    {
                        indicatorBase = indicators.FirstOrDefault(h => h.Type == "Creatinine");

                    }
                    else if (type == "LiverEnzyme")
                    {
                        indicatorBase = indicators.FirstOrDefault(h => h.Type == "ALT");

                    }
                    else if (type == "LipidProfile")
                    {
                        indicatorBase = indicators.FirstOrDefault(h => h.Type == "TotalCholesterol");

                    }else
                    {
                        indicatorBase = indicators.FirstOrDefault(h => h.Type == type);

                    }
                    if (indicatorBase == null)
                        return null;

                    // Determine evaluation
                    string evaluation;
                    if (bmi.HasValue && (type == "Weight" || type == "Height"))
                    {
                        // Use BMI for evaluation
                        evaluation = (decimal)bmi < indicatorBase.MinValue ? "Thấp" :
                                      (decimal)bmi > indicatorBase.MaxValue ? "Cao" :
                                      "Bình thường";
                    }
                    else
                    {
                        // Use the indicator value for evaluation
                        evaluation = latestIndicator < indicatorBase.MinValue ? "Thấp" :
                                      latestIndicator > indicatorBase.MaxValue ? "Cao" :
                                      "Bình thường";
                    }

                    // Calculate average for the last 30 days
                    var last30DaysRecords = records
                        .Where(r => getDateRecorded(r) >= System.DateTime.UtcNow.AddDays(-30))
                        .ToList();

                    var averageIndicator = Math.Round(last30DaysRecords.Any() ?
                        last30DaysRecords.Average(r => getIndicatorValue(r) ?? 0) :
                        0,2);

                    // For weight and height, calculate the difference with the previous indicator
                    string formattedAverageIndicator = averageIndicator.ToString();
                    if (calculateDifference)
                    {
                        var previousRecord = records
                            .OrderByDescending(r => getDateRecorded(r))
                            .Skip(1) // Skip the latest record to get the previous one
                            .FirstOrDefault();

                        if (previousRecord != null)
                        {
                            var previousIndicator = getIndicatorValue(previousRecord);
                            if (previousIndicator.HasValue)
                            {
                                var difference = latestIndicator.Value - previousIndicator.Value;
                                formattedAverageIndicator = difference >= 0 ? $"+{difference}" : $"{difference}";
                            }
                        }
                        else
                        {
                            // If it's the first indicator, set the difference to +0
                            formattedAverageIndicator = "+0";
                        }
                    }

                    return new GetAllHealthIndicatorReponse
                    {
                        Tabs = type,
                        Evaluation = evaluation,
                        DateTime = latestDate.Value.ToString("dd-MM HH:mm"), // Format DateTime as DD-MM HH-mm
                        Indicator = latestIndicator.Value.ToString(),
                        AverageIndicator = formattedAverageIndicator
                    };
                }

                // Fetch records for each type
                var heightRecords = _unitOfWork.HeightRepository
                    .FindByCondition(h => h.ElderlyId == elderly.Elderly.ElderlyId && h.Status == SD.GeneralStatus.ACTIVE)
                    .ToList();

                var weightRecords = _unitOfWork.WeightRepository
                    .FindByCondition(w => w.ElderlyId == elderly.Elderly.ElderlyId && w.Status == SD.GeneralStatus.ACTIVE)
                    .ToList();

                var heartRateRecords = _unitOfWork.HeartRateRepository
                    .FindByCondition(hr => hr.ElderlyId == elderly.Elderly.ElderlyId && hr.Status == SD.GeneralStatus.ACTIVE)
                    .ToList();

                var bloodPressureRecords = _unitOfWork.BloodPressureRepository
                    .FindByCondition(bp => bp.ElderlyId == elderly.Elderly.ElderlyId && bp.Status == SD.GeneralStatus.ACTIVE)
                    .ToList();

                var lipidProfileRecords = _unitOfWork.LipidProfileRepository
                    .FindByCondition(lp => lp.ElderlyId == elderly.Elderly.ElderlyId && lp.Status == SD.GeneralStatus.ACTIVE)
                    .ToList();

                var liverEnzymeRecords = _unitOfWork.LiverEnzymeRepository
                    .FindByCondition(le => le.ElderlyId == elderly.Elderly.ElderlyId && le.Status == SD.GeneralStatus.ACTIVE)
                    .ToList();

                var bloodGlucoseRecords = _unitOfWork.BloodGlucoseRepository
                    .FindByCondition(bg => bg.ElderlyId == elderly.Elderly.ElderlyId && bg.Status == SD.GeneralStatus.ACTIVE)
                    .ToList();

                var kidneyFunctionRecords = _unitOfWork.KidneyFunctionRepository
                    .FindByCondition(kf => kf.ElderlyId == elderly.Elderly.ElderlyId && kf.Status == SD.GeneralStatus.ACTIVE)
                    .ToList();

                // Get the newest Height and Weight
                var newestHeight = heightRecords
                    .OrderByDescending(h => h.DateRecorded)
                    .FirstOrDefault();

                var newestWeight = weightRecords
                    .OrderByDescending(w => w.DateRecorded)
                    .FirstOrDefault();

                // Calculate BMI if both Height and Weight are available
                double? bmi = null;
                if (newestHeight != null && newestWeight != null && newestHeight.Height1.HasValue && newestWeight.Weight1.HasValue)
                {
                    bmi = CalculateBMI(newestWeight.Weight1.Value, newestHeight.Height1.Value);
                }

                // Create responses for each type
                var heightResponse = GetIndicatorResponse(
                    heightRecords,
                    h => h.DateRecorded,
                    h => h.Height1,
                    "Height",
                    healthIndicators,
                    calculateDifference: true,
                    bmi: bmi);

                var weightResponse = GetIndicatorResponse(
                    weightRecords,
                    w => w.DateRecorded,
                    w => w.Weight1,
                    "Weight",
                    healthIndicators,
                    calculateDifference: true,
                    bmi: bmi);

                var heartRateResponse = GetIndicatorResponse(
                    heartRateRecords,
                    hr => hr.DateRecorded,
                    hr => hr.HeartRate1,
                    "HeartRate",
                    healthIndicators);

                var bloodPressureResponse = GetBloodPressureResponse(bloodPressureRecords, healthIndicators);

                // Lipid Profile: Only use TotalCholesterol
                var lipidProfileResponse = GetIndicatorResponse(
                    lipidProfileRecords,
                    lp => lp.DateRecorded,
                    lp => lp.TotalCholesterol, // Use TotalCholesterol as the indicator
                    "LipidProfile", // Renamed to "LipidProfile"
                    healthIndicators,
                    calculateDifference: true); // Enable difference calculation

                // Kidney Function: Only use eGFR
                var kidneyFunctionResponse = GetIndicatorResponse(
                    kidneyFunctionRecords,
                    kf => kf.DateRecorded,
                    kf => kf.EGfr, 
                    "KidneyFunction",
                    healthIndicators,
                    calculateDifference: true); // Enable difference calculation

                var liverEnzymeResponse = GetIndicatorResponse(
                    liverEnzymeRecords,
                    lz =>lz.DateRecorded,
                    lz=>lz.Alt,
                    "LiverEnzyme",
                    healthIndicators,
                    calculateDifference : true
                    
                    );


                /*var liverEnzymeResponse = liverEnzymeRecords
                    .OrderByDescending(le => le.DateRecorded)
                    .Select(le => new GetAllHealthIndicatorReponse
                    {
                        Tabs = "LiverEnzyme",
                        Evaluation = "N/A",
                        DateTime = le.DateRecorded?.ToString("dd-MM HH:mm") ?? "N/A",
                        Indicator = "N/A",
                        AverageIndicator = "N/A"
                    })
                    .FirstOrDefault();*/



                // Blood Glucose: Evaluate based on Time field
                var bloodGlucoseResponse = bloodGlucoseRecords
                    .OrderByDescending(bg => bg.DateRecorded)
                    .Select(bg => new GetAllHealthIndicatorReponse
                    {
                        Tabs = "BloodGlucose",
                        Evaluation = GetBloodGlucoseEvaluation(bg.BloodGlucose1, bg.Time, healthIndicators),
                        DateTime = bg.DateRecorded?.ToString("dd-MM HH:mm") ?? "N/A",
                        Indicator = bg.BloodGlucose1?.ToString() ?? "N/A",
                        AverageIndicator = GetDifferenceIndicator(bloodGlucoseRecords, bg => bg.BloodGlucose1) // Calculate difference for Blood Glucose
                    })
                    .FirstOrDefault();

                // Combine responses
                var responseList = new List<GetAllHealthIndicatorReponse>();

                if (heightResponse != null)
                    responseList.Add(heightResponse);

                if (weightResponse != null)
                    responseList.Add(weightResponse);

                if (heartRateResponse != null)
                    responseList.Add(heartRateResponse);

                if (bloodPressureResponse != null)
                    responseList.Add(bloodPressureResponse);

                if (lipidProfileResponse != null)
                    responseList.Add(lipidProfileResponse);

                if (liverEnzymeResponse != null)
                    responseList.Add(liverEnzymeResponse);

                if (bloodGlucoseResponse != null)
                    responseList.Add(bloodGlucoseResponse);

                if (kidneyFunctionResponse != null)
                    responseList.Add(kidneyFunctionResponse);

                return new BusinessResult(Const.SUCCESS_READ, "Health indicators retrieved successfully.", responseList);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        // Function to calculate the difference indicator
        private string GetDifferenceIndicator<T>(List<T> records, Func<T, decimal?> getIndicatorValue)
        {
            if (!records.Any())
                return "+0"; // If no records, return +0

            var latestRecord = records
                .OrderByDescending(r => getIndicatorValue(r))
                .FirstOrDefault();

            if (latestRecord == null)
                return "+0";

            var previousRecord = records
                .OrderByDescending(r => getIndicatorValue(r))
                .Skip(1) // Skip the latest record to get the previous one
                .FirstOrDefault();

            if (previousRecord == null)
                return "+0"; // If it's the first record, return +0

            var latestIndicator = getIndicatorValue(latestRecord);
            var previousIndicator = getIndicatorValue(previousRecord);

            if (!latestIndicator.HasValue || !previousIndicator.HasValue)
                return "+0";

            var difference = latestIndicator.Value - previousIndicator.Value;
            return difference >= 0 ? $"+{difference}" : $"{difference}";
        }
        private GetAllHealthIndicatorReponse GetBloodPressureResponse(List<BloodPressure> records, List<HealthIndicatorBase> indicators)
        {
            var latestRecord = records
                .OrderByDescending(r => r.DateRecorded)
                .FirstOrDefault();

            if (latestRecord == null)
                return null;

            var latestDate = latestRecord.DateRecorded;
            var systolic = latestRecord.Systolic;
            var diastolic = latestRecord.Diastolic;

            if (!latestDate.HasValue || !systolic.HasValue || !diastolic.HasValue)
                return null;

            // Get the HealthIndicatorBase for Systolic and Diastolic
            var systolicIndicator = indicators.FirstOrDefault(h => h.Type == "Systolic");
            var diastolicIndicator = indicators.FirstOrDefault(h => h.Type == "Diastolic");

            if (systolicIndicator == null || diastolicIndicator == null)
                return null;

            // Determine evaluation for Blood Pressure
            string evaluation = (systolic <= systolicIndicator.MaxValue && diastolic <= diastolicIndicator.MaxValue)
                ? "Bình thường"
                : "Cao ";

            // Calculate average for the last 30 days
            var last30DaysRecords = records
                .Where(r => r.DateRecorded >= System.DateTime.UtcNow.AddDays(-30))
                .ToList();

            var averageSystolic = last30DaysRecords.Any() ?
                last30DaysRecords.Average(r => r.Systolic ?? 0) :
                0;

            var averageDiastolic = last30DaysRecords.Any() ?
                last30DaysRecords.Average(r => r.Diastolic ?? 0) :
                0;

            // Format Indicator and AverageIndicator as "Systolic/Diastolic"
            string indicator = $"{systolic}/{diastolic}";
            string averageIndicator = $"{Math.Round(averageSystolic, 0)}/{Math.Round(averageDiastolic, 0)}";

            return new GetAllHealthIndicatorReponse
            {
                Tabs = "BloodPressure",
                Evaluation = evaluation,
                DateTime = latestDate.Value.ToString("dd-MM HH:mm"), // Format DateTime as DD-MM HH-mm
                Indicator = indicator,
                AverageIndicator = averageIndicator
            };
        }
        private string GetBloodGlucoseEvaluation(decimal? bloodGlucose, string time, List<HealthIndicatorBase> indicators)
        {
            if (!bloodGlucose.HasValue || string.IsNullOrEmpty(time))
                return "N/A";

            // Find the HealthIndicatorBase for the specific time
            var indicatorBase = indicators.FirstOrDefault(h => h.Type == "BloodGlucose" && h.Time == time);
            if (indicatorBase == null)
                return "N/A";

            // Determine evaluation
            return bloodGlucose < indicatorBase.MinValue ? "Thấp" :
                   bloodGlucose > indicatorBase.MaxValue ? "Cao" :
                   "Bình thường";
        }

        public async Task<IBusinessResult> EvaluateBMI(decimal? height, decimal? weight, int accountId)
        {

            try
            {

                double bmi =0.0;

                var elderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(accountId);
                if (height != null)
                {
                    var getWeight = _unitOfWork.WeightRepository.FindByCondition(h => h.ElderlyId == elderly.Elderly.ElderlyId)
                                                 .OrderByDescending(x => x.DateRecorded)
                                                 .FirstOrDefault();
                    bmi = CalculateBMI((decimal)getWeight.Weight1, (decimal)height/100);

                }
                else if (weight != null)
                {
                    var getHeight = _unitOfWork.HeightRepository.FindByCondition(h => h.ElderlyId == elderly.Elderly.ElderlyId)
                                              .OrderByDescending(x => x.DateRecorded)
                                              .FirstOrDefault();
                    bmi = CalculateBMI((decimal)weight, (decimal)getHeight.Height1/100);
                }
                var baseIndicator = _unitOfWork.HealthIndicatorBaseRepository.
                                                    FindByCondition(i => i.Type == "Weight")
                                                    .FirstOrDefault();
                string result;
                if ((decimal)bmi > baseIndicator.MaxValue)
                {
                    result = "Thừa cân";
                }
                else if ((decimal)bmi < baseIndicator.MinValue)
                {
                    result = "Thiếu cân";
                }
                else
                {
                    result = "Bình thường";
                }

                var final = new 
                {
                    Evaluation = result,
                    BMI = bmi
                };

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, final);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IBusinessResult> EvaluateHeartRate(int? heartRate)
        {

            try
            {
               
              
                var baseIndicator = _unitOfWork.HealthIndicatorBaseRepository.
                                                    FindByCondition(i => i.Type == "HeartRate")
                                                    .FirstOrDefault();
                string result;
                if (heartRate > baseIndicator.MaxValue)
                {
                    result = "Nhịp tim nhanh";
                }
                else if (heartRate < baseIndicator.MinValue)
                {
                    result = "Nhịp tim chậm";
                }
                else
                {
                    result = "Nhịp tim bình thường";
                }
                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IBusinessResult> EvaluateBloodPressure(int systolic, int diastolic)
        {

            try
            {

                var baseSystolic = _unitOfWork.HealthIndicatorBaseRepository.
                                                    FindByCondition(i => i.Type == "Systolic")
                                                    .FirstOrDefault();
                var baseDiastolic = _unitOfWork.HealthIndicatorBaseRepository.
                                                    FindByCondition(i => i.Type == "Diastolic")
                                                    .FirstOrDefault();
                string result;
                if (systolic<baseSystolic.MaxValue && diastolic<baseDiastolic.MaxValue)
                {
                    result = "Huyết áp bình thường";
                }
                else if (systolic < baseSystolic.MinValue && diastolic < baseDiastolic.MinValue)
                {
                    result = "Huyết áp thấp";
                }
                else
                {
                    result = "Huyết áp cao";
                }
                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<IBusinessResult> EvaluateBloodGlusose(decimal bloodGlucose, string time)
        {

            try
            {
                var baseBloodGlucose = _unitOfWork.HealthIndicatorBaseRepository.
                                                    FindByCondition(i => i.Type == "BloodGlucose" && i.Time == time)
                                                    .FirstOrDefault();

                string result;
                if (bloodGlucose < baseBloodGlucose.MaxValue && bloodGlucose > baseBloodGlucose.MinValue)
                {
                    result = "Bình thường";
                }
                else if (bloodGlucose > baseBloodGlucose.MaxValue)
                {
                    result = "Cao";
                }
                else
                {
                    result = "Thấp";
                }
                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IBusinessResult> EvaluateKidneyFunction(decimal creatinine, decimal BUN, decimal eGFR)
        {
            try
            {
                // Fetch the base values for creatinine, BUN, and eGFR from the repository
                var baseCreatinine = _unitOfWork.HealthIndicatorBaseRepository
                    .FindByCondition(i => i.Type == "Creatinine")
                    .FirstOrDefault();

                var baseBUN = _unitOfWork.HealthIndicatorBaseRepository
                    .FindByCondition(i => i.Type == "BUN")
                    .FirstOrDefault();

                var baseEGFR = _unitOfWork.HealthIndicatorBaseRepository
                    .FindByCondition(i => i.Type == "eGFR")
                    .FirstOrDefault();

                // Evaluate creatinine levels
                string creatinineResult;
                if (creatinine < baseCreatinine.MinValue)
                {
                    creatinineResult = "Creatinine thấp";
                }
                else if (creatinine > baseCreatinine.MaxValue)
                {
                    creatinineResult = "Creatinine cao";
                }
                else
                {
                    creatinineResult = "Creatinine bình thường";
                }

                // Evaluate BUN levels
                string BUNResult;
                if (BUN < baseBUN.MinValue)
                {
                    BUNResult = "BUN thấp ";
                }
                else if (BUN > baseBUN.MaxValue)
                {
                    BUNResult = "BUN cao ";
                }
                else
                {
                    BUNResult = "BUN level bình thường";
                }

                // Evaluate eGFR levels
                string eGFRResult;
                if (eGFR < baseEGFR.MinValue)
                {
                    eGFRResult = "eGFR thấp ";
                }
                else if (eGFR > baseEGFR.MaxValue)
                {
                    eGFRResult = "eGFR cao ";
                }
                else
                {
                    eGFRResult = "eGFR bình thường";
                }

                // Combine the results into a single message
                string combinedResult = $" {creatinineResult} - {BUNResult} - {eGFRResult}";

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, combinedResult);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IBusinessResult> EvaluateLipidProfile(decimal? totalCholesterol, decimal? ldlCholesterol, decimal? hdlCholesterol, decimal? triglycerides)
        {
            try
            {
                // Fetch the base values for lipid profile indicators from the repository
                var baseTotalCholesterol = _unitOfWork.HealthIndicatorBaseRepository
                    .FindByCondition(i => i.Type == "TotalCholesterol")
                    .FirstOrDefault();

                var baseLDLCholesterol = _unitOfWork.HealthIndicatorBaseRepository
                    .FindByCondition(i => i.Type == "LDLCholesterol")
                    .FirstOrDefault();

                var baseHDLCholesterol = _unitOfWork.HealthIndicatorBaseRepository
                    .FindByCondition(i => i.Type == "HDLCholesterol")
                    .FirstOrDefault();

                var baseTriglycerides = _unitOfWork.HealthIndicatorBaseRepository
                    .FindByCondition(i => i.Type == "Triglycerides")
                    .FirstOrDefault();

                // Evaluate Total Cholesterol
                string totalCholesterolResult;
                if (totalCholesterol < baseTotalCholesterol.MinValue)
                {
                    totalCholesterolResult = "Toàn phần Cholesterol thấp ";
                }
                else if (totalCholesterol > baseTotalCholesterol.MaxValue)
                {
                    totalCholesterolResult = "Toàn phần Cholesterol cao ";
                }
                else
                {
                    totalCholesterolResult = "Toàn phần Cholesterol bình thường";
                }

                // Evaluate LDL Cholesterol
                string ldlCholesterolResult;
                if (ldlCholesterol < baseLDLCholesterol.MinValue)
                {
                    ldlCholesterolResult = "LDL Cholesterol thấp ";
                }
                else if (ldlCholesterol > baseLDLCholesterol.MaxValue)
                {
                    ldlCholesterolResult = "LDL Cholesterol cao ";
                }
                else
                {
                    ldlCholesterolResult = "LDL Cholesterol bình thường";
                }

                // Evaluate HDL Cholesterol
                string hdlCholesterolResult;
                if (hdlCholesterol < baseHDLCholesterol.MinValue)
                {
                    hdlCholesterolResult = "HDL Cholesterol thấp ";
                }
                else if (hdlCholesterol > baseHDLCholesterol.MaxValue)
                {
                    hdlCholesterolResult = "HDL Cholesterol cao ";
                }
                else
                {
                    hdlCholesterolResult = "HDL Cholesterol bình thường";
                }

                // Evaluate Triglycerides
                string triglyceridesResult;
                if (triglycerides < baseTriglycerides.MinValue)
                {
                    triglyceridesResult = "Triglycerides thấp ";
                }
                else if (triglycerides > baseTriglycerides.MaxValue)
                {
                    triglyceridesResult = "Triglycerides cao ";
                }
                else
                {
                    triglyceridesResult = "Triglycerides bình thường";
                }

                // Combine the results into a single message
                string combinedResult = $"{totalCholesterolResult} - {ldlCholesterolResult} - {hdlCholesterolResult} - Triglycerides: {triglyceridesResult}";

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, combinedResult);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<IBusinessResult> EvaluateLiverEnzymes(decimal? alt, decimal? ast, decimal? alp, decimal? ggt)
        {
            try
            {
                var baseALT = _unitOfWork.HealthIndicatorBaseRepository
                    .FindByCondition(i => i.Type == "ALT")
                    .FirstOrDefault();

                var baseAST = _unitOfWork.HealthIndicatorBaseRepository
                    .FindByCondition(i => i.Type == "AST")
                    .FirstOrDefault();

                var baseALP = _unitOfWork.HealthIndicatorBaseRepository
                    .FindByCondition(i => i.Type == "ALP")
                    .FirstOrDefault();

                var baseGGT = _unitOfWork.HealthIndicatorBaseRepository
                    .FindByCondition(i => i.Type == "GGT")
                    .FirstOrDefault();

                // Evaluate ALT (Alanine Aminotransferase)
                string altResult;
                if (alt < baseALT.MinValue)
                {
                    altResult = "ALT thấp ";
                }
                else if (alt > baseALT.MaxValue)
                {
                    altResult = "ALT cao ";
                }
                else
                {
                    altResult = "ALT bình thường";
                }

                // Evaluate AST (Aspartate Aminotransferase)
                string astResult;
                if (ast < baseAST.MinValue)
                {
                    astResult = "AST thấp ";
                }
                else if (ast > baseAST.MaxValue)
                {
                    astResult = "AST cao ";
                }
                else
                {
                    astResult = "AST bình thường";
                }

                // Evaluate ALP (Alkaline Phosphatase)
                string alpResult;
                if (alp < baseALP.MinValue)
                {
                    alpResult = "ALP thấp ";
                }
                else if (alp > baseALP.MaxValue)
                {
                    alpResult = "ALP cao ";
                }
                else
                {
                    alpResult = "ALP bình thường";
                }

                // Evaluate GGT (Gamma-Glutamyl Transferase)
                string ggtResult;
                if (ggt < baseGGT.MinValue)
                {
                    ggtResult = "GGT thấp ";
                }
                else if (ggt > baseGGT.MaxValue)
                {
                    ggtResult = "GGT cao ";
                }
                else
                {
                    ggtResult = "GGT bình thường";
                }

                // Combine the results into a single message
                string combinedResult = $"{altResult} - {astResult} - {alpResult} - {ggtResult}";

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, combinedResult);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        private string GetBmiEvaluation(decimal weight, decimal heightInMeters)
        {
            if (heightInMeters <= 0)
            {
                return "N/A"; // Height must be greater than 0
            }

            // Calculate BMI
            double bmi = CalculateBMI(weight, heightInMeters);

            // Evaluate BMI based on standard categories
            if (bmi < 18.5)
            {
                return "Thiếu cân";
            }
            else if (bmi >= 18.5 && bmi < 24.9)
            {
                return "Bình thường";
            }
            else if (bmi >= 25 && bmi < 29.9)
            {
                return "Thừa cân";
            }
            else if (bmi >= 30)
            {
                return "Béo phì";
            }
            else
            {
                return "Không xác định";
            }
        }

        public async Task<IBusinessResult> GetLogBookResponses(int accountId)
        {

            var elderlyId = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(accountId);
            var responses = new List<LogBookReponse>();

            // Fetch all relevant data for the given elderlyId
            var bloodGlucoseRecords = _unitOfWork.BloodGlucoseRepository
                .FindByCondition(bg => bg.ElderlyId == elderlyId.Elderly.ElderlyId)
                .ToList();

            var bloodPressureRecords = _unitOfWork.BloodPressureRepository
                .FindByCondition(bp => bp.ElderlyId == elderlyId.Elderly.ElderlyId)
                .ToList();

            var heartRateRecords = _unitOfWork.HeartRateRepository
                .FindByCondition(hr => hr.ElderlyId == elderlyId.Elderly.ElderlyId)
                .ToList();

            var heightRecords = _unitOfWork.HeightRepository
                .FindByCondition(h => h.ElderlyId == elderlyId.Elderly.ElderlyId)
                .ToList();

            var height = heightRecords.Select(x=>x.HeightId).LastOrDefault();

            var weightRecords = _unitOfWork.WeightRepository
                .FindByCondition(w => w.ElderlyId == elderlyId.Elderly.ElderlyId)
                .ToList();

            var weight = weightRecords.Select(x => x.Weight1).LastOrDefault();

            var kidneyFunctionRecords = _unitOfWork.KidneyFunctionRepository
                .FindByCondition(kf => kf.ElderlyId == elderlyId.Elderly.ElderlyId)
                .ToList();

            var lipidProfileRecords = _unitOfWork.LipidProfileRepository
                .FindByCondition(lp => lp.ElderlyId == elderlyId.Elderly.ElderlyId)
                .ToList();

            var liverEnzymeRecords = _unitOfWork.LiverEnzymeRepository
                .FindByCondition(le => le.ElderlyId == elderlyId.Elderly.ElderlyId)
                .ToList();

            // Evaluate and add responses for each tab
            responses.AddRange(GetBloodGlucoseResponse(bloodGlucoseRecords));
            responses.AddRange(GetBloodPressureResponse(bloodPressureRecords));
            responses.AddRange(GetHeartRateResponse(heartRateRecords));
            responses.AddRange(GetHeightResponse( heightRecords,(int)weight));
            responses.AddRange(GetWeightResponse(weightRecords, height));
            responses.AddRange(GetKidneyFunctionResponse(kidneyFunctionRecords));
            responses.AddRange(GetLipidProfileResponse(lipidProfileRecords));
            responses.AddRange(GetLiverEnzymeResponse(liverEnzymeRecords));

            return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, responses.OrderByDescending(x=>x.DateRecorded));
        }

        private List<LogBookReponse> GetBloodGlucoseResponse(List<BloodGlucose> records)
        {
            var responses = new List<LogBookReponse>();

            foreach (var record in records)
            {
                var response = new LogBookReponse
                {
                    Tabs = "BloodGlucose",
                    Id = record.BloodGlucoseId,
                    DataType = record.BloodGlucoseSource,
                    DateTime = record.DateRecorded?.ToString("dd'-th'MM HH:mm"),
                    TimeRecorded = record.DateRecorded?.ToString("HH:mm"),
                    DateRecorded = record.DateRecorded?.ToString("dd-MM-yyyy"), 
                    Indicator = $"{record.BloodGlucose1}/{record.Time}",
                    Evaluation =(string) EvaluateBloodGlusose((int)record.BloodGlucose1,record.Time).Result.Data
                };
                responses.Add(response);
            }

            if (!responses.Any())
            {
                responses.Add(new LogBookReponse
                {
                    Tabs = "BloodGlucose",
                    DateRecorded = System.DateTime.Now.ToString(),
                    Indicator = "N/A",
                    Evaluation = "N/A"
                });
            }

            return responses;
        }

        private List<LogBookReponse> GetBloodPressureResponse(List<BloodPressure> records)
        {
            var responses = new List<LogBookReponse>();

            foreach (var record in records)
            {
                var response = new LogBookReponse
                {
                    Tabs = "BloodPressure",
                    Id = record.BloodPressureId,
                    DataType = record.SystolicSource,
                    DateTime = record.DateRecorded?.ToString("dd'-th'MM HH:mm"),
                    TimeRecorded = record.DateRecorded?.ToString("HH:mm"),
                    DateRecorded = record.DateRecorded?.ToString("dd-MM-yyyy"),
                    Indicator = $"{record.Systolic}/{record.Diastolic}",
                    Evaluation = GetBloodPressureEvaluation((double)record.Systolic, (double)record.Diastolic)
                };
                responses.Add(response);
            }

            if (!responses.Any())
            {
                responses.Add(new LogBookReponse
                {
                    Tabs = "BloodPressure",
                    DateRecorded = System.DateTime.Now.ToString(),
                    Indicator = "N/A",
                    Evaluation = "N/A"
                });
            }

            return responses;
        }

        private List<LogBookReponse> GetHeartRateResponse(List<HeartRate> records)
        {
            var responses = new List<LogBookReponse>();

            foreach (var record in records)
            {
                var response = new LogBookReponse
                {
                    Tabs = "HeartRate",
                    Id = record.HeartRateId,
                    DataType = record.HeartRateSource,
                    DateTime = record.DateRecorded?.ToString("dd'-th'MM HH:mm"),
                    TimeRecorded = record.DateRecorded?.ToString("HH:mm"),
                    DateRecorded = record.DateRecorded?.ToString("dd-MM-yyyy"),
                    Indicator = $"{record.HeartRate1}",
                    Evaluation = GetHeartRateEvaluation((double)record.HeartRate1)
                };
                responses.Add(response);
            }

            if (!responses.Any())
            {
                responses.Add(new LogBookReponse
                {
                    Tabs = "HeartRate",
                    DateRecorded = System.DateTime.Now.ToString(),
                    Indicator = "N/A",
                    Evaluation = "N/A"
                });
            }

            return responses;
        }

        private List<LogBookReponse> GetHeightResponse(List<Height> records, int weight)
        {
            var responses = new List<LogBookReponse>();
            foreach (var record in records)
            {
                
                var response = new LogBookReponse
                {
                    Tabs = "Height",
                    Id = record.HeightId,
                    DataType = record.HeightSource,
                    DateTime = record.DateRecorded?.ToString("dd'-th'MM HH:mm"),
                    TimeRecorded = record.DateRecorded?.ToString("HH:mm"),
                    DateRecorded = record.DateRecorded?.ToString("dd-MM-yyyy"),
                    Indicator = $"{record.Height1}",
                    Evaluation = GetBmiEvaluation(weight,(decimal)record.Height1) 
                };
                responses.Add(response);
            }

            if (!responses.Any())
            {
                responses.Add(new LogBookReponse
                {
                    Tabs = "Height",
                    DateRecorded = System.DateTime.Now.ToString(),
                    Indicator = "N/A",
                    Evaluation = "N/A"
                });
            }

            return responses;
        }

        private List<LogBookReponse> GetWeightResponse(List<Weight> records, int height)
        {
            var responses = new List<LogBookReponse>();

            foreach (var record in records)
            {
                var response = new LogBookReponse
                {
                    Tabs = "Weight",
                    Id = record.WeightId,
                    DataType = record.WeightSource,
                    DateTime = record.DateRecorded?.ToString("dd'-th'MM HH:mm"),
                    TimeRecorded = record.DateRecorded?.ToString("HH:mm"),
                    DateRecorded = record.DateRecorded?.ToString("dd-MM-yyyy"),
                    Indicator = $"{record.Weight1}",
                    Evaluation = GetBmiEvaluation((decimal)record.Weight1, height)
                };
                responses.Add(response);
            }

            if (!responses.Any())
            {
                responses.Add(new LogBookReponse
                {
                    Tabs = "Weight",
                    DateRecorded = System.DateTime.Now.ToString(),
                    Indicator = "N/A",
                    Evaluation = "N/A"
                });
            }

            return responses;
        }

        private List<LogBookReponse> GetKidneyFunctionResponse(List<KidneyFunction> records)
        {
            var responses = new List<LogBookReponse>();

            foreach (var record in records)
            {
                var healthIndicators = _unitOfWork.HealthIndicatorBaseRepository
    .FindByCondition(h => h.Status == SD.GeneralStatus.ACTIVE && h.Type == "Creatinine")
    .FirstOrDefault();
                var evaluation = record.EGfr < healthIndicators.MinValue ? "Thấp" :
                              record.EGfr > healthIndicators.MaxValue ? "Cao" :
                              "Bình thường";
                var response = new LogBookReponse
                {
                    Tabs = "KidneyFunction",
                    Id = record.KidneyFunctionId,
                    DataType = record.KidneyFunctionSource,
                    DateTime = record.DateRecorded?.ToString("dd'-th'MM HH:mm"),
                    TimeRecorded = record.DateRecorded?.ToString("HH:mm"),
                    DateRecorded = record.DateRecorded?.ToString("dd-MM-yyyy"),
                    Indicator = $"{record.Creatinine}/{record.Bun}/{record.EGfr}",
                    Evaluation = evaluation
                };
                responses.Add(response);
            }

            if (!responses.Any())
            {
                responses.Add(new LogBookReponse
                {
                    Tabs = "KidneyFunction",
                    DateRecorded = System.DateTime.Now.ToString(),
                    Indicator = "N/A",
                    Evaluation = "N/A"
                });
            }

            return responses;
        }

        private List<LogBookReponse> GetLipidProfileResponse(List<LipidProfile> records)
        {
            var responses = new List<LogBookReponse>();

            foreach (var record in records)
            {
                var healthIndicators = _unitOfWork.HealthIndicatorBaseRepository
    .FindByCondition(h => h.Status == SD.GeneralStatus.ACTIVE && h.Type == "TotalCholesterol")
    .FirstOrDefault();
                var evaluation = record.TotalCholesterol < healthIndicators.MinValue ? "Thấp" :
                              record.TotalCholesterol > healthIndicators.MaxValue ? "Cao" :
                              "Bình thường";
                var response = new LogBookReponse
                {
                    Tabs = "LipidProfile",
                    Id = record.LipidProfileId,
                    DataType = record.LipidProfileSource,
                    DateTime = record.DateRecorded?.ToString("dd'-th'MM HH:mm"),
                    TimeRecorded = record.DateRecorded?.ToString("HH:mm"),
                    DateRecorded = record.DateRecorded?.ToString("dd-MM-yyyy"),
                    Indicator = $"{record.TotalCholesterol}/{record.Ldlcholesterol}/{record.Hdlcholesterol}/{record.Triglycerides}",
                    Evaluation = evaluation
                };
                responses.Add(response);
            }

            if (!responses.Any())
            {
                responses.Add(new LogBookReponse
                {
                    Tabs = "LipidProfile",
                    DateRecorded = System.DateTime.Now.ToString(),
                    Indicator = "N/A",
                    Evaluation = "N/A"
                });
            }

            return responses;
        }

        private List<LogBookReponse> GetLiverEnzymeResponse(List<LiverEnzyme> records)
        {
            var responses = new List<LogBookReponse>();

            foreach (var record in records)
            {
                var healthIndicators = _unitOfWork.HealthIndicatorBaseRepository
.FindByCondition(h => h.Status == SD.GeneralStatus.ACTIVE && h.Type == "ALT")
.FirstOrDefault();
                var evaluation = record.Alt < healthIndicators.MinValue ? "Thấp" :
                              record.Alt > healthIndicators.MaxValue ? "Cao" :
                              "Bình thường";
                var response = new LogBookReponse
                {
                    Tabs = "LiverEnzyme",
                    Id = record.LiverEnzymesId,
                    DataType = record.LiverEnzymesSource,
                    DateTime = record.DateRecorded?.ToString("dd'-th'MM HH:mm"),
                    TimeRecorded = record.DateRecorded?.ToString("HH:mm"),
                    DateRecorded = record.DateRecorded?.ToString("dd-MM-yyyy"),
                    Indicator = $"{record.Alt}/{record.Ast}/{record.Alp}/{record.Ggt}",
                    Evaluation = evaluation
                };
                responses.Add(response);
            }

            if (!responses.Any())
            {
                responses.Add(new LogBookReponse
                {
                    Tabs = "LiverEnzyme",
                    DateRecorded = System.DateTime.Now.ToString(),
                    Indicator = "N/A",
                    Evaluation = "N/A"
                });
            }

            return responses;
        }

    }
}
