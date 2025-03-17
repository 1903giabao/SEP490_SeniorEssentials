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
using static Google.Cloud.Vision.V1.ProductSearchResults.Types;

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
        Task<IBusinessResult> EvaluateBloodGlusose(int bloodGlucose, string time);


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

        //tivo
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
                        WeekLabel = $"Week {CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, System.DayOfWeek.Monday)}, {date.Year}"
                    })
                    .ToList();

                var last4Months = Enumerable.Range(0, 4)
                    .Select(offset => today.AddMonths(-offset))
                    .OrderBy(date => date)
                    .Select(date => new
                    {
                        StartOfMonth = new System.DateTime(date.Year, date.Month, 1),
                        EndOfMonth = new System.DateTime(date.Year, date.Month, System.DateTime.DaysInMonth(date.Year, date.Month)),
                        MonthLabel = $"{CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(date.Month)} {date.Year}"
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
                        Type = x.Date.DayOfWeek.ToString(),
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
                        Type = x.Week.WeekLabel,
                        Indicator = x.Records.Any() ? (double?)Math.Round(x.Records.Average(w => w.Weight1 ?? 0), 2) : null
                    })
                    .OrderBy(record => System.DateTime.Parse(record.Type.Split(',')[1].Trim() + "-" + record.Type.Split(' ')[1].TrimStart('0')))
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
                        Type = x.Month.MonthLabel,
                        Indicator = x.Records.Any() ? (double?)Math.Round(x.Records.Average(w => w.Weight1 ?? 0), 2) : null
                    })
                    .OrderBy(record => System.DateTime.ParseExact(record.Type, "MMMM yyyy", CultureInfo.InvariantCulture))
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
                        WeekLabel = $"Week {CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, System.DayOfWeek.Monday)}, {date.Year}"
                    })
                    .ToList();

                var last4Months = Enumerable.Range(0, 4)
                    .Select(offset => today.AddMonths(-offset))
                    .OrderBy(date => date)
                    .Select(date => new
                    {
                        StartOfMonth = new System.DateTime(date.Year, date.Month, 1),
                        EndOfMonth = new System.DateTime(date.Year, date.Month, System.DateTime.DaysInMonth(date.Year, date.Month)),
                        MonthLabel = $"{CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(date.Month)} {date.Year}"
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
                        Type = x.Date.DayOfWeek.ToString(),
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
                        Type = x.Week.WeekLabel,
                        Indicator = x.Records.Any() ? (double?)x.Records.Average(h => h.Height1) : null // Keep null if no valid records
                    })
                    .OrderBy(record => System.DateTime.Parse(record.Type.Split(',')[1].Trim() + "-" + record.Type.Split(' ')[1].TrimStart('0')))
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
                        Type = x.Month.MonthLabel,
                        Indicator = x.Records.Any() ? (double?)x.Records.Average(h => h.Height1) : null // Keep null if no valid records
                    })
                    .OrderBy(record => System.DateTime.ParseExact(record.Type, "MMMM yyyy", CultureInfo.InvariantCulture))
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
                        WeekLabel = $"Week {CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, System.DayOfWeek.Monday)}, {date.Year}"
                    })
                    .ToList();

                var last4Months = Enumerable.Range(0, 4)
                    .Select(offset => today.AddMonths(-offset))
                    .OrderBy(date => date)
                    .Select(date => new
                    {
                        StartOfMonth = new System.DateTime(date.Year, date.Month, 1),
                        EndOfMonth = new System.DateTime(date.Year, date.Month, System.DateTime.DaysInMonth(date.Year, date.Month)),
                        MonthLabel = $"{CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(date.Month)} {date.Year}"
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
                        Type = x.Date.DayOfWeek.ToString(),
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
                        Type = x.Week.WeekLabel,
                        Indicator = x.Records.Any() ? (double?)Math.Round(x.Records.Average(h => h.HeartRate1 ?? 0),2) : null
                    })
                    .OrderBy(record => System.DateTime.Parse(record.Type.Split(',')[1].Trim() + "-" + record.Type.Split(' ')[1].TrimStart('0')))
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
                        Type = x.Month.MonthLabel,
                        Indicator = x.Records.Any() ? (double?)Math.Round(x.Records.Average(h => h.HeartRate1 ?? 0),2) : null
                    })
                    .OrderBy(record => System.DateTime.ParseExact(record.Type, "MMMM yyyy", CultureInfo.InvariantCulture))
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
            if (averageHeartRate < 60)
            {
                return "Thấp";
            }
            else if (averageHeartRate >= 60 && averageHeartRate <= 100)
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
                        WeekLabel = $"Week {CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, System.DayOfWeek.Monday)}, {date.Year}"
                    })
                    .ToList();

                var last4Months = Enumerable.Range(0, 4)
                    .Select(offset => today.AddMonths(-offset))
                    .OrderBy(date => date)
                    .Select(date => new
                    {
                        StartOfMonth = new System.DateTime(date.Year, date.Month, 1),
                        EndOfMonth = new System.DateTime(date.Year, date.Month, System.DateTime.DaysInMonth(date.Year, date.Month)),
                        MonthLabel = $"{CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(date.Month)} {date.Year}"
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
                        Type = x.Date.DayOfWeek.ToString(),
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
                        Type = x.Week.WeekLabel,
                        Indicator = x.Records.Any() ? $"{Math.Round(x.Records.Average(b => b.Systolic ?? 0), MidpointRounding.AwayFromZero)}/{Math.Round(x.Records.Average(b => b.Diastolic ?? 0), MidpointRounding.AwayFromZero)}" : null
                    })
                    .OrderBy(record => System.DateTime.Parse(record.Type.Split(',')[1].Trim() + "-" + record.Type.Split(' ')[1].TrimStart('0')))
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
                        Type = x.Month.MonthLabel,
                        Indicator = x.Records.Any() ? $"{Math.Round(x.Records.Average(b => b.Systolic ?? 0), MidpointRounding.AwayFromZero)}/{Math.Round(x.Records.Average(b => b.Diastolic ?? 0), MidpointRounding.AwayFromZero)}" : null
                    })
                    .OrderBy(record => System.DateTime.ParseExact(record.Type, "MMMM yyyy", CultureInfo.InvariantCulture))
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
            if (averageSystolic < 90 && averageDiastolic < 60)
            {
                return "Thấp";
            }
            else if (averageSystolic < 120 && averageDiastolic < 80)
            {
                return "Bình thường";
            }
            else if (averageSystolic >= 130 || averageDiastolic >= 80)
            {
                return "Cao";
            }
            else
            {
                return "Không xác định";
            }
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
                        WeekLabel = $"Week {CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, System.DayOfWeek.Monday)}, {date.Year}"
                    })
                    .ToList();

                var last4Months = Enumerable.Range(0, 4)
                    .Select(offset => today.AddMonths(-offset))
                    .OrderBy(date => date)
                    .Select(date => new
                    {
                        StartOfMonth = new System.DateTime(date.Year, date.Month, 1),
                        EndOfMonth = new System.DateTime(date.Year, date.Month, System.DateTime.DaysInMonth(date.Year, date.Month)),
                        MonthLabel = $"{CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(date.Month)} {date.Year}"
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

                    return (
                        LowPercent: (double)lowCount / totalRecords * 100,
                        NormalPercent: (double)normalCount / totalRecords * 100,
                        HighPercent: (double)highCount / totalRecords * 100
                    );
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
                        Type = x.Date.DayOfWeek.ToString(),
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
                        Type = x.Week.WeekLabel,
                        Indicator = x.Records.Any() ? (double?)Math.Round(x.Records.Average(b => b.BloodGlucose1 ?? 0), MidpointRounding.AwayFromZero) : null
                    })
                    .OrderBy(record => System.DateTime.Parse(record.Type.Split(',')[1].Trim() + "-" + record.Type.Split(' ')[1].TrimStart('0')))
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
                        Type = x.Month.MonthLabel,
                        Indicator = x.Records.Any() ? (double?)Math.Round(x.Records.Average(b => b.BloodGlucose1 ?? 0), MidpointRounding.AwayFromZero) : null
                    })
                    .OrderBy(record => System.DateTime.ParseExact(record.Type, "MMMM yyyy", CultureInfo.InvariantCulture))
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
                        WeekLabel = $"Week {CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, System.DayOfWeek.Monday)}, {date.Year}"
                    })
                    .ToList();

                var last4Months = Enumerable.Range(0, 4)
                    .Select(offset => today.AddMonths(-offset))
                    .OrderBy(date => date)
                    .Select(date => new
                    {
                        StartOfMonth = new System.DateTime(date.Year, date.Month, 1),
                        EndOfMonth = new System.DateTime(date.Year, date.Month, System.DateTime.DaysInMonth(date.Year, date.Month)),
                        MonthLabel = $"{CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(date.Month)} {date.Year}"
                    })
                    .ToList();

                // Calculate dailyRecords with non-null Indicator values
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
                        Type = x.Date.DayOfWeek.ToString(),
                        TotalCholesterol = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.TotalCholesterol ?? 0), 2) : null,
                        Ldlcholesterol = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Ldlcholesterol ?? 0), 2) : null,
                        Hdlcholesterol = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Hdlcholesterol ?? 0),2) : null,
                        Triglycerides = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Triglycerides ?? 0), 2) : null
                    })
                    .ToList();

                // Calculate weeklyRecords with non-null Indicator values
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
                        Type = x.Week.WeekLabel,
                        TotalCholesterol = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.TotalCholesterol ?? 0), 2) : null,
                        Ldlcholesterol = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Ldlcholesterol ?? 0), 2) : null,
                        Hdlcholesterol = x.Records.Any() ? (decimal?)x.Records.Average(l => l.Hdlcholesterol ?? 0) : null,
                        Triglycerides = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Triglycerides ?? 0), 2) : null
                    })
                    .OrderBy(record => System.DateTime.Parse(record.Type.Split(',')[1].Trim() + "-" + record.Type.Split(' ')[1].TrimStart('0')))
                    .ToList();

                // Calculate monthlyRecords with non-null Indicator values
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
                        Type = x.Month.MonthLabel,
                        TotalCholesterol = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.TotalCholesterol ?? 0), 2) : null,
                        Ldlcholesterol = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Ldlcholesterol ?? 0), 2) : null,
                        Hdlcholesterol = x.Records.Any() ? (decimal?)x.Records.Average(l => l.Hdlcholesterol ?? 0) : null,
                        Triglycerides = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Triglycerides ?? 0), 2) : null
                    })
                    .OrderBy(record => System.DateTime.ParseExact(record.Type, "MMMM yyyy", CultureInfo.InvariantCulture))
                    .ToList();

                // Calculate yearlyRecords with non-null Indicator values
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

                // Calculate averages for each tab (only non-null Indicators)
                var dailyTotalCholesterolAverage = dailyRecords
                    .Where(d => d.TotalCholesterol.HasValue)
                    .DefaultIfEmpty(new CharLipidProfileModel { TotalCholesterol = 0 })
                    .Average(d => d.TotalCholesterol.Value);

                var dailyLdlcholesterolAverage = dailyRecords
                    .Where(d => d.Ldlcholesterol.HasValue)
                    .DefaultIfEmpty(new CharLipidProfileModel { Ldlcholesterol = 0 })
                    .Average(d => d.Ldlcholesterol.Value);

                var dailyHdlcholesterolAverage = dailyRecords
                    .Where(d => d.Hdlcholesterol.HasValue)
                    .DefaultIfEmpty(new CharLipidProfileModel { Hdlcholesterol = 0 })
                    .Average(d => d.Hdlcholesterol.Value);

                var dailyTriglyceridesAverage = dailyRecords
                    .Where(d => d.Triglycerides.HasValue)
                    .DefaultIfEmpty(new CharLipidProfileModel { Triglycerides = 0 })
                    .Average(d => d.Triglycerides.Value);

                var weeklyTotalCholesterolAverage = weeklyRecords
                    .Where(w => w.TotalCholesterol.HasValue)
                    .DefaultIfEmpty(new CharLipidProfileModel { TotalCholesterol = 0 })
                    .Average(w => w.TotalCholesterol.Value);

                var weeklyLdlcholesterolAverage = weeklyRecords
                    .Where(w => w.Ldlcholesterol.HasValue)
                    .DefaultIfEmpty(new CharLipidProfileModel { Ldlcholesterol = 0 })
                    .Average(w => w.Ldlcholesterol.Value);

                var weeklyHdlcholesterolAverage = weeklyRecords
                    .Where(w => w.Hdlcholesterol.HasValue)
                    .DefaultIfEmpty(new CharLipidProfileModel { Hdlcholesterol = 0 })
                    .Average(w => w.Hdlcholesterol.Value);

                var weeklyTriglyceridesAverage = weeklyRecords
                    .Where(w => w.Triglycerides.HasValue)
                    .DefaultIfEmpty(new CharLipidProfileModel { Triglycerides = 0 })
                    .Average(w => w.Triglycerides.Value);

                var monthlyTotalCholesterolAverage = monthlyRecords
                    .Where(m => m.TotalCholesterol.HasValue)
                    .DefaultIfEmpty(new CharLipidProfileModel { TotalCholesterol = 0 })
                    .Average(m => m.TotalCholesterol.Value);

                var monthlyLdlcholesterolAverage = monthlyRecords
                    .Where(m => m.Ldlcholesterol.HasValue)
                    .DefaultIfEmpty(new CharLipidProfileModel { Ldlcholesterol = 0 })
                    .Average(m => m.Ldlcholesterol.Value);

                var monthlyHdlcholesterolAverage = monthlyRecords
                    .Where(m => m.Hdlcholesterol.HasValue)
                    .DefaultIfEmpty(new CharLipidProfileModel { Hdlcholesterol = 0 })
                    .Average(m => m.Hdlcholesterol.Value);

                var monthlyTriglyceridesAverage = monthlyRecords
                    .Where(m => m.Triglycerides.HasValue)
                    .DefaultIfEmpty(new CharLipidProfileModel { Triglycerides = 0 })
                    .Average(m => m.Triglycerides.Value);

                var yearlyTotalCholesterolAverage = yearlyRecords
                    .Where(y => y.TotalCholesterol.HasValue)
                    .DefaultIfEmpty(new CharLipidProfileModel { TotalCholesterol = 0 })
                    .Average(y => y.TotalCholesterol.Value);

                var yearlyLdlcholesterolAverage = yearlyRecords
                    .Where(y => y.Ldlcholesterol.HasValue)
                    .DefaultIfEmpty(new CharLipidProfileModel { Ldlcholesterol = 0 })
                    .Average(y => y.Ldlcholesterol.Value);

                var yearlyHdlcholesterolAverage = yearlyRecords
                    .Where(y => y.Hdlcholesterol.HasValue)
                    .DefaultIfEmpty(new CharLipidProfileModel { Hdlcholesterol = 0 })
                    .Average(y => y.Hdlcholesterol.Value);

                var yearlyTriglyceridesAverage = yearlyRecords
                    .Where(y => y.Triglycerides.HasValue)
                    .DefaultIfEmpty(new CharLipidProfileModel { Triglycerides = 0 })
                    .Average(y => y.Triglycerides.Value);

                // Calculate Evaluation for each tab
                var dailyTotalCholesterolEvaluation = GetTotalCholesterolEvaluation(dailyTotalCholesterolAverage);
                var dailyLdlcholesterolEvaluation = GetLdlcholesterolEvaluation(dailyLdlcholesterolAverage);
                var dailyHdlcholesterolEvaluation = GetHdlcholesterolEvaluation(dailyHdlcholesterolAverage);
                var dailyTriglyceridesEvaluation = GetTriglyceridesEvaluation(dailyTriglyceridesAverage);

                var weeklyTotalCholesterolEvaluation = GetTotalCholesterolEvaluation(weeklyTotalCholesterolAverage);
                var weeklyLdlcholesterolEvaluation = GetLdlcholesterolEvaluation(weeklyLdlcholesterolAverage);
                var weeklyHdlcholesterolEvaluation = GetHdlcholesterolEvaluation(weeklyHdlcholesterolAverage);
                var weeklyTriglyceridesEvaluation = GetTriglyceridesEvaluation(weeklyTriglyceridesAverage);

                var monthlyTotalCholesterolEvaluation = GetTotalCholesterolEvaluation(monthlyTotalCholesterolAverage);
                var monthlyLdlcholesterolEvaluation = GetLdlcholesterolEvaluation(monthlyLdlcholesterolAverage);
                var monthlyHdlcholesterolEvaluation = GetHdlcholesterolEvaluation(monthlyHdlcholesterolAverage);
                var monthlyTriglyceridesEvaluation = GetTriglyceridesEvaluation(monthlyTriglyceridesAverage);

                var yearlyTotalCholesterolEvaluation = GetTotalCholesterolEvaluation(yearlyTotalCholesterolAverage);
                var yearlyLdlcholesterolEvaluation = GetLdlcholesterolEvaluation(yearlyLdlcholesterolAverage);
                var yearlyHdlcholesterolEvaluation = GetHdlcholesterolEvaluation(yearlyHdlcholesterolAverage);
                var yearlyTriglyceridesEvaluation = GetTriglyceridesEvaluation(yearlyTriglyceridesAverage);

                var responseList = new List<GetLipidProfileDetail>
        {
            new GetLipidProfileDetail
            {
                Tabs = "Ngày",
                TotalCholesterolAverage = dailyTotalCholesterolAverage,
                LdlcholesterolAverage = dailyLdlcholesterolAverage,
                HdlcholesterolAverage = dailyHdlcholesterolAverage,
                TriglyceridesAverage = dailyTriglyceridesAverage,
                TotalCholesterolEvaluation = dailyTotalCholesterolEvaluation,
                LdlcholesteroEvaluation = dailyLdlcholesterolEvaluation,
                HdlcholesterolEvaluation = dailyHdlcholesterolEvaluation,
                TriglyceridesEvaluation = dailyTriglyceridesEvaluation,
                ChartDatabase = dailyRecords
            },
            new GetLipidProfileDetail
            {
                Tabs = "Tuần",
                TotalCholesterolAverage = weeklyTotalCholesterolAverage,
                LdlcholesterolAverage = weeklyLdlcholesterolAverage,
                HdlcholesterolAverage = weeklyHdlcholesterolAverage,
                TriglyceridesAverage = weeklyTriglyceridesAverage,
                TotalCholesterolEvaluation = weeklyTotalCholesterolEvaluation,
                LdlcholesteroEvaluation = weeklyLdlcholesterolEvaluation,
                HdlcholesterolEvaluation = weeklyHdlcholesterolEvaluation,
                TriglyceridesEvaluation = weeklyTriglyceridesEvaluation,
                ChartDatabase = weeklyRecords
            },
            new GetLipidProfileDetail
            {
                Tabs = "Tháng",
                TotalCholesterolAverage = monthlyTotalCholesterolAverage,
                LdlcholesterolAverage = monthlyLdlcholesterolAverage,
                HdlcholesterolAverage = monthlyHdlcholesterolAverage,
                TriglyceridesAverage = monthlyTriglyceridesAverage,
                TotalCholesterolEvaluation = monthlyTotalCholesterolEvaluation,
                LdlcholesteroEvaluation = monthlyLdlcholesterolEvaluation,
                HdlcholesterolEvaluation = monthlyHdlcholesterolEvaluation,
                TriglyceridesEvaluation = monthlyTriglyceridesEvaluation,
                ChartDatabase = monthlyRecords
            },
            new GetLipidProfileDetail
            {
                Tabs = "Năm",
                TotalCholesterolAverage = yearlyTotalCholesterolAverage,
                LdlcholesterolAverage = yearlyLdlcholesterolAverage,
                HdlcholesterolAverage = yearlyHdlcholesterolAverage,
                TriglyceridesAverage = yearlyTriglyceridesAverage,
                TotalCholesterolEvaluation = yearlyTotalCholesterolEvaluation,
                LdlcholesteroEvaluation = yearlyLdlcholesterolEvaluation,
                HdlcholesterolEvaluation = yearlyHdlcholesterolEvaluation,
                TriglyceridesEvaluation = yearlyTriglyceridesEvaluation,
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

        private string GetTotalCholesterolEvaluation(decimal averageTotalCholesterol)
        {
            if (averageTotalCholesterol < 160)
            {
                return "Thấp";
            }
            else if (averageTotalCholesterol >= 160 && averageTotalCholesterol < 180)
            {
                return "Bình thường";
            }
            else if (averageTotalCholesterol >= 180 && averageTotalCholesterol < 220)
            {
                return "Cao";
            }
         
            else
            {
                return "Không xác định";
            }
        }

        private string GetLdlcholesterolEvaluation(decimal averageLdlcholesterol)
        {
            if (averageLdlcholesterol < 40)
            {
                return "Thấp";
            }
            else if (averageLdlcholesterol >= 40 && averageLdlcholesterol < 120)
            {
                return "Bình thường";
            }
            else if (averageLdlcholesterol >= 120 && averageLdlcholesterol < 170)
            {
                return "Cao";
            }
            else if (averageLdlcholesterol >= 170)
            {
                return "Rất cao";
            }
            else
            {
                return "Không xác định";
            }
        }

        private string GetHdlcholesterolEvaluation(decimal averageHdlcholesterol)
        {
            if (averageHdlcholesterol < 40)
            {
                return "Thấp";
            }
            else if (averageHdlcholesterol >= 40 && averageHdlcholesterol < 65)
            {
                return "Bình thường";
            }
            else if (averageHdlcholesterol >= 65)
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
            if (averageTriglycerides < 50)
            {
                return "Thấp";
            }
            else if (averageTriglycerides >= 50 && averageTriglycerides < 170)
            {
                return "Bình thường";
            }
            else if (averageTriglycerides >= 170)
            {
                return "Hơi cao";
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
                        WeekLabel = $"Week {CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, System.DayOfWeek.Monday)}, {date.Year}"
                    })
                    .ToList();

                var last4Months = Enumerable.Range(0, 4)
                    .Select(offset => today.AddMonths(-offset))
                    .OrderBy(date => date)
                    .Select(date => new
                    {
                        StartOfMonth = new System.DateTime(date.Year, date.Month, 1),
                        EndOfMonth = new System.DateTime(date.Year, date.Month, System.DateTime.DaysInMonth(date.Year, date.Month)),
                        MonthLabel = $"{CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(date.Month)} {date.Year}"
                    })
                    .ToList();

                // Calculate dailyRecords with non-null Indicator values
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
                        Type = x.Date.DayOfWeek.ToString(),
                        Alt = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Alt ?? 0), 2) : null,
                        Ast = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Ast ?? 0), 2) : null,
                        Alp = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Alp ?? 0), 2) : null,
                        Ggt = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Ggt ?? 0), 2) : null
                    })
                    .ToList();

                // Calculate weeklyRecords with non-null Indicator values
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
                        Type = x.Week.WeekLabel,
                        Alt = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Alt ?? 0), 2) : null,
                        Ast = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Ast ?? 0), 2) : null,
                        Alp = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Alp ?? 0), 2) : null,
                        Ggt = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Ggt ?? 0), 2) : null
                    })
                    .OrderBy(record => System.DateTime.Parse(record.Type.Split(',')[1].Trim() + "-" + record.Type.Split(' ')[1].TrimStart('0')))
                    .ToList();

                // Calculate monthlyRecords with non-null Indicator values
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
                        Type = x.Month.MonthLabel,
                        Alt = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Alt ?? 0), 2) : null,
                        Ast = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Ast ?? 0), 2) : null,
                        Alp = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Alp ?? 0), 2) : null,
                        Ggt = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(l => l.Ggt ?? 0), 2) : null
                    })
                    .OrderBy(record => System.DateTime.ParseExact(record.Type, "MMMM yyyy", CultureInfo.InvariantCulture))
                    .ToList();

                // Calculate yearlyRecords with non-null Indicator values
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

                // Calculate averages for each tab (only non-null Indicators)
                var dailyAltAverage = dailyRecords
                    .Where(d => d.Alt.HasValue)
                    .DefaultIfEmpty(new CharLiverEnzymesModel { Alt = 0 })
                    .Average(d => d.Alt.Value);

                var dailyAstAverage = dailyRecords
                    .Where(d => d.Ast.HasValue)
                    .DefaultIfEmpty(new CharLiverEnzymesModel { Ast = 0 })
                    .Average(d => d.Ast.Value);

                var dailyAlpAverage = dailyRecords
                    .Where(d => d.Alp.HasValue)
                    .DefaultIfEmpty(new CharLiverEnzymesModel { Alp = 0 })
                    .Average(d => d.Alp.Value);

                var dailyGgtAverage = dailyRecords
                    .Where(d => d.Ggt.HasValue)
                    .DefaultIfEmpty(new CharLiverEnzymesModel { Ggt = 0 })
                    .Average(d => d.Ggt.Value);

                var weeklyAltAverage = weeklyRecords
                    .Where(w => w.Alt.HasValue)
                    .DefaultIfEmpty(new CharLiverEnzymesModel { Alt = 0 })
                    .Average(w => w.Alt.Value);

                var weeklyAstAverage = weeklyRecords
                    .Where(w => w.Ast.HasValue)
                    .DefaultIfEmpty(new CharLiverEnzymesModel { Ast = 0 })
                    .Average(w => w.Ast.Value);

                var weeklyAlpAverage = weeklyRecords
                    .Where(w => w.Alp.HasValue)
                    .DefaultIfEmpty(new CharLiverEnzymesModel { Alp = 0 })
                    .Average(w => w.Alp.Value);

                var weeklyGgtAverage = weeklyRecords
                    .Where(w => w.Ggt.HasValue)
                    .DefaultIfEmpty(new CharLiverEnzymesModel { Ggt = 0 })
                    .Average(w => w.Ggt.Value);

                var monthlyAltAverage = monthlyRecords
                    .Where(m => m.Alt.HasValue)
                    .DefaultIfEmpty(new CharLiverEnzymesModel { Alt = 0 })
                    .Average(m => m.Alt.Value);

                var monthlyAstAverage = monthlyRecords
                    .Where(m => m.Ast.HasValue)
                    .DefaultIfEmpty(new CharLiverEnzymesModel { Ast = 0 })
                    .Average(m => m.Ast.Value);

                var monthlyAlpAverage = monthlyRecords
                    .Where(m => m.Alp.HasValue)
                    .DefaultIfEmpty(new CharLiverEnzymesModel { Alp = 0 })
                    .Average(m => m.Alp.Value);

                var monthlyGgtAverage = monthlyRecords
                    .Where(m => m.Ggt.HasValue)
                    .DefaultIfEmpty(new CharLiverEnzymesModel { Ggt = 0 })
                    .Average(m => m.Ggt.Value);

                var yearlyAltAverage = yearlyRecords
                    .Where(y => y.Alt.HasValue)
                    .DefaultIfEmpty(new CharLiverEnzymesModel { Alt = 0 })
                    .Average(y => y.Alt.Value);

                var yearlyAstAverage = yearlyRecords
                    .Where(y => y.Ast.HasValue)
                    .DefaultIfEmpty(new CharLiverEnzymesModel { Ast = 0 })
                    .Average(y => y.Ast.Value);

                var yearlyAlpAverage = yearlyRecords
                    .Where(y => y.Alp.HasValue)
                    .DefaultIfEmpty(new CharLiverEnzymesModel { Alp = 0 })
                    .Average(y => y.Alp.Value);

                var yearlyGgtAverage = yearlyRecords
                    .Where(y => y.Ggt.HasValue)
                    .DefaultIfEmpty(new CharLiverEnzymesModel { Ggt = 0 })
                    .Average(y => y.Ggt.Value);

                // Calculate Evaluation for each tab
                var dailyAltEvaluation = GetAltEvaluation(dailyAltAverage);
                var dailyAstEvaluation = GetAstEvaluation(dailyAstAverage);
                var dailyAlpEvaluation = GetAlpEvaluation(dailyAlpAverage);
                var dailyGgtEvaluation = GetGgtEvaluation(dailyGgtAverage);

                var weeklyAltEvaluation = GetAltEvaluation(weeklyAltAverage);
                var weeklyAstEvaluation = GetAstEvaluation(weeklyAstAverage);
                var weeklyAlpEvaluation = GetAlpEvaluation(weeklyAlpAverage);
                var weeklyGgtEvaluation = GetGgtEvaluation(weeklyGgtAverage);

                var monthlyAltEvaluation = GetAltEvaluation(monthlyAltAverage);
                var monthlyAstEvaluation = GetAstEvaluation(monthlyAstAverage);
                var monthlyAlpEvaluation = GetAlpEvaluation(monthlyAlpAverage);
                var monthlyGgtEvaluation = GetGgtEvaluation(monthlyGgtAverage);

                var yearlyAltEvaluation = GetAltEvaluation(yearlyAltAverage);
                var yearlyAstEvaluation = GetAstEvaluation(yearlyAstAverage);
                var yearlyAlpEvaluation = GetAlpEvaluation(yearlyAlpAverage);
                var yearlyGgtEvaluation = GetGgtEvaluation(yearlyGgtAverage);

                var responseList = new List<GetLiverEnzymesDetail>
        {
            new GetLiverEnzymesDetail
            {
                Tabs = "Ngày",
                AltAverage = dailyAltAverage,
                AstAverage = dailyAstAverage,
                AlpAverage = dailyAlpAverage,
                GgtAverage = dailyGgtAverage,
                AltEvaluation = dailyAltEvaluation,
                AstEvaluation = dailyAstEvaluation,
                AlpEvaluation = dailyAlpEvaluation,
                GgtEvaluation = dailyGgtEvaluation,
                ChartDatabase = dailyRecords
            },
            new GetLiverEnzymesDetail
            {
                Tabs = "Tuần",
                AltAverage = weeklyAltAverage,
                AstAverage = weeklyAstAverage,
                AlpAverage = weeklyAlpAverage,
                GgtAverage = weeklyGgtAverage,
                AltEvaluation = weeklyAltEvaluation,
                AstEvaluation = weeklyAstEvaluation,
                AlpEvaluation = weeklyAlpEvaluation,
                GgtEvaluation = weeklyGgtEvaluation,
                ChartDatabase = weeklyRecords
            },
            new GetLiverEnzymesDetail
            {
                Tabs = "Tháng",
                AltAverage = monthlyAltAverage,
                AstAverage = monthlyAstAverage,
                AlpAverage = monthlyAlpAverage,
                GgtAverage = monthlyGgtAverage,
                AltEvaluation = monthlyAltEvaluation,
                AstEvaluation = monthlyAstEvaluation,
                AlpEvaluation = monthlyAlpEvaluation,
                GgtEvaluation = monthlyGgtEvaluation,
                ChartDatabase = monthlyRecords
            },
            new GetLiverEnzymesDetail
            {
                Tabs = "Năm",
                AltAverage = yearlyAltAverage,
                AstAverage = yearlyAstAverage,
                AlpAverage = yearlyAlpAverage,
                GgtAverage = yearlyGgtAverage,
                AltEvaluation = yearlyAltEvaluation,
                AstEvaluation = yearlyAstEvaluation,
                AlpEvaluation = yearlyAlpEvaluation,
                GgtEvaluation = yearlyGgtEvaluation,
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

        private string GetAltEvaluation(decimal averageAlt)
        {
            if (averageAlt >= 7 && averageAlt <= 56)
            {
                return "Bình thường";
            }
            else if (averageAlt > 56)
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
            if (averageAst >= 10 && averageAst <= 40)
            {
                return "Bình thường";
            }
            else if (averageAst > 40)
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
            if (averageAlp >= 44 && averageAlp <= 147)
            {
                return "Bình thường";
            }
            else if (averageAlp > 147)
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
            if (averageGgt >= 8 && averageGgt <= 50)
            {
                return "Bình thường";
            }
            else if (averageGgt > 50)
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
                        WeekLabel = $"Week {CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, System.DayOfWeek.Monday)}, {date.Year}"
                    })
                    .ToList();

                var last4Months = Enumerable.Range(0, 4)
                    .Select(offset => today.AddMonths(-offset))
                    .OrderBy(date => date)
                    .Select(date => new
                    {
                        StartOfMonth = new System.DateTime(date.Year, date.Month, 1),
                        EndOfMonth = new System.DateTime(date.Year, date.Month, System.DateTime.DaysInMonth(date.Year, date.Month)),
                        MonthLabel = $"{CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(date.Month)} {date.Year}"
                    })
                    .ToList();

                // Calculate dailyRecords with non-null Indicator values
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
                        Type = x.Date.DayOfWeek.ToString(),
                        Creatinine = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(k => k.Creatinine ?? 0), 2) : null,
                        Bun = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(k => k.Bun ?? 0), 2) : null,
                        EGfr = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(k => k.EGfr ?? 0), 2) : null
                    })
                    .ToList();

                // Calculate weeklyRecords with non-null Indicator values
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
                        Type = x.Week.WeekLabel,
                        Creatinine = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(k => k.Creatinine ?? 0), 2) : null,
                        Bun = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(k => k.Bun ?? 0), 2) : null,
                        EGfr = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(k => k.EGfr ?? 0), 2) : null
                    })
                    .OrderBy(record => System.DateTime.Parse(record.Type.Split(',')[1].Trim() + "-" + record.Type.Split(' ')[1].TrimStart('0')))
                    .ToList();

                // Calculate monthlyRecords with non-null Indicator values
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
                        Type = x.Month.MonthLabel,
                        Creatinine = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(k => k.Creatinine ?? 0), 2) : null,
                        Bun = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(k => k.Bun ?? 0), 2) : null,
                        EGfr = x.Records.Any() ? (decimal?)Math.Round(x.Records.Average(k => k.EGfr ?? 0), 2) : null
                    })
                    .OrderBy(record => System.DateTime.ParseExact(record.Type, "MMMM yyyy", CultureInfo.InvariantCulture))
                    .ToList();

                // Calculate yearlyRecords with non-null Indicator values
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

                // Calculate averages for each tab (only non-null Indicators)
                var dailyCreatinineAverage = dailyRecords
                    .Where(d => d.Creatinine.HasValue)
                    .DefaultIfEmpty(new CharKidneyFunctionModel { Creatinine = 0 })
                    .Average(d => d.Creatinine.Value);

                var dailyBunAverage = dailyRecords
                    .Where(d => d.Bun.HasValue)
                    .DefaultIfEmpty(new CharKidneyFunctionModel { Bun = 0 })
                    .Average(d => d.Bun.Value);

                var dailyEGfrAverage = dailyRecords
                    .Where(d => d.EGfr.HasValue)
                    .DefaultIfEmpty(new CharKidneyFunctionModel { EGfr = 0 })
                    .Average(d => d.EGfr.Value);

                var weeklyCreatinineAverage = weeklyRecords
                    .Where(w => w.Creatinine.HasValue)
                    .DefaultIfEmpty(new CharKidneyFunctionModel { Creatinine = 0 })
                    .Average(w => w.Creatinine.Value);

                var weeklyBunAverage = weeklyRecords
                    .Where(w => w.Bun.HasValue)
                    .DefaultIfEmpty(new CharKidneyFunctionModel { Bun = 0 })
                    .Average(w => w.Bun.Value);

                var weeklyEGfrAverage = weeklyRecords
                    .Where(w => w.EGfr.HasValue)
                    .DefaultIfEmpty(new CharKidneyFunctionModel { EGfr = 0 })
                    .Average(w => w.EGfr.Value);

                var monthlyCreatinineAverage = monthlyRecords
                    .Where(m => m.Creatinine.HasValue)
                    .DefaultIfEmpty(new CharKidneyFunctionModel { Creatinine = 0 })
                    .Average(m => m.Creatinine.Value);

                var monthlyBunAverage = monthlyRecords
                    .Where(m => m.Bun.HasValue)
                    .DefaultIfEmpty(new CharKidneyFunctionModel { Bun = 0 })
                    .Average(m => m.Bun.Value);

                var monthlyEGfrAverage = monthlyRecords
                    .Where(m => m.EGfr.HasValue)
                    .DefaultIfEmpty(new CharKidneyFunctionModel { EGfr = 0 })
                    .Average(m => m.EGfr.Value);

                var yearlyCreatinineAverage = yearlyRecords
                    .Where(y => y.Creatinine.HasValue)
                    .DefaultIfEmpty(new CharKidneyFunctionModel { Creatinine = 0 })
                    .Average(y => y.Creatinine.Value);

                var yearlyBunAverage = yearlyRecords
                    .Where(y => y.Bun.HasValue)
                    .DefaultIfEmpty(new CharKidneyFunctionModel { Bun = 0 })
                    .Average(y => y.Bun.Value);

                var yearlyEGfrAverage = yearlyRecords
                    .Where(y => y.EGfr.HasValue)
                    .DefaultIfEmpty(new CharKidneyFunctionModel { EGfr = 0 })
                    .Average(y => y.EGfr.Value);

                // Calculate Evaluation for each tab
                var dailyCreatinineEvaluation = GetCreatinineEvaluation(dailyCreatinineAverage);
                var dailyBunEvaluation = GetBunEvaluation(dailyBunAverage);
                var dailyEGfrEvaluation = GetEGfrEvaluation(dailyEGfrAverage);

                var weeklyCreatinineEvaluation = GetCreatinineEvaluation(weeklyCreatinineAverage);
                var weeklyBunEvaluation = GetBunEvaluation(weeklyBunAverage);
                var weeklyEGfrEvaluation = GetEGfrEvaluation(weeklyEGfrAverage);

                var monthlyCreatinineEvaluation = GetCreatinineEvaluation(monthlyCreatinineAverage);
                var monthlyBunEvaluation = GetBunEvaluation(monthlyBunAverage);
                var monthlyEGfrEvaluation = GetEGfrEvaluation(monthlyEGfrAverage);

                var yearlyCreatinineEvaluation = GetCreatinineEvaluation(yearlyCreatinineAverage);
                var yearlyBunEvaluation = GetBunEvaluation(yearlyBunAverage);
                var yearlyEGfrEvaluation = GetEGfrEvaluation(yearlyEGfrAverage);

                var responseList = new List<GetKidneyFunctionDetail>
        {
            new GetKidneyFunctionDetail
            {
                Tabs = "Ngày",
                CreatinineAverage = dailyCreatinineAverage,
                BunAverage = dailyBunAverage,
                EGfrAverage = dailyEGfrAverage,
                CreatinineEvaluation = dailyCreatinineEvaluation,
                BunEvaluation = dailyBunEvaluation,
                EGfrEvaluation = dailyEGfrEvaluation,
                ChartDatabase = dailyRecords
            },
            new GetKidneyFunctionDetail
            {
                Tabs = "Tuần",
                CreatinineAverage = weeklyCreatinineAverage,
                BunAverage = weeklyBunAverage,
                EGfrAverage = weeklyEGfrAverage,
                CreatinineEvaluation = weeklyCreatinineEvaluation,
                BunEvaluation = weeklyBunEvaluation,
                EGfrEvaluation = weeklyEGfrEvaluation,
                ChartDatabase = weeklyRecords
            },
            new GetKidneyFunctionDetail
            {
                Tabs = "Tháng",
                CreatinineAverage = monthlyCreatinineAverage,
                BunAverage = monthlyBunAverage,
                EGfrAverage = monthlyEGfrAverage,
                CreatinineEvaluation = monthlyCreatinineEvaluation,
                BunEvaluation = monthlyBunEvaluation,
                EGfrEvaluation = monthlyEGfrEvaluation,
                ChartDatabase = monthlyRecords
            },
            new GetKidneyFunctionDetail
            {
                Tabs = "Năm",
                CreatinineAverage = yearlyCreatinineAverage,
                BunAverage = yearlyBunAverage,
                EGfrAverage = yearlyEGfrAverage,
                CreatinineEvaluation = yearlyCreatinineEvaluation,
                BunEvaluation = yearlyBunEvaluation,
                EGfrEvaluation = yearlyEGfrEvaluation,
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

        private string GetCreatinineEvaluation(decimal averageCreatinine)
        {
            if (averageCreatinine >= 0.6m && averageCreatinine <= 1.2m)
            {
                return "Bình thường";
            }
            else if (averageCreatinine < 0.6m)
            {
                return "Thấp";
            }
            else if (averageCreatinine > 1.2m)
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
            if (averageBun >= 7 && averageBun <= 20)
            {
                return "Bình thường";
            }
            else if (averageBun < 7)
            {
                return "Thấp";
            }
            else if (averageBun > 20)
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
            if (averageEGfr >= 90 && averageEGfr <= 120)
            {
                return "Bình thường";
            }
            else if (averageEGfr < 60)
            {
                return "Thấp";
            }
            else if (averageEGfr < 15)
            {
                return "Rất thấp";
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

                // Fetch all HealthIndicatorBase records for each type
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

                    // Get the HealthIndicatorBase for the type
                    var indicatorBase = indicators.FirstOrDefault(h => h.Type == type);
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
                        .Where(r => (System.DateTime)getDateRecorded(r) >= System.DateTime.UtcNow.AddDays(-30))
                        .ToList();

                    var averageIndicator = last30DaysRecords.Any() ?
                        last30DaysRecords.Average(r => getIndicatorValue(r) ?? 0) :
                        0;

                    // For weight and height, calculate the difference with the previous indicator
                    string formattedAverageIndicator = averageIndicator.ToString("0");
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
                    }

                    return new GetAllHealthIndicatorReponse
                    {
                        Tabs = type,
                        Evaluation = evaluation,
                        DateTime = latestDate.Value.ToString("dd-MM HH-mm"), // Format DateTime as DD-MM HH-mm
                        Indicator = latestIndicator.Value.ToString("0"),
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
                    calculateDifference: true, // Enable difference calculation for height
                    bmi: bmi); // Pass BMI for evaluation

                var weightResponse = GetIndicatorResponse(
                    weightRecords,
                    w => w.DateRecorded,
                    w => w.Weight1,
                    "Weight",
                    healthIndicators,
                    calculateDifference: true, // Enable difference calculation for weight
                    bmi: bmi); // Pass BMI for evaluation

                var heartRateResponse = GetIndicatorResponse(
                    heartRateRecords,
                    hr => hr.DateRecorded,
                    hr => hr.HeartRate1,
                    "HeartRate",
                    healthIndicators);

                // Special handling for Blood Pressure
                var bloodPressureResponse = GetBloodPressureResponse(bloodPressureRecords, healthIndicators);

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

                return new BusinessResult(Const.SUCCESS_READ, "Health indicators retrieved successfully.", responseList);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        // Function to handle Blood Pressure response
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
                : "Cao hơn mức bình thường";

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
                DateTime = latestDate.Value.ToString("dd-MM HH-mm"), // Format DateTime as DD-MM HH-mm
                Indicator = indicator,
                AverageIndicator = averageIndicator
            };
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
                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);

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
                    result = "Bình thường";
                }
                else if (systolic > baseSystolic.MaxValue && diastolic > baseDiastolic.MaxValue)
                {
                    result = "Cao hơn mức bình thường";
                }
                else
                {
                    result = "Không xác định";
                }
                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<IBusinessResult> EvaluateBloodGlusose(int bloodGlucose, string time)
        {

            try
            {
                var baseBloodGlucose = _unitOfWork.HealthIndicatorBaseRepository.
                                                    FindByCondition(i => i.Type == "BloodGlucose" && i.Time == time)
                                                    .FirstOrDefault();

                string result;
                if (bloodGlucose < baseBloodGlucose.MaxValue && bloodGlucose > baseBloodGlucose.MinValue)
                {
                    result = "Thận bình thường";
                }
                else if (bloodGlucose > baseBloodGlucose.MaxValue)
                {
                    result = "Thận cao hơn mức bình thường";
                }
                else
                {
                    result = "Thận thấp hơn mức bình thường";
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
                    creatinineResult = "Creatinine thấp hơn mức bình thường";
                }
                else if (creatinine > baseCreatinine.MaxValue)
                {
                    creatinineResult = "Creatinine cao hơn mức bình thường";
                }
                else
                {
                    creatinineResult = "Creatinine bình thường";
                }

                // Evaluate BUN levels
                string BUNResult;
                if (BUN < baseBUN.MinValue)
                {
                    BUNResult = "BUN thấp hơn mức bình thường";
                }
                else if (BUN > baseBUN.MaxValue)
                {
                    BUNResult = "BUN cao hơn mức bình thường";
                }
                else
                {
                    BUNResult = "BUN level is normal";
                }

                // Evaluate eGFR levels
                string eGFRResult;
                if (eGFR < baseEGFR.MinValue)
                {
                    eGFRResult = "eGFR thấp hơn mức bình thường";
                }
                else if (eGFR > baseEGFR.MaxValue)
                {
                    eGFRResult = "eGFR cao hơn mức bình thường";
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
                    totalCholesterolResult = "Toàn phần Cholesterol thấp hơn mức bình thường";
                }
                else if (totalCholesterol > baseTotalCholesterol.MaxValue)
                {
                    totalCholesterolResult = "Toàn phần Cholesterol cao hơn mức bình thường";
                }
                else
                {
                    totalCholesterolResult = "Toàn phần Cholesterol bình thường";
                }

                // Evaluate LDL Cholesterol
                string ldlCholesterolResult;
                if (ldlCholesterol < baseLDLCholesterol.MinValue)
                {
                    ldlCholesterolResult = "LDL Cholesterol thấp hơn mức bình thường";
                }
                else if (ldlCholesterol > baseLDLCholesterol.MaxValue)
                {
                    ldlCholesterolResult = "LDL Cholesterol cao hơn mức bình thường";
                }
                else
                {
                    ldlCholesterolResult = "LDL Cholesterol bình thường";
                }

                // Evaluate HDL Cholesterol
                string hdlCholesterolResult;
                if (hdlCholesterol < baseHDLCholesterol.MinValue)
                {
                    hdlCholesterolResult = "HDL Cholesterol thấp hơn mức bình thường";
                }
                else if (hdlCholesterol > baseHDLCholesterol.MaxValue)
                {
                    hdlCholesterolResult = "HDL Cholesterol cao hơn mức bình thường";
                }
                else
                {
                    hdlCholesterolResult = "HDL Cholesterol bình thường";
                }

                // Evaluate Triglycerides
                string triglyceridesResult;
                if (triglycerides < baseTriglycerides.MinValue)
                {
                    triglyceridesResult = "Triglycerides thấp hơn mức bình thường";
                }
                else if (triglycerides > baseTriglycerides.MaxValue)
                {
                    triglyceridesResult = "Triglycerides cao hơn mức bình thường";
                }
                else
                {
                    triglyceridesResult = "Triglycerides bình thường";
                }

                // Combine the results into a single message
                string combinedResult = $"{totalCholesterolResult} - {ldlCholesterolResult} - {hdlCholesterolResult}, Triglycerides: {triglyceridesResult}";

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
                    altResult = "ALT thấp hơn mức bình thường";
                }
                else if (alt > baseALT.MaxValue)
                {
                    altResult = "ALT cao hơn mức bình thường";
                }
                else
                {
                    altResult = "ALT bình thường";
                }

                // Evaluate AST (Aspartate Aminotransferase)
                string astResult;
                if (ast < baseAST.MinValue)
                {
                    astResult = "AST thấp hơn mức bình thường";
                }
                else if (ast > baseAST.MaxValue)
                {
                    astResult = "AST cao hơn mức bình thường";
                }
                else
                {
                    astResult = "AST bình thường";
                }

                // Evaluate ALP (Alkaline Phosphatase)
                string alpResult;
                if (alp < baseALP.MinValue)
                {
                    alpResult = "ALP thấp hơn mức bình thường";
                }
                else if (alp > baseALP.MaxValue)
                {
                    alpResult = "ALP cao hơn mức bình thường";
                }
                else
                {
                    alpResult = "ALP bình thường";
                }

                // Evaluate GGT (Gamma-Glutamyl Transferase)
                string ggtResult;
                if (ggt < baseGGT.MinValue)
                {
                    ggtResult = "GGT thấp hơn mức bình thường";
                }
                else if (ggt > baseGGT.MaxValue)
                {
                    ggtResult = "GGT cao hơn mức bình thường";
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
    }
}
