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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Tesseract;
using static SE.Common.DTO.GetPresciptionFromScan;
using static System.Net.Mime.MediaTypeNames;
using Google.Cloud.Vision.V1;
using Google.Apis.Auth.OAuth2;



namespace SE.Service.Services
{
    public interface IMedicationService
    {
        Task<IBusinessResult> ScanFromPic(IFormFile file, int accountId);
       Task<IBusinessResult> GetMedicationsForToday(int accountId, System.DateOnly today);

        Task<IBusinessResult> UpdateMedicationInPrescription(int prescriptionId, UpdateMedicationInPrescriptionRequest req);

        Task<IBusinessResult> CreateMedicationByManually(CreateMedicationRequest req);

        Task<IBusinessResult> ConfirmMedicationDrinking(ConfirmMedicationDrinkingReq request);
        Task<IBusinessResult> CancelPrescription(int prescriptionId);
        Task<IBusinessResult> GetPrescriptionOfElderly(int accountId);
        Task<IBusinessResult> ScanByGoogle(IFormFile file);


    }

    public class MedicationService : IMedicationService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly GoogleCredential _googleCredential;

        public MedicationService(UnitOfWork unitOfWork, IMapper mapper, GoogleCredential googleCredential)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _googleCredential = googleCredential;
        }



        public async Task<IBusinessResult> ScanByGoogle(IFormFile file)
        {
            // Create the scoped credential using the injected GoogleCredential
            var scopedCredential = _googleCredential.CreateScoped(ImageAnnotatorClient.DefaultScopes);

            // Create the ImageAnnotatorClient using the Builder pattern
            var client = await new ImageAnnotatorClientBuilder
            {
                Credential = scopedCredential
            }.BuildAsync(); // Use BuildAsync() for async

            // Check if the file is not null and has content
            if (file == null || file.Length == 0)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, "No file uploaded", null);
            }

            // Convert the uploaded file to a byte array
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            // Load the image from the byte array
            var image = Google.Cloud.Vision.V1.Image.FromBytes(memoryStream.ToArray());

            // Call the API to detect text
            var response = client.DetectText(image);

            // Create a List to store the unique text
            List<string> uniqueTextList = new List<string>();

            // Use a HashSet to track already added lines to avoid duplicates
            var uniqueLines = new HashSet<string>();

            // StringBuilder to concatenate detected words into sentences
            StringBuilder sb = new StringBuilder();

            // Iterate over each text annotation in the response
            foreach (var annotation in response)
            {
                // Avoid adding duplicates using HashSet
                if (!uniqueLines.Contains(annotation.Description))
                {
                    uniqueLines.Add(annotation.Description);

                    // Append detected text to StringBuilder (concatenating adjacent words into sentences)
                    sb.Append(annotation.Description.Trim() + " ");
                }
            }

            // Split concatenated text into individual lines
            var resultText = sb.ToString().Trim();
            var lines = resultText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            // Add the lines to the List
            foreach (var line in lines)
            {
                uniqueTextList.Add(line);
            }

            // Output the detected text saved in the List
            Console.WriteLine("Detected unique text:");
            foreach (var text in uniqueTextList)
            {
                Console.WriteLine(text);
            }

            return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, uniqueTextList);
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
                return new BusinessResult(Const.FAIL_CREATE, ex.InnerException.Message);
            }
        }

        public static string ExtractTextFromImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return "Invalid file";

            string tempFilePath = Path.GetTempFileName();

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
            catch (FileNotFoundException ex)
            {
                return $"Error: File not found - {ex.Message} - Inner Exception: {ex.InnerException.Message}";
            }
            catch (DirectoryNotFoundException ex)
            {
                return $"Error: Directory not found - {ex.Message} - Inner Exception: {ex.InnerException.Message}";
            }
            catch (UnauthorizedAccessException ex)
            {
                return $"Error: Unauthorized access - {ex.Message} - Inner Exception: {ex.InnerException.Message}";
            }
            catch (IOException ex)
            {
                return $"Error: I/O error - {ex.Message} - Inner Exception: {ex.InnerException.Message}";
            }
            catch (TesseractException ex)
            {
                return $"Error: Tesseract processing error - {ex.Message} - Inner Exception: {ex.InnerException.Message}";
            }
            catch (Exception ex)
            {
                return $"Error: An unexpected error occurred - {ex.Message} - Inner Exception: {ex.InnerException.Message}";
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
        public async Task<IBusinessResult> CreateMedicationByManually(CreateMedicationRequest req)
        {
            try
            {
                var checkAccount = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(req.AccountId);
                var checkElderly = await _unitOfWork.ElderlyRepository.GetByIdAsync(checkAccount.Elderly.ElderlyId);
                if (checkElderly == null)
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Elderly not existed.");
                }

                var newPrescription = new Prescription
                {
                    Elderly = checkElderly.ElderlyId,
                    CreatedAt = DateTime.UtcNow.AddHours(7),
                    Status = SD.GeneralStatus.ACTIVE,
                    CreatedBy = req.CreatedBy,
                    Url = "Manually",
                    Treatment = req.Treatment,
                    EndDate = req.EndDate
                };

                var rsImage = await _unitOfWork.PrescriptionRepository.CreateAsync(newPrescription);

                foreach (var medication in req.Medication)
                {
                    var rs = new Medication
                    {
                        Dosage = medication.Dosage,
                        CreatedDate = DateTime.UtcNow.AddHours(7),
                        EndDate = null, // EndDate sẽ được tính toán sau
                        FrequencyType = medication.FrequencyType,
                        StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7)), // StartDate là ngày tạo, kiểu DateOnly
                        Shape = medication.Shape,
                        Status = SD.GeneralStatus.ACTIVE,
                        MedicationName = medication.MedicationName,
                        Remaining = medication.Remaining,
                        PrescriptionId = newPrescription.PrescriptionId,
                        ElderlyId = checkElderly.ElderlyId,
                        Note = medication.Note,
                        IsBeforeMeal = medication.IsBeforeMeal,
                        Treatment = medication.Treatment
                    };

                    var createMedication = await _unitOfWork.MedicationRepository.CreateAsync(rs);
                    if (createMedication < 1)
                    {
                        return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Cannot create medication.");
                    }

                    var createSchedule = await GenerateMedicationSchedules(rs, medication.Schedule, medication.FrequencySelect);
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

        public async Task<int> GenerateMedicationSchedules(Medication medication, List<string> scheduleTimes, List<string> frequencySelect = null)
        {
            if (scheduleTimes == null || !scheduleTimes.Any())
            {
                throw new InvalidOperationException("scheduleTimes must be set and non-empty.");
            }

            var currentDate = ConvertToDateTime(medication.StartDate).Value;
            var remaining = medication.Remaining;
            var rs = 0;

            if (!int.TryParse(medication.Dosage.Split(' ')[0], out int dosage))
            {
                throw new InvalidOperationException("Invalid Dosage format. Expected format like '1 Viên'.");
            }

            while (remaining > 0)
            {
                if (medication.FrequencyType == "Select" && frequencySelect != null && frequencySelect.Any())
                {
                    var dayOfWeek = GetVietnameseDayOfWeek(currentDate.DayOfWeek);
                    if (!frequencySelect.Contains(dayOfWeek))
                    {
                        currentDate = currentDate.AddDays(1);
                        continue;
                    }
                }

                foreach (var time in scheduleTimes)
                {
                    if (remaining <= 0)
                    {
                        break;
                    }

                    var timeOfDay = TimeSpan.Parse(time);

                    var schedule = new MedicationSchedule
                    {
                        MedicationId = medication.MedicationId,
                        Dosage = medication.Dosage,
                        Status = "Unused",
                        DateTaken = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day)
                                    .Add(timeOfDay),
                        IsTaken = false
                    };
                    var result = await _unitOfWork.MedicationScheduleRepository.CreateAsync(schedule);
                    if (result > 0)
                    {
                        rs++;
                        remaining -= dosage;
                    }
                }

                if (medication.FrequencyType.StartsWith("Every ") && medication.FrequencyType.EndsWith(" day"))
                {
                    string numberStr = medication.FrequencyType.Replace("Every ", "").Replace(" day", "").Trim();
                    if (int.TryParse(numberStr, out int day))
                    {
                        currentDate = currentDate.AddDays(day);
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

            // Cập nhật EndDate của medication dựa trên currentDate
            medication.EndDate = DateOnly.FromDateTime(currentDate);
            await _unitOfWork.MedicationRepository.UpdateAsync(medication);

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
   
        public static System.DateTime? ConvertToDateTime(DateOnly? dateOnly)
        {
            return dateOnly?.ToDateTime(new TimeOnly(0, 0));
        }

        public async Task<IBusinessResult> GetMedicationsForToday(int accountId, DateOnly today)
        {
            try
            {
                
                var checkAccount = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(accountId);
                var prescription = await _unitOfWork.PrescriptionRepository
                    .GetAllIncludeMedicationInElderly(checkAccount.Elderly.ElderlyId);

                if (prescription == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "No prescription found for the given elderly ID.");
                }

                var medicationDtos = new List<MedicationModel>();

                foreach (var medication in prescription.Medications
                    .Where(m => m.StartDate <= today &&
                                 (m.EndDate == null ||m.EndDate >= today)))
                {
                    string frequencyEvery = null;
                    var medicationSchedule = _unitOfWork.MedicationScheduleRepository.FindByCondition(ms => ms.MedicationId == medication.MedicationId).FirstOrDefault();
                    if (medication.FrequencyType.StartsWith("Every ") && medication.FrequencyType.EndsWith(" day"))
                    {
                        string numberStr = medication.FrequencyType.Replace("Every ", "").Replace(" day", "").Trim();
                        if (int.TryParse(numberStr, out int day))
                        {
                            frequencyEvery = day.ToString();
                        }
                    }
                    var medicationDto = new MedicationModel
                    {
                        MedicationId = medication.MedicationId,
                        MedicationName = medication.MedicationName,
                        Dosage = medication.Dosage,
                        Shape = medication.Shape,
                        Remaining = medication.Remaining,
                        FrequencyType = medication.FrequencyType,
                        FrequencySelect = GetWeeklyMedicationScheduleForMedication(medication.MedicationId, today),
                        IsBeforeMeal = medication.IsBeforeMeal,
                        Schedule = medication.MedicationSchedules
                            .Where(ms => ms.DateTaken?.ToString("yyyy-MM-dd") == today.ToString("yyyy-MM-dd"))
                            .Select(ms => new ScheduleModel
                            {
                                Time = ms.DateTaken.HasValue ? ms.DateTaken.Value.ToString("HH:mm:ss") : null,
                                Status = ms.Status
                            }).ToList()
                    };

                    medicationDtos.Add(medicationDto);
                }

                var result = new
                {
                    Id = prescription.PrescriptionId,
                    Treatment = prescription.Treatment,
                    EndDate = prescription.EndDate?.ToString("yyyy-MM-dd"),
                    StartDate = prescription.CreatedAt.ToString("yyyy-MM-dd"),
                    Medicines = medicationDtos
                };

                return new BusinessResult(Const.SUCCESS_READ, "Medications retrieved successfully.", result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, ex.Message);
            }
        }

        private List<string> GetWeeklyMedicationScheduleForMedication(int medicationId, System.DateOnly today)
        {
            var dayOfWeek = (int)today.DayOfWeek;
            var mondayOfWeek = today.AddDays(-((dayOfWeek == 0 ? 7 : dayOfWeek) - 1));
            var sundayOfWeek = mondayOfWeek.AddDays(6);

            var schedules =  _unitOfWork.MedicationScheduleRepository
                .FindByCondition(ms => ms.MedicationId == medicationId &&
                                       ms.DateTaken >= ConvertToDateTime(mondayOfWeek) &&
                                       ms.DateTaken <= ConvertToDateTime(sundayOfWeek))
                .ToList();

            if (!schedules.Any())
                return new List<string>();

            var daysTaken = schedules
                .Select(s => s.DateTaken?.ToString("dddd"))
                .Distinct()
                .ToList();

            return daysTaken;
        }
        public async Task<IBusinessResult> CancelPrescription(int prescriptionId)
        {
            try
            {
                var checkPrescription = _unitOfWork.PrescriptionRepository.GetById(prescriptionId);
                if (checkPrescription == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "Prescription ID does not existed.");
                }
                checkPrescription.Status = SD.GeneralStatus.INACTIVE;
                var rs = await _unitOfWork.PrescriptionRepository.UpdateAsync(checkPrescription);
                if (rs > 0)
                {
                    return new BusinessResult(Const.SUCCESS_UNACTIVATE, Const.SUCCESS_UNACTIVATE_MSG);
                }
                return new BusinessResult(Const.FAIL_UNACTIVATE, Const.FAIL_UNACTIVATE_MSG);

            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, ex.Message);
            }
        }

        public async Task<IBusinessResult> UpdateMedicationInPrescription(int prescriptionId, UpdateMedicationInPrescriptionRequest req)
        {
            try
            {
                var existingPrescription = _unitOfWork.PrescriptionRepository.FindByCondition(p=>p.PrescriptionId == prescriptionId && p.Status == "Active").FirstOrDefault();
                if (existingPrescription == null)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG, "Prescription not found.");
                }
                var today = DateTime.Now;
                existingPrescription.Treatment = req.Treatment;
                var updatePrescriptionResult = await _unitOfWork.PrescriptionRepository.UpdateAsync(existingPrescription);
                if (updatePrescriptionResult < 1)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG, "Failed to update prescription.");
                }
                var existingMedications = await _unitOfWork.MedicationRepository.GetByPrescriptionIdAsync(prescriptionId);

                foreach (var medicationReq in req.Medication)
                {
                    var existingMedication = new Medication();
                    if (medicationReq.MedicationId == null)
                    {
                        existingMedication = null;
                    }
                    else
                    {
                        existingMedication = existingMedications.FirstOrDefault(m => medicationReq.MedicationId != null && m.MedicationId == medicationReq.MedicationId && m.PrescriptionId == prescriptionId && m.Status == "Active");
                    }
                    if (existingMedication != null)
                    {
                        existingMedication.Treatment = medicationReq.Treatment;
                        existingMedication.MedicationName = medicationReq.MedicationName;
                        existingMedication.Dosage = medicationReq.Dosage;
                        existingMedication.FrequencyType = medicationReq.FrequencyType;
                        existingMedication.Shape = medicationReq.Shape;
                        existingMedication.Remaining = medicationReq.Remaining;
                        existingMedication.Note = medicationReq.Note;
                        existingMedication.IsBeforeMeal = medicationReq.IsBeforeMeal;

                        var updateMedicationResult = await _unitOfWork.MedicationRepository.UpdateAsync(existingMedication);
                        if (updateMedicationResult < 1)
                        {
                            return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG, "Failed to update medication.");
                        }

                        var deleteSchedulesResult = await _unitOfWork.MedicationScheduleRepository.DeleteByMedicationIdAsync(existingMedication.MedicationId, today);
                        if (deleteSchedulesResult < 0)
                        {
                            return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG, "Failed to delete existing schedules.");
                        }

                        var createScheduleResult = await GenerateMedicationSchedules(existingMedication, medicationReq.Schedule, medicationReq.FrequencySelect);
                        if (createScheduleResult < 1)
                        {
                            return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG, "Failed to create new schedules.");
                        }
                    }
                    else
                    {
                        var newMedication = new Medication
                        {
                            Dosage = medicationReq.Dosage,
                            CreatedDate = DateTime.UtcNow.AddHours(7),
                            EndDate = null,
                            FrequencyType = medicationReq.FrequencyType,
                            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7)),
                            Shape = medicationReq.Shape,
                            Status = SD.GeneralStatus.ACTIVE,
                            MedicationName = medicationReq.MedicationName,
                            Remaining = medicationReq.Remaining,
                            PrescriptionId = prescriptionId,
                            ElderlyId = existingPrescription.Elderly,
                            Note = medicationReq.Note,
                            IsBeforeMeal = medicationReq.IsBeforeMeal,
                            Treatment = medicationReq.Treatment
                        };

                        var createMedicationResult = await _unitOfWork.MedicationRepository.CreateAsync(newMedication);
                        if (createMedicationResult < 1)
                        {
                            return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG, "Failed to create new medication.");
                        }

                        var createScheduleResult = await GenerateMedicationSchedules(newMedication, medicationReq.Schedule, medicationReq.FrequencySelect);
                        if (createScheduleResult < 1)
                        {
                            return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG, "Failed to create schedules for new medication.");
                        }
                    }
                }

                return new BusinessResult(Const.SUCCESS_UPDATE, "Medication updated successfully.", req);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, ex.Message);
            }
        }
        public async Task<IBusinessResult> ConfirmMedicationDrinking(ConfirmMedicationDrinkingReq request)
        {
            try
            {
                int updatedCount = 0;

                foreach (var confirmation in request.Confirmations)
                {
                    if (!DateTime.TryParseExact(confirmation.DateTaken, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTaken))
                    {
                        return new BusinessResult(Const.FAIL_UPDATE, $"Invalid date format: {confirmation.DateTaken}");
                    }
                    var medication =await _unitOfWork.MedicationRepository.GetByIdAsync(confirmation.MedicationId);
                    var schedule = await _unitOfWork.MedicationScheduleRepository
                        .GetByDateAndMedicationIdAsync(dateTaken, confirmation.MedicationId);

                    if (schedule != null)
                    {
                        schedule.IsTaken = true;
                        schedule.Status = confirmation.Status;
                        
                        var updateResult = await _unitOfWork.MedicationScheduleRepository.UpdateAsync(schedule);

                        if (!int.TryParse(medication.Dosage.Split(' ')[0], out int dosage))
                        {
                            throw new InvalidOperationException("Invalid Dosage format. Expected format like '1 Viên'.");
                        }

                        medication.Remaining = medication.Remaining - dosage;

                        await _unitOfWork.MedicationRepository.UpdateAsync(medication);
                        if (updateResult > 0)
                        {
                            updatedCount++;
                        }
                    }
                }

                if (updatedCount == request.Confirmations.Count)
                {
                    return new BusinessResult(Const.SUCCESS_UPDATE, "All medication schedules confirmed successfully.");
                }
                else
                {
                    return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                }
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, ex.Message);
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

        public async Task<IBusinessResult> GetPrescriptionOfElderly(int accountId)
        {
            try
            {
                var checkAccount = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(accountId);

                var prescription = await _unitOfWork.PrescriptionRepository
                    .GetAllIncludeMedicationInElderly(checkAccount.Elderly.ElderlyId);

                if (prescription == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "No prescription found for the given elderly ID.");
                }

                var medicationDtos = new List<UpdateMedicationModel>();
                var today = DateOnly.FromDateTime(System.DateTime.UtcNow.AddHours(7));

                foreach (var medication in prescription.Medications)
                {
                    string frequencyEvery = null;
                    var medicationSchedule = _unitOfWork.MedicationScheduleRepository.FindByCondition(ms => ms.MedicationId == medication.MedicationId).FirstOrDefault();
                    if (medication.FrequencyType.StartsWith("Every ") && medication.FrequencyType.EndsWith(" day"))
                    {
                        string numberStr = medication.FrequencyType.Replace("Every ", "").Replace(" day", "").Trim();
                        if (int.TryParse(numberStr, out int day))
                        {
                            frequencyEvery = day.ToString();
                        }
                        
                    }
                    var date = medication.MedicationSchedules.FirstOrDefault();

                    var medicationDto = new UpdateMedicationModel
                    {
                        MedicationId = medication.MedicationId,
                        MedicationName = medication.MedicationName,
                        Dosage = medication.Dosage,
                        Shape = medication.Shape,
                        Remaining = medication.Remaining,
                        FrequencyType = medication.FrequencyType,
                        FrequencySelect = GetWeeklyMedicationScheduleForMedication(medication.MedicationId, today),
                        IsBeforeMeal = medication.IsBeforeMeal,
                        Schedule = medication.MedicationSchedules
                                      .Where(ms => ms.DateTaken?.ToString("yyyy-MM-dd") == today.ToString("yyyy-MM-dd"))
                                      .Select(ms => ms.DateTaken.HasValue ? ms.DateTaken.Value.ToString("HH:mm:ss") : null)
                                      .ToList()
                    };

                    medicationDtos.Add(medicationDto);
                }

                var result = new
                {
                    Id = prescription.PrescriptionId,
                    Treatment = prescription.Treatment,
                    EndDate = prescription.EndDate?.ToString("yyyy-MM-dd"),
                    StartDate = prescription.CreatedAt.ToString("yyyy-MM-dd"),
                    Medicines = medicationDtos
                };

                return new BusinessResult(Const.SUCCESS_READ, "Medications retrieved successfully.", result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, ex.Message);
            }
        }
    }
}

