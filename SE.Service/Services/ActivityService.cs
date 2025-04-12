using System.Globalization;
using AutoMapper;
using Microsoft.Identity.Client;
using SE.Common;
using SE.Common.DTO;
using SE.Common.Enums;
using SE.Common.Response.HealthIndicator;
using SE.Data.Models;
using SE.Data.Repository;
using SE.Data.UnitOfWork;
using SE.Service.Base;

namespace SE.Service.Services
{
    public interface IActivityService
    {
        Task<IBusinessResult> GetAllActivityForDay(int elderlyId, DateOnly date);
        Task<IBusinessResult> CreateActivityWithSchedules(CreateActivityModel model);
        Task<IBusinessResult> UpdateActivityWithSchedules(UpdateScheduleModel model);
        Task<IBusinessResult> UpdateStatusActivity(int activityId, DateOnly date);

    }

    public class ActivityService : IActivityService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ActivityService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }


        public async Task<IBusinessResult> GetAllActivityForDay(int accountId, DateOnly date)
        {
            try
            {
                var checkAccount = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(accountId);

                var activities = await _unitOfWork.ActivityRepository.GetActivitiesIncludeOfElderly(checkAccount.Elderly.ElderlyId);
                var medicationSchedules = await _unitOfWork.MedicationScheduleRepository.GetMedicationSchedulesForDay(checkAccount.Elderly.ElderlyId, date);
                var professorAppointments = await _unitOfWork.ProfessorAppointmentRepository.GetProfessorAppointmentsForDay(checkAccount.Elderly.ElderlyId, date);

                var createBy = activities.Select(a => a.CreatedBy).FirstOrDefault();

                var result = new List<GetScheduleInDayResponse>();
                var activitySchedules = activities
                                        .SelectMany(a => a.ActivitySchedules
                                            .Where(s => s.StartTime?.ToString("yyyy-MM-dd") == date.ToString("yyyy-MM-dd"))
                                            .Select(s => new GetScheduleInDayResponse
                                            {
                                                ActivityId = a.ActivityId,
                                                Title = a.ActivityName,
                                                Description = a.ActivityDescription,
                                                StartTime = s.StartTime?.ToString("HH:mm"),
                                                EndTime = s.EndTime?.ToString("HH:mm"),
                                                Type = "Activity",
                                                CreatedBy = createBy,
                                                Duration = CalculateDuration(date, s.ActivityScheduleId, a.ActivitySchedules)
                                            })).ToList();
                result.AddRange(activitySchedules);

                var medicationScheduleResponses = medicationSchedules
                    .Select(ms => new GetScheduleInDayResponse
                    {
                        Title = ms.Medication.MedicationName,
                        Description = "Dùng " + ms.Dosage + " vào " + ms.DateTaken?.ToString("HH:mm"),
                        StartTime = ms.DateTaken?.ToString("HH:mm"),
                        EndTime = null,
                        Type = "Medication"
                    })
                    .ToList();

                result.AddRange(medicationScheduleResponses);

                var professorAppointmentResponses = professorAppointments
                    .Select(pa => new GetScheduleInDayResponse
                    {
                        Title = "Tư vấn với bác sĩ",
                        Description = "Bác sĩ " + pa.UserSubscription.Professor.Account.FullName,
                        StartTime = pa.StartTime?.ToString("HH:mm"),
                        EndTime = pa.EndTime?.ToString("HH:mm"),
                        Type = "Professor Appointment"
                    })
                    .ToList();

                result.AddRange(professorAppointmentResponses);
                result = result.OrderBy(r => r.StartTime).ToList();

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static System.DateTime ConvertToDateTime(DateOnly dateOnly)
        {
            return dateOnly.ToDateTime(new TimeOnly(0, 0));
        }

        private static int CalculateDuration(DateOnly specificDate, int activityScheduleId, ICollection<ActivitySchedule> activitySchedules)
        {
            // Tìm ActivitySchedule dựa trên ActivityScheduleId
            var currentSchedule = activitySchedules.FirstOrDefault(s => s.ActivityScheduleId == activityScheduleId);
            if (currentSchedule == null)
            {
                return 0; // Trả về 0 nếu không tìm thấy ActivitySchedule
            }

            // Tìm ngày kết thúc của hoạt động (ngày StartTime của bản ghi cuối cùng)
            DateTime endDate = activitySchedules.Max(s => s.StartTime)?.Date ?? DateTime.MinValue;

            // Tính số ngày từ ngày hiện tại (specificDate) đến ngày kết thúc (endDate)
            TimeSpan duration = endDate - ConvertToDateTime(specificDate);
            return duration.Days + 1; // Bao gồm cả ngày hiện tại và ngày kết thúc
        }
        public async Task<IBusinessResult> CreateActivityWithSchedules(CreateActivityModel model)
        {
            try
            {
                // Kiểm tra tài khoản người cao tuổi
                var elderlyAccount = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(model.AccountId);
                if (elderlyAccount == null || elderlyAccount.Elderly == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "Không thể tìm thấy người già");
                }

                // Lấy tất cả lịch trình hiện có của người cao tuổi
                var existingSchedules = await _unitOfWork.ActivityScheduleRepository
                    .GetByElderlyIdAsync(elderlyAccount.Elderly.ElderlyId);

                var startDate = model.StartDate;

                // Kiểm tra trùng lịch trước khi tạo
                for (int i = 0; i < model.Duration; i++)
                {
                    var scheduleDate = startDate.AddDays(i);
                    foreach (var schedule in model.Schedules)
                    {
                        var newStartTime = new DateTime(scheduleDate.Year, scheduleDate.Month, scheduleDate.Day,
                                                      schedule.StartTime.Hour, schedule.StartTime.Minute, 0);
                        var newEndTime = new DateTime(scheduleDate.Year, scheduleDate.Month, scheduleDate.Day,
                                                    schedule.EndTime.Hour, schedule.EndTime.Minute, 0);

                        // Kiểm tra trùng lịch với các hoạt động hiện có
                        bool isConflict = existingSchedules.Any(existing =>
                            existing.StartTime < newEndTime &&
                            existing.EndTime > newStartTime);

                        if (isConflict)
                        {
                            var conflictActivity = existingSchedules.First(existing =>
                                existing.StartTime < newEndTime &&
                                existing.EndTime > newStartTime).Activity;

                            return new BusinessResult(Const.FAIL_CREATE,
                                $"Lịch trình bị trùng với hoạt động '{conflictActivity.ActivityName}' " +
                                $"({conflictActivity.ActivityId}) từ {conflictActivity.ActivitySchedules.First().StartTime:HH:mm} " +
                                $"đến {conflictActivity.ActivitySchedules.First().EndTime:HH:mm}");
                        }
                    }
                }

                // Tạo hoạt động mới nếu không có lịch trùng
                var newActivity = new Activity
                {
                    ElderlyId = elderlyAccount.Elderly.ElderlyId,
                    ActivityName = model.Title,
                    ActivityDescription = model.Description,
                    CreatedBy = model.CreatedBy,
                    Status = SD.GeneralStatus.ACTIVE
                };
                await _unitOfWork.ActivityRepository.CreateAsync(newActivity);

                // Tạo lịch trình mới
                for (int i = 0; i < model.Duration; i++)
                {
                    var scheduleDate = startDate.AddDays(i);
                    foreach (var schedule in model.Schedules)
                    {
                        var newSchedule = new ActivitySchedule
                        {
                            ActivityId = newActivity.ActivityId,
                            StartTime = new DateTime(scheduleDate.Year, scheduleDate.Month, scheduleDate.Day,
                                                  schedule.StartTime.Hour, schedule.StartTime.Minute, 0),
                            EndTime = new DateTime(scheduleDate.Year, scheduleDate.Month, scheduleDate.Day,
                                                schedule.EndTime.Hour, schedule.EndTime.Minute, 0),
                            Status = SD.GeneralStatus.ACTIVE
                        };
                        await _unitOfWork.ActivityScheduleRepository.CreateAsync(newSchedule);
                    }
                }

                return new BusinessResult(Const.SUCCESS_CREATE, "Tạo mới hoạt động và lịch trình thành công");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, ex.Message);
            }
        }
        public async Task<IBusinessResult> UpdateActivityWithSchedules(UpdateScheduleModel model)
        {
            try
            {
                var existingActivity = await _unitOfWork.ActivityRepository.GetActivitiesByIdInclude(model.ActivityId);
                if (existingActivity == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "Không thể tìm thấy hoạt động");
                }

                existingActivity.ActivityName = model.Title;
                existingActivity.ActivityDescription = model.Description;
                existingActivity.CreatedBy = model.CreatedBy;

                var today = model.Date;
                var schedulesToDelete = existingActivity.ActivitySchedules
                                        .Where(s => s.StartTime.HasValue && DateOnly.FromDateTime(s.StartTime.Value) >= today)
                                        .ToList();
                if (schedulesToDelete == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "Lịch trình không tìm thấy");
                }
                if (schedulesToDelete.Any())
                {
                    foreach (var schedule in schedulesToDelete)
                    {
                        _unitOfWork.ActivityScheduleRepository.Remove(schedule);
                    }
                }
                for (int i = 0; i < model.Duration; i++)
                {
                    var scheduleDate = today.AddDays(i);
                    foreach (var schedule in model.Schedules)
                    {
                        var newSchedule = new ActivitySchedule
                        {
                            ActivityId = existingActivity.ActivityId,
                            StartTime = new DateTime(scheduleDate.Year, scheduleDate.Month, scheduleDate.Day, schedule.StartTime.Hour, schedule.StartTime.Minute, 0),
                            EndTime = new DateTime(scheduleDate.Year, scheduleDate.Month, scheduleDate.Day, schedule.EndTime.Hour, schedule.EndTime.Minute, 0),
                            Status = "Active"
                        };
                        var acShe = await _unitOfWork.ActivityScheduleRepository.CreateAsync(newSchedule);
                        if (acShe < 1)
                        {
                            return new BusinessResult(Const.FAIL_UPDATE, "Cập nhật hoạt động thành công");
                        }
                    }
                }

                var rs = await _unitOfWork.ActivityRepository.UpdateAsync(existingActivity);
                if (rs < 1)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Cập nhật hoạt động và lịch trình không thành công.");
                }
                return new BusinessResult(Const.SUCCESS_UPDATE, "Cập nhật hoạt động và lịch trình thành công.");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IBusinessResult> UpdateStatusActivity(int activityId, DateOnly date)
        {
            try
            {
                var checkActivity = await _unitOfWork.ActivityRepository.GetActivitiesByIdInclude(activityId);
                if (checkActivity == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "Không thể tìm thấy hoạt động");
                }

                checkActivity.Status = SD.GeneralStatus.INACTIVE;
                var rs = await _unitOfWork.ActivityRepository.UpdateAsync(checkActivity);

                var today = date;
                var schedulesToDelete = checkActivity.ActivitySchedules
                                        .Where(s => s.StartTime.HasValue && DateOnly.FromDateTime(s.StartTime.Value) >= today)
                                        .ToList();
                if (schedulesToDelete == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "Không thể tìm thấy lịch trình.");
                }
                if (schedulesToDelete.Any())
                {
                    foreach (var schedule in schedulesToDelete)
                    {
                        _unitOfWork.ActivityScheduleRepository.Remove(schedule);
                    }
                }

                if (rs < 1)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Cập nhật trạng thái không thành công.");
                }
                return new BusinessResult(Const.SUCCESS_UPDATE, "Cập nhật trạng thái thành công.");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
