﻿using AutoMapper;
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
using static SE.Common.DTO.GetPresciptionFromScan;
using static System.Net.Mime.MediaTypeNames;
using Google.Cloud.Vision.V1;
using Google.Apis.Auth.OAuth2;
using static Google.Cloud.Vision.V1.ProductSearchResults.Types;
using Google.Api.Gax;



namespace SE.Service.Services
{
    public interface IMedicationService
    {
        Task<IBusinessResult> GetMedicationsForToday(int accountId, System.DateOnly today);

        Task<IBusinessResult> UpdateMedicationInPrescription(int prescriptionId, UpdateMedicationInPrescriptionRequest req);

        Task<IBusinessResult> CreateMedicationByManually(CreateMedicationRequest req);

        Task<IBusinessResult> ConfirmMedicationDrinking(ConfirmMedicationDrinkingReq request);
        Task<IBusinessResult> CancelPrescription(int prescriptionId);
        Task<IBusinessResult> GetPrescriptionOfElderly(int accountId);
        Task<IBusinessResult> ScanByGoogle(IFormFile file);

        Task<IBusinessResult> GetUsedPrescriptionsOfElderly(int accountId);

    }

    public class MedicationService : IMedicationService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly GoogleCredential _googleCredential;
        private readonly INotificationService _notificationService;
        private readonly IGroupService _groupService;

        public MedicationService(UnitOfWork unitOfWork, IMapper mapper, GoogleCredential googleCredential, INotificationService notificationService, IGroupService groupService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _googleCredential = googleCredential;
            _notificationService = notificationService;
            _groupService = groupService;
        }


        public bool IsValidPrescriptionStructure(List<string> textLines)
        {
            // Hàm kiểm tra linh hoạt với sai sót OCR
                        bool FlexibleContains(string source, params string[] targets)
                        {
                            string normalizedSource = RemoveDiacritics(source.ToUpper());

                            foreach (string target in targets)
                            {
                                string normalizedTarget = RemoveDiacritics(target.ToUpper());

                                // Kiểm tra chứa toàn bộ hoặc 80% ký tự đầu
                                if (normalizedSource.Contains(normalizedTarget))
                                    return true;

                                if (normalizedTarget.Length > 5)
                                {
                                    string partialTarget = normalizedTarget.Substring(0, (int)(normalizedTarget.Length * 0.8));
                                    if (normalizedSource.Contains(partialTarget))
                                        return true;
                                }
                            }
                            return false;
                        } 

                        // Danh sách các yếu tố quan trọng
                        var importantElements = new List<bool>
                {
                    // 1. Thông tin bệnh viện/phòng khám
                    textLines.Any(line => FlexibleContains(line, "Bệnh viện", "BENH VIEN", "Phòng khám", "PHONG KHAM")),
        
                    // 2. Thông tin bệnh nhân
                    textLines.Any(line => FlexibleContains(line, "Họ tên:", "HO TEN:", "Địa chỉ:", "DIA CHI:")),
        
                    // 3. Chẩn đoán
                    textLines.Any(line => FlexibleContains(line, "Chẩn đoán", "CHAN DOAN", "Chuan doan")),

                            textLines.Any(line => FlexibleContains(line, "Bác sĩ", "Bac si","BAC SI", "BÁC SĨ")),

                            textLines.Any(line => FlexibleContains(line, "Tái khám", "TÁI KHÁM","Tai kham", "TAI KHAM")),


                    // 4. Thuốc và liều dùng
                    textLines.Any(line => Regex.IsMatch(line, @"^\d+\.")) ||
                    textLines.Any(line => FlexibleContains(line, "Uống:", "UONG:", "Sáng", "Chiều", "Tối", "SL:"))
                };

                        // Chỉ cần thỏa mãn 2/4 yếu tố là đủ
                        int matchedCount = importantElements.Count(x => x);
                    return matchedCount >= 2;
        }

        public string RemoveDiacritics( string text)
        {
            string normalized = text.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            foreach (char c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
        public async Task<IBusinessResult> ScanByGoogle(IFormFile file)
        {
            // Check if the file is not null and has content
            if (file == null || file.Length == 0)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, "Không nhận được hình ảnh", null);
            }

            // First check if the image is likely a prescription
            if (!await IsPrescriptionImage(file))
            {
                return new BusinessResult(0, "Đây không phải là ảnh toa thuốc", null);
            }

            // Create the scoped credential using the injected GoogleCredential
            var scopedCredential = _googleCredential.CreateScoped(ImageAnnotatorClient.DefaultScopes);

            // Create the ImageAnnotatorClient using the Builder pattern
            var client = await new ImageAnnotatorClientBuilder
            {
                Credential = scopedCredential
            }.BuildAsync();

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
                    sb.Append(annotation.Description.Trim() + " ");
                }
            }

            var resultText = sb.ToString().Trim();
            var lines = resultText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                uniqueTextList.Add(line);
            }

            // Check if the prescription has the expected structure
            if (!IsValidPrescriptionStructure(uniqueTextList))
            {
                return new BusinessResult(2, "Đây không phải là ảnh toa thuốc", null);
            }

            int numberDate = ExtractReExaminationDate(uniqueTextList);
            uniqueTextList.RemoveAt(uniqueTextList.Count - 1);

            Parallel.ForEach(uniqueTextList, (item, state, index) =>
            {
                uniqueTextList[(int)index] = item.Replace("Viện", "Viên");
            });

            var treatment = ParseDiagnosis(uniqueTextList);
            var groupData = GetData(GroupData(uniqueTextList));
            var medicationList = ConvertToMedicationModels(groupData);

            if (!medicationList.Any())
            {
                return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Không thể quét toa thuốc này!");
            }

            var result = new
            {
                Treatment = treatment,
                EndDate = DateTime.UtcNow.AddHours(7).AddDays(numberDate).ToString("yyyy-MM-dd"),
                Medicines = medicationList
            };

            return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
        }

        private async Task<bool> IsPrescriptionImage(IFormFile file)
        {
            // Simple check based on file content (you might want to enhance this)
            try
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();

                // Simple check for minimum file size (prescriptions usually have some text)
                if (imageBytes.Length < 1024) // Less than 1KB is probably not a prescription
                {
                    return false;
                }

                // You could add more sophisticated checks here if needed
                return true;
            }
            catch
            {
                return false;
            }
        }

        
        public List<List<string>> GroupData(List<string> data)
        {
            string diagnosis = string.Empty;
            var result = new ScanMediModel();
            List<List<string>> groupedMedications = new List<List<string>>();

            foreach (var line1 in data)
            {
                if (line1.Contains("Chẩn đoán"))
                {
                    diagnosis = line1.Split(':')[1].Trim();
                    result.Treatment = diagnosis;
                    break;
                }
            }

            List<string> currentMedicationGroup = new List<string>();
            bool isInMedicationGroup = false;

            for (int i = 0; i < data.Count; i++)
            {
                string line = data[i];

                // Kiểm tra nếu dòng bắt đầu với một số và dấu chấm (ví dụ: "1.", "2.", "3.")
                if (line.Length > 0 && Char.IsDigit(line[0]) && line.Contains("."))
                {
                    // Nếu đang trong một nhóm thuốc, kết thúc nhóm cũ và lưu vào danh sách
                    if (isInMedicationGroup)
                    {
                        groupedMedications.Add(new List<string>(currentMedicationGroup));
                    }

                    currentMedicationGroup.Clear();
                    currentMedicationGroup.Add(line);
                    isInMedicationGroup = true;
                }
                else if (isInMedicationGroup)
                {
                    // Thêm dòng vào nhóm thuốc hiện tại
                    currentMedicationGroup.Add(line);

                    // Kiểm tra nếu dòng tiếp theo bắt đầu với số tiếp theo, kết thúc nhóm thuốc hiện tại
                    if (i + 1 < data.Count && data[i + 1].Length > 0 && Char.IsDigit(data[i + 1][0]) && data[i + 1].Contains("."))
                    {
                        groupedMedications.Add(new List<string>(currentMedicationGroup));
                        currentMedicationGroup.Clear();
                        isInMedicationGroup = false;
                    }
                }
            }

            // Lưu nhóm thuốc cuối cùng nếu có
            if (currentMedicationGroup.Count > 0)
            {
                groupedMedications.Add(new List<string>(currentMedicationGroup));

            }

            return groupedMedications;
        }

        static string ParseDiagnosis(List<string> data)
        {
            foreach (var line1 in data)
            {
                if (line1.Contains("Chẩn đoán"))
                {
                    var diagnosis = line1.Split(':')[1].Trim();
                    return diagnosis;
                }
            }
            return "";
        }

        public ScanMediModel GetData(List<List<string>> data)
        {
            var rs = new ScanMediModel();
            var medications = new List<MediModel>();
            for (int i = 0; i < data.Count; i++)
            {
                var medi = new MediModel();
                for (int j = 0; j < data[i].Count; j++)
                {
                    if (data[i][j].Contains("SL"))
                    {
                        medi.Quantity = ExtractQuantity(data[i][j]);
                    }
                    else if (data[i][j].Contains("Sáng") || data[i][j].Contains("Tối") || data[i][j].Contains("Chiều"))
                    {
                        var timeDosageParts = data[i][j].Split(',');

                        foreach (var part in timeDosageParts)
                        {
                            string time = string.Empty;
                            if (part.Contains("Sáng")) time = "Sáng";
                            else if (part.Contains("Tối")) time = "Tối";
                            else if (part.Contains("Chiều")) time = "Chiều";

                            string dosage = ExtractDosage(part);

                            if (!string.IsNullOrEmpty(time))
                            {
                                if (string.IsNullOrEmpty(medi.Time))
                                {
                                    medi.Time = time;
                                }
                                else
                                {
                                    medi.Time += ", " + time;
                                }
                            }

                            if (!string.IsNullOrEmpty(dosage))
                            {
                                medi.Dosage = dosage;
                            }
                        }
                    }

                    medi.Name = RemoveNumberFromName(data[i][0]);

                }
                medications.Add(medi);
            }
            rs.mediModels = medications;
            return rs;
        }

        // Cắt bỏ số trong tên thuốc (ví dụ: "4. Paracetamol" -> "Paracetamol")
        static string RemoveNumberFromName(string name)
        {
            return Regex.Replace(name, @"^\d+\.\s*", "").Trim();
        }

        // Lấy số lượng thuốc từ chuỗi "SL: 10 Viên"
        static int ExtractQuantity(string line)
        {
            var match = Regex.Match(line, @"SL:\s(\d+)\sViên");
            return match.Success ? int.Parse(match.Groups[1].Value) : 0;
        }

        // Phân tách thời gian và liều lượng từ chuỗi "Tối 1 Viên"
        static string ExtractDosage(string line)
        {
            // Regex to find the number followed by "Viên" or "Viện"
            var match = Regex.Match(line, @"(\d+)\sVi[êeệ]n");  // Matches "Viên" or "Viện"
            if (match.Success)
            {
                return match.Groups[0].Value;  // Return the matched dosage (e.g., "1 Viên" or "1 Viện")
            }
            return string.Empty;  // Return empty if no match is found
        }


        static List<UpdateMedicationModel> ConvertToMedicationModels(ScanMediModel mediModels)
        {
            var rs = new List<UpdateMedicationModel>();
            var timeMapping = new Dictionary<string, string>
        {
            { "Sáng", "07:00" },
            { "Trưa", "11:00" },
            { "Chiều", "16:00" },
            { "Tối", "19:00" }
        };

            foreach (var model in mediModels.mediModels)
            {
                if (model.Name == null || model.Dosage == null || model.Time == null)
                {
                    return rs;

                }
                var temp = new UpdateMedicationModel();
                temp.MedicationName = model.Name;
                temp.Dosage = model.Dosage;
                temp.Remaining = model.Quantity;
                temp.Shape = ExtractUnit(model.Dosage);
                temp.FrequencyType = "Every 1 day";
                temp.IsBeforeMeal = true;
                temp.FrequencySelect = null;
                temp.Schedule = ((string)model.Time).Split(", ").Select(t => timeMapping[t]).ToList();
                rs.Add(temp);
            }

            return rs;

        }
        static int ExtractReExaminationDate(List<string> data)
        {
            foreach (string line in data)
            {
                if (line.Contains("Hẹn tái khám sau"))
                {
                    Match match = Regex.Match(line, @"Hẹn tái khám sau (\d+) ngày");
                    if (match.Success)
                    {
                        int soNgayTaiKham = int.Parse(match.Groups[1].Value);
                        return soNgayTaiKham;
                    }
                }
            }
            return 0;
        }

        static string ExtractUnit(string input)
        {
            if (input == null)
            {
                return "Không thể quét toa thuốc này!";
            }
            Match match = Regex.Match(input, @"^\d+\s+(.+)$");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }
        public async Task<IBusinessResult> CreateMedicationByManually(CreateMedicationRequest req)
        {
            try
            {
                var checkAccount = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(req.AccountId);
                var checkElderly = await _unitOfWork.ElderlyRepository.GetByIdAsync(checkAccount.Elderly.ElderlyId);
                if (checkElderly == null)
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "ID người già không tồn tại");
                }
                string imageUrl;
                if (req.MedicationImage != null)
                {
                    var imagePrescription = await CloudinaryHelper.UploadImageAsync(req.MedicationImage);
                    imageUrl = imagePrescription.Url;
                }
                else
                {
                    imageUrl = "Manually";
                }

                var newPrescription = new Prescription
                {
                    Elderly = checkElderly.ElderlyId,
                    CreatedAt = DateTime.UtcNow.AddHours(7),
                    Status = SD.GeneralStatus.ACTIVE,
                    CreatedBy = req.CreatedBy,
                    Treatment = req.Treatment,
                    EndDate = req.EndDate,
                    Url = imageUrl
                };

                

                var rsImage = await _unitOfWork.PrescriptionRepository.CreateAsync(newPrescription);

                foreach (var medication in req.Medication)
                {
                    var rs = new Medication
                    {
                        Dosage = medication.Dosage,
                        CreatedDate = DateTime.UtcNow.AddHours(7),
                        EndDate = null,
                        FrequencyType = medication.FrequencyType,
                        StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7)),
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
                        return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, $"Không thể tạo đơn thuốc có tên là {medication.MedicationName}");
                    }

                    var createSchedule = await GenerateMedicationSchedules(rs, medication.Schedule, medication.FrequencySelect);
                    if (createSchedule < 1)
                    {
                        return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, $"Không thể tạo lịch cho {medication.MedicationName}");
                    }
                }

                return new BusinessResult(Const.SUCCESS_CREATE, "Tạo thành công", req);
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
                throw new InvalidOperationException("Lịch uống thuốc không thể trống");
            }

            var now = DateTime.UtcNow.AddHours(7);
            var currentDate = ConvertToDateTime(medication.StartDate).Value;

            // Nếu ngày bắt đầu là hôm nay, điều chỉnh để bỏ qua các thời điểm đã qua
            if (currentDate.Date == now.Date)
            {
                currentDate = now; // Bắt đầu từ thời điểm hiện tại
            }
            else if (currentDate < now)
            {
                currentDate = now.Date.AddDays(1); // Nếu ngày bắt đầu đã qua, bắt đầu từ ngày mai
            }

            var remaining = medication.Remaining;
            var rs = 0;
            var maxDate = currentDate.AddMonths(6); // Giới hạn 6 tháng
            const int maxSchedules = 1000; // Giới hạn tổng số lịch

            if (!int.TryParse(medication.Dosage.Split(' ')[0], out int dosage))
            {
                throw new InvalidOperationException("Sai format của liều lượng, ví dụ '1 Viên'.");
            }

            // Chỉ xử lý nếu là FrequencyType = "Select" và có chọn ngày
            if (medication.FrequencyType == "Select" && frequencySelect != null && frequencySelect.Any())
            {
                // Chuyển đổi ngày trong tuần từ tiếng Việt sang DayOfWeek
                var selectedDays = frequencySelect.Select(day =>
                {
                    return day switch
                    {
                        "Monday" => DayOfWeek.Monday,
                        "Tuesday" => DayOfWeek.Tuesday,
                        "Wednesday" => DayOfWeek.Wednesday,
                        "Thursday" => DayOfWeek.Thursday,
                        "Friday" => DayOfWeek.Friday,
                        "Saturday" => DayOfWeek.Saturday,
                        "Sunday" => DayOfWeek.Sunday,
                        "Thứ 2" => DayOfWeek.Monday,
                        "Thứ 3" => DayOfWeek.Tuesday,
                        "Thứ 4" => DayOfWeek.Wednesday,
                        "Thứ 5" => DayOfWeek.Thursday,
                        "Thứ 6" => DayOfWeek.Friday,
                        "Thứ 7" => DayOfWeek.Saturday,
                        "Chủ nhật" => DayOfWeek.Sunday,
                        _ => throw new InvalidOperationException($"Ngày không hợp lệ: {day}")
                    };
                }).ToList();

                // Tìm ngày tiếp theo phù hợp với ngày đã chọn
                while (remaining > 0 && currentDate <= maxDate && rs < maxSchedules)
                {
                    if (selectedDays.Contains(currentDate.DayOfWeek))
                    {
                        foreach (var time in scheduleTimes)
                        {
                            if (remaining <= 0) break;

                            if (!TimeSpan.TryParse(time, out var timeOfDay))
                            {
                                throw new InvalidOperationException($"Thời gian không hợp lệ: {time}");
                            }

                            var scheduleTime = currentDate.Date.Add(timeOfDay);

                            // Bỏ qua nếu thời gian đã qua trong ngày hiện tại
                            if (currentDate.Date == now.Date && scheduleTime <= now)
                            {
                                continue;
                            }

                            var schedule = new MedicationSchedule
                            {
                                MedicationId = medication.MedicationId,
                                Dosage = medication.Dosage,
                                Status = "Unused",
                                DateTaken = scheduleTime,
                                IsTaken = false
                            };

                            var result = await _unitOfWork.MedicationScheduleRepository.CreateAsync(schedule);
                            if (result > 0)
                            {
                                rs++;
                                remaining -= dosage;
                            }
                        }
                    }

                    // Chuyển sang ngày tiếp theo
                    currentDate = currentDate.AddDays(1);
                }
            }
            else
            {
                // Xử lý các FrequencyType khác (Every X day)
                while (remaining > 0 && currentDate <= maxDate && rs < maxSchedules)
                {
                    foreach (var time in scheduleTimes)
                    {
                        if (remaining <= 0) break;

                        if (!TimeSpan.TryParse(time, out var timeOfDay))
                        {
                            throw new InvalidOperationException($"Thời gian không hợp lệ: {time}");
                        }

                        var scheduleTime = currentDate.Date.Add(timeOfDay);

                        // Bỏ qua nếu thời gian đã qua trong ngày hiện tại
                        if (currentDate.Date == now.Date && scheduleTime <= now)
                        {
                            continue;
                        }

                        var schedule = new MedicationSchedule
                        {
                            MedicationId = medication.MedicationId,
                            Dosage = medication.Dosage,
                            Status = "Unused",
                            DateTaken = scheduleTime,
                            IsTaken = false
                        };

                        var result = await _unitOfWork.MedicationScheduleRepository.CreateAsync(schedule);
                        if (result > 0)
                        {
                            rs++;
                            remaining -= dosage;
                        }
                    }

                    // Xác định ngày tiếp theo dựa trên FrequencyType
                    if (medication.FrequencyType.StartsWith("Every "))
                    {
                        string numberStr = medication.FrequencyType.Replace("Every ", "").Replace(" day", "").Trim();
                        if (int.TryParse(numberStr, out int day))
                        {
                            currentDate = currentDate.AddDays(day);
                        }
                        else
                        {
                            currentDate = currentDate.AddDays(1);
                        }
                    }
                    else
                    {
                        currentDate = currentDate.AddDays(1);
                    }
                }
            }

            // Cập nhật EndDate
            medication.EndDate = DateOnly.FromDateTime(currentDate);
            await _unitOfWork.MedicationRepository.UpdateAsync(medication);

            return rs;
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
                    return new BusinessResult(Const.FAIL_READ, "Không thể tìm thấy đơn thuốc");
                }

                var medicationDtos = new List<MedicationModel>();

                foreach (var medication in prescription.Medications
                    .Where(m => m.StartDate <= today &&
                                 (m.EndDate == null || m.EndDate >= today)))
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

                return new BusinessResult(Const.SUCCESS_READ, "Truy xuất dữ liệu thành công", result);
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

            var schedules = _unitOfWork.MedicationScheduleRepository
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
                    return new BusinessResult(Const.FAIL_READ, "Đơn thuốc không tồn tại.");
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
                    var existingPrescription = _unitOfWork.PrescriptionRepository.FindByCondition(p => p.PrescriptionId == prescriptionId && p.Status == "Active").FirstOrDefault();
                    if (existingPrescription == null)
                    {
                        return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG, "Đơn thuốc không tồn tại.");
                    }
                    var today = DateTime.UtcNow.AddHours(7);
                    existingPrescription.Treatment = req.Treatment;
                    var updatePrescriptionResult = await _unitOfWork.PrescriptionRepository.UpdateAsync(existingPrescription);
                    if (updatePrescriptionResult < 1)
                    {
                        return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG, "Cập nhật không thành công");
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
                                return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG, "Cập nhật không thành công");
                            }

                            var deleteSchedulesResult = await _unitOfWork.MedicationScheduleRepository.DeleteByMedicationIdAsync(existingMedication.MedicationId, today);
                            if (deleteSchedulesResult < 0)
                            {
                                return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG, "Xóa lịch thuốc không thành công");
                            }

                            var createScheduleResult = await GenerateMedicationSchedules(existingMedication, medicationReq.Schedule, medicationReq.FrequencySelect);
                            if (createScheduleResult < 1)
                            {
                                return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG, "Tạo mới lịch trình không thành công.");
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
                                return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG, "Tạo thuốc không thành công.");
                            }

                            var createScheduleResult = await GenerateMedicationSchedules(newMedication, medicationReq.Schedule, medicationReq.FrequencySelect);
                            if (createScheduleResult < 1)
                            {
                                return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG, "Tạo lịch cho đơn thuốc không thành công.");
                            }
                        }
                    }

                    return new BusinessResult(Const.SUCCESS_UPDATE, "Cập nhật thuốc thành công.", req);
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
                        return new BusinessResult(Const.FAIL_UPDATE, $"Sai format ngày: {confirmation.DateTaken}");
                    }
                    var medication = await _unitOfWork.MedicationRepository.GetByMedicationIdAsync(confirmation.MedicationId);
                    var schedule = await _unitOfWork.MedicationScheduleRepository
                        .GetByDateAndMedicationIdAsync(dateTaken, confirmation.MedicationId);

                    if (schedule != null)
                    {
                        schedule.IsTaken = true;
                        schedule.Status = confirmation.Status;

                        var updateResult = await _unitOfWork.MedicationScheduleRepository.UpdateAsync(schedule);

                        if (!int.TryParse(medication.Dosage.Split(' ')[0], out int dosage))
                        {
                            throw new InvalidOperationException("Sai format của liều lượng, ví dụ như '1 Viên'.");
                        }

                        medication.Remaining = medication.Remaining - dosage;

                        await _unitOfWork.MedicationRepository.UpdateAsync(medication);
                        if (updateResult > 0)
                        {
                            updatedCount++;
                        }

                        if (confirmation.Status.Equals("Skip"))
                        {
                            var elderly = await _unitOfWork.AccountRepository.GetAccountAsync(medication.Elderly.AccountId);

                            var listFamilyMember = await _groupService.GetAllFamilyMembersByElderly(elderly.AccountId);

                            foreach (var member in listFamilyMember)
                            {
                                var familyMember = await _unitOfWork.AccountRepository.GetAccountAsync(member);
                                if (!string.IsNullOrEmpty(familyMember.DeviceToken) && familyMember.DeviceToken != "string")
                                {
                                    // Send notification
                                    await _notificationService.SendNotification(
                                        familyMember.DeviceToken,
                                        "Bỏ qua lịch uống thuốc",
                                        $"{elderly.FullName} vừa bỏ qua lịch uống thuốc lúc {schedule.DateTaken?.ToString("HH:mm dd/MM/yyyy")}.");

                                    var newNotification = new Data.Models.Notification
                                    {
                                        NotificationType = "Bỏ qua lịch uống thuốc",
                                        AccountId = familyMember.AccountId,
                                        Status = SD.GeneralStatus.ACTIVE,
                                        Title = "Bỏ qua lịch uống thuốc",
                                        Message = $"{elderly.FullName} vừa bỏ qua lịch uống thuốc lúc {schedule.DateTaken?.ToString("HH:mm dd/MM/yyyy")}.",
                                        CreatedDate = System.DateTime.UtcNow.AddHours(7),
                                    };

                                    await _unitOfWork.NotificationRepository.CreateAsync(newNotification);
                                }
                            }                              
                        }
                    }
                }

                if (updatedCount == request.Confirmations.Count)
                {
                    return new BusinessResult(Const.SUCCESS_UPDATE, "Xác nhận uống thuốc thành công");
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
                    return new BusinessResult(Const.FAIL_READ, "Không thể tìm thấy thuốc");
                }

                medication.Status = status;

                var result = await _unitOfWork.MedicationRepository.UpdateAsync(medication);
                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_UPDATE, "Cập nhật trạng thái thuốc thành công");
                }

                return new BusinessResult(Const.FAIL_UPDATE, "Không thể cập nhật trạng thái uống thuốc");
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
                    return new BusinessResult(Const.FAIL_READ, "Không thể tìm thấy hóa đơn thuốc");
                }

                var medicationDtos = new List<UpdateMedicationModel>();

                foreach (var medication in prescription.Medications)
                {
                    List<string> frequencySelect = null;
                    List<string> scheduleTimes = null;

                    if (medication.FrequencyType == "Select")
                    {
                        // Lấy tất cả các ngày trong tuần từ lịch uống thuốc
                        var daysOfWeek = medication.MedicationSchedules
                            .Where(ms => ms.DateTaken.HasValue)
                            .Select(ms => ms.DateTaken.Value.DayOfWeek.ToString())
                            .Distinct()
                            .ToList();

                        frequencySelect = daysOfWeek;

                        // Lấy tất cả các giờ uống thuốc (không trùng lặp)
                        scheduleTimes = medication.MedicationSchedules
                            .Where(ms => ms.DateTaken.HasValue)
                            .Select(ms => ms.DateTaken.Value.ToString("HH:mm"))
                            .Distinct()
                            .ToList();
                    }
                    else
                    {
                        // Với các loại tần suất khác, lấy tất cả các giờ uống thuốc
                        scheduleTimes = medication.MedicationSchedules
                            .Where(ms => ms.DateTaken.HasValue)
                            .Select(ms => ms.DateTaken.Value.ToString("HH:mm"))
                            .Distinct()
                            .ToList();
                    }

                    var medicationDto = new UpdateMedicationModel
                    {
                        MedicationId = medication.MedicationId,
                        MedicationName = medication.MedicationName,
                        Treatment = medication.Treatment,
                        Dosage = medication.Dosage,
                        Shape = medication.Shape,
                        Remaining = medication.Remaining,
                        FrequencyType = medication.FrequencyType,
                        FrequencySelect = frequencySelect,
                        IsBeforeMeal = medication.IsBeforeMeal,
                        Note = medication.Note,
                        Schedule = scheduleTimes
                    };

                    medicationDtos.Add(medicationDto);
                }

                var result = new
                {
                    Id = prescription.PrescriptionId,
                    MedicationImage = prescription.Url,
                    CreatedBy = prescription.CreatedBy,
                    Treatment = prescription.Treatment,
                    EndDate = prescription.EndDate?.ToString("yyyy-MM-dd"),
                    StartDate = prescription.CreatedAt.ToString("yyyy-MM-dd"),
                    Medicines = medicationDtos
                };

                return new BusinessResult(Const.SUCCESS_READ, "Truy xuất thuốc thành công", result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, ex.Message);
            }
        }
        public async Task<IBusinessResult> GetUsedPrescriptionsOfElderly(int accountId)
        {
            try
            {
                // Kiểm tra tài khoản người cao tuổi
                var checkAccount = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(accountId);
                if (checkAccount == null || checkAccount.Elderly == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "Không tìm thấy thông tin người cao tuổi");
                }

                // Lấy tất cả toa thuốc đã sử dụng (status Inactive)
                var prescriptions = await _unitOfWork.PrescriptionRepository
                    .GetInactivePrescriptionsByElderlyId(checkAccount.Elderly.ElderlyId);

                if (prescriptions == null || !prescriptions.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, "Không tìm thấy toa thuốc đã sử dụng");
                }

                var result = new List<object>();

                foreach (var prescription in prescriptions)
                {
                    var medicationDtos = new List<UpdateMedicationModel>();

                    foreach (var medication in prescription.Medications)
                    {
                        List<string> frequencySelect = null;
                        List<string> scheduleTimes = null;

                        if (medication.FrequencyType == "Select")
                        {
                            // Lấy các ngày trong tuần từ lịch uống thuốc
                            var daysOfWeek = medication.MedicationSchedules
                                .Where(ms => ms.DateTaken.HasValue)
                                .Select(ms => ms.DateTaken.Value.DayOfWeek.ToString())
                                .Distinct()
                                .ToList();

                            frequencySelect = daysOfWeek;

                            // Lấy các giờ uống thuốc
                            scheduleTimes = medication.MedicationSchedules
                                .Where(ms => ms.DateTaken.HasValue)
                                .Select(ms => ms.DateTaken.Value.ToString("HH:mm"))
                                .Distinct()
                                .ToList();
                        }
                        else
                        {
                            // Lấy tất cả các giờ uống thuốc
                            scheduleTimes = medication.MedicationSchedules
                                .Where(ms => ms.DateTaken.HasValue)
                                .Select(ms => ms.DateTaken.Value.ToString("HH:mm"))
                                .Distinct()
                                .ToList();
                        }

                        var medicationDto = new UpdateMedicationModel
                        {
                            MedicationId = medication.MedicationId,
                            MedicationName = medication.MedicationName,
                            Treatment = medication.Treatment,
                            Dosage = medication.Dosage,
                            Shape = medication.Shape,
                            Remaining = medication.Remaining,
                            FrequencyType = medication.FrequencyType,
                            FrequencySelect = frequencySelect,
                            IsBeforeMeal = medication.IsBeforeMeal,
                            Note = medication.Note,
                            Schedule = scheduleTimes
                        };

                        medicationDtos.Add(medicationDto);
                    }

                    result.Add(new
                    {
                        Id = prescription.PrescriptionId,
                        MedicationImage = prescription.Url,
                        CreatedBy = prescription.CreatedBy,
                        Treatment = prescription.Treatment,
                        EndDate = prescription.EndDate?.ToString("yyyy-MM-dd"),
                        StartDate = prescription.CreatedAt.ToString("yyyy-MM-dd"),
                        Status = prescription.Status,
                        Medicines = medicationDtos
                    });
                }

                return new BusinessResult(Const.SUCCESS_READ, "Lấy danh sách toa thuốc đã sử dụng thành công", result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, ex.Message);
            }
        }
    }
}

