using AutoMapper;
using Google.Type;
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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Tesseract;
using static System.Net.Mime.MediaTypeNames;


namespace SE.Service.Services
{
    public interface IMedicationService
    {
        Task<IBusinessResult> ScanFromPic(IFormFile file, int ElderlyID);

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

                var image = await CloudinaryHelper.UploadImageAsync(file);
                var newImage = new Prescription
                {
                    Elderly = ElderlyID,
                    CreatedAt = System.DateTime.Now,
                    Status = SD.GeneralStatus.ACTIVE,
                     Url = image.Url
                };

                var rsImage = await _unitOfWork.PrescriptionRepository.CreateAsync(newImage);

                List<string> medicines = ParseMedicineDetails(extractedText);

                var listMediFromPic = CreateMedicationRequests(medicines);

                foreach (var medicine in listMediFromPic)
                {
                    var rs = new Medication
                    {
                        ElderlyId = ElderlyID,
                        Dosage = (medicine.Dosage == "I Viên") ? "1 Viên" : medicine.Dosage,
                        CreatedDate = System.DateTime.Now,
                        DateFrequency = medicine.DateFrequency,
                        EndDate = DateOnly.FromDateTime(medicine.EndDate ?? System.DateTime.MinValue),
                        FrequencyType = medicine.TimeFrequency,
                        TimeFrequency = medicine.TimeFrequency,
                        StartDate = DateOnly.FromDateTime(medicine.StartDate ?? System.DateTime.MinValue),
                        Shape = medicine.Shape,
                        Status = SD.GeneralStatus.ACTIVE,
                        PrescriptionId = newImage.PrescriptionId,
                        MedicationName  = medicine.MedicationName
                    };
                    var rs1 =await _unitOfWork.MedicationRepository.CreateAsync(rs);
                   

                    var newSchedule = new MedicationSchedule
                    {
                        MedicationId = rs.MedicationId,
                        Dosage = (medicine.Dosage == "I Viên") ? "1 Viên" : medicine.Dosage,
                        TimeOfDay = (rs.TimeFrequency == "Sáng") ? new TimeOnly(7, 0) :
                                    (rs.TimeFrequency == "Trưa") ? new TimeOnly(11, 0) :
                                    (rs.TimeFrequency == "Chiều") ? new TimeOnly(17, 0) :
                                    (rs.TimeFrequency == "Tối") ? new TimeOnly(20, 0) : TimeOnly.MinValue,
                        Status = SD.GeneralStatus.ACTIVE
                    };

                    var rs2 = await _unitOfWork.MedicationScheduleRepository.CreateAsync(newSchedule);

                    var newScheduleDay = new MedicationDay
                    {
                        Status = SD.GeneralStatus.ACTIVE,
                        DayOfWeek = "All Day",
                        MedicationScheduleId = newSchedule.MedicationScheduleId
                    };
                    var rs3 =await _unitOfWork.MedicationDayRepository.CreateAsync(newScheduleDay);

                }



                return new BusinessResult(Const.SUCCESS_CREATE, "Medication created successfully.");


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

        static List<string> ParseMedicineDetails(string text)
        {
            string pattern = @"(\d+\.\.\s.*?Uông\s*:\s*.*?)(?=\n\s*\d+\.\.|$)";
            MatchCollection matches = Regex.Matches(text, pattern, RegexOptions.Singleline);

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


        static List<NewMedicationFromPicDTO> CreateMedicationRequests(List<string> medicines)
        {
            List<NewMedicationFromPicDTO> requests = new List<NewMedicationFromPicDTO>();
            int daysToAdd = ExtractDays(medicines);
            string name="Unknow";
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
                            StartDate = System.DateTime.Now,
                            EndDate = System.DateTime.Now.AddDays(double.Parse(quantity)),
                            Dosage = dosage,
                            MedicationName = name,
                            TimeFrequency= timeToTake,
                            DateFrequency =1

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





        public async Task<IBusinessResult> CreateMedication(CreateMedicationRequest req)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(req.MedicationName))
                {
                    return new BusinessResult(Const.FAIL_READ, "MEDICATION NAME MUST NOT BE EMPTY");
                }

                if (req.StartDate > req.EndDate)
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "THỜI GIAN BẮT ĐẦU PHẢI TRƯỚC THỜI GIAN KẾT THÚC!");
                }

                var medication = _mapper.Map<Medication>(req);

                var result = await _unitOfWork.MedicationRepository.CreateAsync(medication);
                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE, "Medication created successfully.", req);
                }

                return new BusinessResult(Const.FAIL_CREATE, "Failed to create medication.");
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
                medication.TimeFrequency = req.TimeFrequency;
                medication.DateFrequency = req.DateFrequency;
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

        public async Task<IBusinessResult> GetAllMedications()
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

