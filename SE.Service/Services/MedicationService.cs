using AutoMapper;
using Microsoft.AspNetCore.Http;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Ocsp;
using SE.Common;
using SE.Common.DTO;
using SE.Common.Enums;
using SE.Common.Request;
using SE.Data.Models;
using SE.Data.UnitOfWork;
using SE.Service.Base;
using SE.Service.Helper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Tesseract;
using static SE.Common.DTO.GetPresciptionFromScan;
using static System.Net.Mime.MediaTypeNames;


namespace SE.Service.Services
{
    public interface IMedicationService
    {
        Task<IBusinessResult> ScanFromPic(IFormFile file, int ElderlyID);
       Task<IBusinessResult> GetMedicationsForToday(int elderlyId, System.DateTime today);


        Task<IBusinessResult> CreateMedicationByManually(CreateMedicationRequest req);


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


        public async Task<IBusinessResult> ScanFromPic(IFormFile file, int ElderlyID)
        {
            try
            {

                var extractedText = ExtractTextFromImage(file);
                /*
                                var image = await CloudinaryHelper.UploadImageAsync(file);
                                var newImage = new Prescription
                                {
                                    Elderly = ElderlyID,
                                    CreatedAt = System.DateTime.UtcNow.AddHours(7),
                                    Status = SD.GeneralStatus.ACTIVE,
                                     Url = image.Url,
                                     Treatment = ParseDiagnosis(extractedText)
                                };

                                var rsImage = await _unitOfWork.PrescriptionRepository.CreateAsync(newImage);*/

                List<string> medicines = ParseMedicineDetails(extractedText);

                var listMediFromPic = CreateMedicationRequests(medicines);

                var medications = new List<GetMedicationFromScanDTO>();

                foreach (var medicine in listMediFromPic)
                {
                    DetermineFrequencyType(medicine);
                    var rs = new GetMedicationFromScanDTO
                    {
                        Dosage = (medicine.Dosage == "I Viên") ? "1 Viên" : medicine.Dosage,
                        CreatedDate = System.DateTime.UtcNow.AddHours(7),
                        DateFrequency = medicine.DateFrequency,
                        EndDate = DateOnly.FromDateTime(medicine.EndDate ?? System.DateTime.MinValue),
                        FrequencyType = medicine.FrequencyType,
                        TimeFrequency = medicine.TimeFrequency,
                        StartDate = DateOnly.FromDateTime(medicine.StartDate ?? System.DateTime.MinValue),
                        Shape = medicine.Shape,
                        Status = SD.GeneralStatus.ACTIVE,
                        MedicationName = medicine.MedicationName,
                        Remaining = medicine.Quantity

                    };
                    /*  var rs1 =await _unitOfWork.MedicationRepository.CreateAsync(rs);


                      var newSchedule = new MedicationSchedule
                      {
                          MedicationId = rs.MedicationId,
                          Dosage = (medicine.Dosage == "I Viên") ? "1 Viên" : medicine.Dosage,
                         DateTaken = (rs.TimeFrequency == "Sáng") ? new TimeOnly(7, 0) :
                                      (rs.TimeFrequency == "Trưa") ? new TimeOnly(11, 0) :
                                      (rs.TimeFrequency == "Chiều") ? new TimeOnly(17, 0) :
                                      (rs.TimeFrequency == "Tối") ? new TimeOnly(20, 0) : TimeOnly.MinValue,



                          Status = SD.GeneralStatus.ACTIVE
                      };

                      var rs2 = await _unitOfWork.MedicationScheduleRepository.CreateAsync(newSchedule);*/

                    medications.Add(rs);
                }

                var result = new GetPresciptionFromScan
                {
                    Treatment = ParseDiagnosis(extractedText),
                    Medication = medications,
                    tmp = extractedText
                };

                return new BusinessResult(Const.SUCCESS_CREATE, "Medication scan successfully.", result);


            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, ex.Message);
            }
        }

        public static string ExtractTextFromImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return "Invalid file";

            string tempFilePath = Path.GetTempFileName(); // Temporary file path

            try
            {
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                var credentialPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");

                using (var engine = new TesseractEngine(credentialPath, "vie", EngineMode.Default))
                {
                    using (var img = Pix.LoadFromFile(tempFilePath))
                    {
                        using (var page = engine.Process(img))
                        {
                            return page.GetText().Trim();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }
        static string ParseDiagnosis(string text)
        {
            string pattern = @"(Chuẩn đoán|Chvẩn đoán):.*?(?=\n\d+\.\.|$)";
            Match match = Regex.Match(text, pattern, RegexOptions.Singleline);

            if (match.Success)
            {
                string result = match.Value.Split(new[] { ':' }, 2).Last().Trim();
                return result;
            }

            return "Diagnosis not found.";
        }
        static List<string> ParseMedicineDetails(string text)
        {
            string corrected = Regex.Replace(text, @"\bUông\b", "Uống");
            string textFormat = corrected.Replace("I", "1");

            string pattern = @"(\d+\.\.\s.*?Uống\s*:\s*.*?)(?=\n\s*\d+\.\.|$)"; MatchCollection matches = Regex.Matches(textFormat, pattern, RegexOptions.Singleline);

            List<string> medications = new List<string>();

            foreach (Match match in matches)
            {
                medications.Add(match.Value.Trim());
            }

            string formattedText = string.Join(Environment.NewLine, medications).Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine).Trim();


            string[] lines = formattedText.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            bool inMedicationSection = true;
            List<string> medication2s = new List<string>();

            foreach (string line in lines)
            {
                if (line.StartsWith("-L") || line.StartsWith("Ngày") || line.StartsWith("Bác s?") || line.StartsWith("-Khám")/* || line.StartsWith("- Tên")*/)
                {
                    inMedicationSection = false;
                    continue;
                }

                if (line.StartsWith("H"))
                {
                    medication2s.Add(line.Trim());
                }
                if (inMedicationSection)
                {
                    medication2s.Add(line.Trim());
                }
            }

            return medication2s;


        }

        public void DetermineFrequencyType(NewMedicationFromPicDTO medication)
        {
            if (medication.Instruction.Contains("Uống : Sáng") || medication.Instruction.Contains("Uống : Chiều") || medication.Instruction.Contains("Uống : Tối"))
            {
                medication.FrequencyType = "Daily";
            }
            else if (medication.Instruction.Contains("Uống: Cách ngày"))
            {
                medication.FrequencyType = "Every X days"; // Uống cách ngày
            }
            else if (medication.Instruction.Contains("Uống: Mỗi tuần"))
            {
                medication.FrequencyType = "Weekly"; // Uống hàng tuần
            }
            else
            {
                medication.FrequencyType = "Unknown"; // Không xác định
            }
        }
        static List<NewMedicationFromPicDTO> CreateMedicationRequests(List<string> medicines)
        {
            List<NewMedicationFromPicDTO> requests = new List<NewMedicationFromPicDTO>();
            int daysToAdd = ExtractDays(medicines);
            string name = "Unknow";
            string quantity = "0";
            foreach (var medicine in medicines)
            {
                if (char.IsDigit(medicine[0]))
                {
                    var match = Regex.Match(medicine, @"^(\d+\.\.\s+|\d+\.\s+)?(.+?)\s+(\d+mg)?.*SL:\s*(\d+) Viên", RegexOptions.IgnoreCase);
                    if (!match.Success) return null;

                    name = match.Groups[2].Value.Trim();
                    quantity = match.Groups[4].Value;

                }

                else if (Regex.IsMatch(medicine, @"U[oôố]ng\s*[:\-]?\s*(Sáng|Chiều|Tối)\s*([1I\d]+)\s*Viên", RegexOptions.IgnoreCase))
                {
                    var doseMatch = Regex.Match(medicine, @"U[ôốo]ng\s*[:\-]?\s*(Sáng|Chiều|Tối)\s*([1I\d]+)\s*Viên", RegexOptions.IgnoreCase);
                    if (doseMatch.Success)
                    {
                        string timeToTake = doseMatch.Groups[1].Value;
                        string dosage = doseMatch.Groups[2].Value + " Viên";
                        var rs = new NewMedicationFromPicDTO
                        {
                            Shape = "Viên",
                            StartDate = System.DateTime.UtcNow.AddHours(7),
                            EndDate = System.DateTime.UtcNow.AddHours(7).AddDays(double.Parse(quantity)),
                            Dosage = dosage,
                            MedicationName = name,
                            TimeFrequency = timeToTake,
                            DateFrequency = 1,
                            Quantity = int.Parse(quantity),
                            Instruction = medicine
                        };
                        requests.Add(rs);
                    }
                }
            }

            return requests;
        }

        static int ExtractDays(List<string> lines)
        {
            foreach (var line in lines)
            {
                var match = Regex.Match(line, @"(\d+)\s*ngày", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return int.Parse(match.Groups[1].Value);
                }
            }
            return 30;
        }


        public async Task<int> GenerateMedicationSchedules(Medication medication, List<string> scheduleTimes, List<string> frequencySelect = null)
        {
            if (medication.StartDate == null || medication.EndDate == null || scheduleTimes == null || !scheduleTimes.Any())
            {
                throw new InvalidOperationException("StartDate, EndDate, and scheduleTimes must be set and non-empty.");
            }

            var currentDate = medication.StartDate.Value;
            var endDate = medication.EndDate.Value;
            var rs = 0;

            while (currentDate <= endDate)
            {
                // Check if the current date matches the selected days (if FrequencyType is "Select")
                if (medication.FrequencyType == "Select" && frequencySelect != null && frequencySelect.Any())
                {
                    var dayOfWeek = GetVietnameseDayOfWeek(currentDate.DayOfWeek);
                    if (!frequencySelect.Contains(dayOfWeek))
                    {
                        currentDate = currentDate.AddDays(1); // Skip to the next day
                        continue;
                    }
                }

                foreach (var time in scheduleTimes)
                {
                    // Parse the time string (e.g., "8:00") into a TimeSpan
                    var timeOfDay = TimeSpan.Parse(time);

                    // Create a new MedicationSchedule for the current date and time
                    var schedule = new MedicationSchedule
                    {
                        MedicationId = medication.MedicationId,
                        Dosage = medication.Dosage,
                        Status = "Unused",
                        DateTaken = new System.DateTime(currentDate.Year, currentDate.Month, currentDate.Day)
                                    .Add(timeOfDay), // Combine date and time
                        IsTaken = false
                    };

                    // Save the schedule to the database
                    var result = await _unitOfWork.MedicationScheduleRepository.CreateAsync(schedule);
                    if (result > 0)
                    {
                        rs++; // Increment the success count
                    }
                }
                if (medication.FrequencyType.StartsWith("Every ") && medication.FrequencyType.EndsWith(" day"))
                {
                    string numberStr = medication.FrequencyType.Replace("Every ", "").Replace(" day", "").Trim();
                    if (int.TryParse(numberStr, out int day))
                    {
                        currentDate = currentDate.AddDays(day); // Move by X days
                    }
                }
                else if (medication.FrequencyType == "Select")
                {
                    currentDate = currentDate.AddDays(1); 
                }
                else
                {
                    throw new InvalidOperationException("Invalid FrequencyType or missing DateFrequency.");
                }
            }

            return rs; // Return the total number of schedules created
        }

        private string GetVietnameseDayOfWeek(System.DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case System.DayOfWeek.Monday:
                    return "Thứ 2";
                case System.DayOfWeek.Tuesday:
                    return "Thứ 3";
                case System.DayOfWeek.Wednesday:
                    return "Thứ 4";
                case System.DayOfWeek.Thursday:
                    return "Thứ 5";
                case System.DayOfWeek.Friday:
                    return "Thứ 6";
                case System.DayOfWeek.Saturday:
                    return "Thứ 7";
                case System.DayOfWeek.Sunday:
                    return "Chủ nhật";
                default:
                    throw new ArgumentOutOfRangeException(nameof(dayOfWeek), dayOfWeek, null);
            }
        }
        public async Task<IBusinessResult> CreateMedicationByManually(CreateMedicationRequest req)
        {
            try
            {
                var checkElderly = await _unitOfWork.ElderlyRepository.GetByIdAsync(req.ElderlyId);
                if (checkElderly == null)
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Elderly not existed.");
                }

                var newPrescription = new Prescription
                {
                    Elderly = checkElderly.ElderlyId,
                    CreatedAt = System.DateTime.UtcNow.AddHours(7),
                    Status = SD.GeneralStatus.ACTIVE,
                    Url = "Manually",
                    Treatment = req.Treatment
                };

                var rsImage = await _unitOfWork.PrescriptionRepository.CreateAsync(newPrescription);

                foreach (var medication in req.Medication)
                {
                 

                    var rs = new Medication
                    {
                        Dosage = medication.Dosage,
                        CreatedDate = System.DateTime.UtcNow.AddHours(7),
                        EndDate = medication.EndDate,
                        FrequencyType = medication.FrequencyType,
                        StartDate = medication.StartDate,
                        Shape = medication.Shape,
                        Status = SD.GeneralStatus.ACTIVE,
                        MedicationName = medication.MedicationName,
                        Remaining = medication.Remaining,
                        PrescriptionId = newPrescription.PrescriptionId,
                        ElderlyId = req.ElderlyId
                    };

                    var createMedication = await _unitOfWork.MedicationRepository.CreateAsync(rs);
                    if (createMedication < 1)
                    {
                        return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Cannot create medication.");
                    }

                    var createSchedule = await GenerateMedicationSchedules(rs, medication.Schedule,medication.FrequencySelect);
                    if (createSchedule < 1)
                    {
                        return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Cannot create medication schedule.");
                    }
                }

                return new BusinessResult(Const.SUCCESS_CREATE, "Medication created successfully.", req);
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

        /*public async Task<IBusinessResult> GetAllMedicationsInPrescription(int elderlyId)
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
        }*/

        public static System.DateTime? ConvertToDateTime(DateOnly? dateOnly)
        {
            return dateOnly?.ToDateTime(new TimeOnly(0, 0));
        }

        public async Task<IBusinessResult> GetMedicationsForToday(int elderlyId, System.DateTime today)
        {
            try
            {
                var prescription = await _unitOfWork.PrescriptionRepository
                    .GetAllIncludeMedicationInElderly(elderlyId);

                if (prescription == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "No prescription found for the given elderly ID.");
                }

                var medicationDtos = new List<MedicationModel>();

                foreach (var medication in prescription.Medications
                    .Where(m => ConvertToDateTime(m.StartDate) <= today &&
                                 (m.EndDate == null || ConvertToDateTime(m.EndDate) >= today)))
                {
                   
                    var medicationSchedule = _unitOfWork.MedicationScheduleRepository.FindByCondition(ms => ms.MedicationId == medication.MedicationId).FirstOrDefault();
                  
                    var medicationDto = new MedicationModel
                    {
                        Id = medication.MedicationId,
                        Name = medication.MedicationName,
                        Dosage = medication.Dosage,
                        Form = medication.Shape,
                        Remaining = medication.Remaining.ToString(),
                        TypeFrequency = medication.FrequencyType,
                        FrequencySelect = GetWeeklyMedicationScheduleForMedication(medication.MedicationId, today),
                        MealTime = medication.IsBeforeMeal == true ? "Trước ăn" : "Sau ăn",
                       Schedule = medication.MedicationSchedules
                            .Select(ms => new ScheduleModel
                            {
                                Time = ms.DateTaken.HasValue ? ms.DateTaken.Value.ToString("h:mm:ss") : null,
                                Status = "unUsed"
                            }).ToList()
                    };

                    medicationDtos.Add(medicationDto);
                }

                var result = new
                {
                    Id = elderlyId,
                    Treatment = "viêm họng",
                    StartDate = today.ToString("yyyy-MM-dd"),
                    Medicines = medicationDtos
                };

                return new BusinessResult(Const.SUCCESS_READ, "Medications retrieved successfully.", result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, ex.Message);
            }
        }

        private List<string> GetWeeklyMedicationScheduleForMedication(int medicationId, System.DateTime today)
        {
            var dayOfWeek = (int)today.DayOfWeek;
            var mondayOfWeek = today.AddDays(-((dayOfWeek == 0 ? 7 : dayOfWeek) - 1)).Date;
            var sundayOfWeek = mondayOfWeek.AddDays(6);

            var schedules =  _unitOfWork.MedicationScheduleRepository
                .FindByCondition(ms => ms.MedicationId == medicationId &&
                                       ms.DateTaken >= mondayOfWeek &&
                                       ms.DateTaken <= sundayOfWeek)
                .ToList();

            if (!schedules.Any())
                return new List<string>();

            var daysTaken = schedules
                .Select(s => s.DateTaken?.ToString("dddd"))
                .Distinct()
                .ToList();

            return daysTaken;
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

