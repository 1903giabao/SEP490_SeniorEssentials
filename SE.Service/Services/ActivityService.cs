using System.Globalization;
using AutoMapper;
using SE.Common;
using SE.Common.DTO;
using SE.Common.Enums;
using SE.Common.Response;
using SE.Data.Models;
using SE.Data.Repository;
using SE.Data.UnitOfWork;
using SE.Service.Base;

namespace SE.Service.Services
{
    public interface IActivityService
    {
        Task<List<GetScheduleInDayResponse>> GetAllActivityForDay(int elderlyId, DateTime date);
        Task<IBusinessResult> CreateActivityWithSchedule(CreateActivityModel model);
        Task<IBusinessResult> UpdateSchedule(UpdateScheduleModel model);
        Task<IBusinessResult> GetActivityById(int activityId);

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
        public async Task<List<GetScheduleInDayResponse>> GetAllActivityForDay(int elderlyId, DateTime date)
        {
            var activities = await _unitOfWork.ActivityRepository.GetActivitiesInclude(elderlyId);
            var medicationSchedules = await _unitOfWork.MedicationScheduleRepository.GetMedicationSchedulesForDay(elderlyId, date);
            var professorAppointments = await _unitOfWork.ProfessorAppointmentRepository.GetProfessorAppointmentsForDay(elderlyId, date);

            var result = new List<GetScheduleInDayResponse>();

            var activitySchedules = activities
                .SelectMany(a => a.ActivitySchedules
                    .Where(s => s.StartTime?.Date == date.Date)
                    .Select(s => new GetScheduleInDayResponse
                    {
                        Title = a.ActivityName,
                        Description = a.ActivityDescription,
                        StartTime = s.StartTime?.ToString("HH:mm"),
                        EndTime = s.EndTime?.ToString("HH:mm"),
                        ElderlyId = a.ElderlyId,
                        Type = "Activity" 
                    }))
                .ToList();

            result.AddRange(activitySchedules);

            // Lấy lịch trình từ MedicationSchedule
            var medicationScheduleResponses = medicationSchedules
                .Select(ms => new GetScheduleInDayResponse
                {
                    Title = ms.Medication.MedicationName, 
                    Description ="Dùng " + ms.Dosage + " vào " + ms.DateTaken?.ToString("HH:mm"),
                    StartTime = ms.DateTaken?.ToString("HH:mm"),
                    EndTime = null,
                    ElderlyId = elderlyId,
                    Type = "Medication"
                })
                .ToList();

            result.AddRange(medicationScheduleResponses);

            var professorAppointmentResponses = professorAppointments
                .Select(pa => new GetScheduleInDayResponse
                {
                    Title = "Tư vấn với bác sĩ",
                    Description = "Bác sĩ " + pa.TimeSlot.ProfessorSchedule.Professor.Account.FullName,
                    StartTime = pa.StartTime?.ToString("HH:mm"),
                    EndTime = pa.EndTime?.ToString("HH:mm"),
                    ElderlyId = elderlyId,
                    Type = "Professor Appointment"
                })
                .ToList();

            result.AddRange(professorAppointmentResponses);
            result = result.OrderBy(r => r.StartTime).ToList();

            return result;
        }
        public async Task<IBusinessResult> CreateActivityWithSchedule(CreateActivityModel model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.ActivityName) || model.StartTime == null || model.EndTime == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Invalid activity data.");
                }

                var activity = _mapper.Map<Activity>(model);
                activity.Status = SD.GeneralStatus.ACTIVE;
                await _unitOfWork.ActivityRepository.CreateAsync(activity);

                var activitySchedule = new ActivitySchedule
                {
                    ActivityId = activity.ActivityId,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    Status = SD.GeneralStatus.ACTIVE
                };

                await _unitOfWork.ActivityScheduleRepository.CreateAsync(activitySchedule);

                return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, "Activity and schedule created successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, ex.Message);
            }
        }

        public async Task<IBusinessResult> UpdateSchedule(UpdateScheduleModel model)
        {
            try
            {
                if (model == null || model.ActivityId <= 0 || model.ActivityScheduleId <= 0)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Invalid date format.");
                }

                var activity = await _unitOfWork.ActivityRepository.GetByIdAsync(model.ActivityId);
                if (activity == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Activity not found.");
                }

                var activitySchedule = await _unitOfWork.ActivityScheduleRepository.GetByIdAsync(model.ActivityScheduleId);
                if (activitySchedule == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Activity schedule not found.");
                }

                activity.ActivityName = model.ActivityName;
                activity.ActivityDescription = model.ActivityDescription;

                activitySchedule.StartTime = model.StartTime;
                activitySchedule.EndTime = model.EndTime;

                await _unitOfWork.ActivityRepository.UpdateAsync(activity);
                await _unitOfWork.ActivityScheduleRepository.UpdateAsync(activitySchedule);

                return new BusinessResult(Const.SUCCESS_UPDATE, Const.SUCCESS_UPDATE_MSG, "Activity and schedule updated successfully.");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        public async Task<IBusinessResult> GetActivityById(int activityId)
        {
            try
            {
                if (activityId <= 0)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Invalid activity ID.");
                }

                var activity = await _unitOfWork.ActivityRepository.GetByIdAsync(activityId);
                if (activity == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Activity not found.");
                }

                var schedules = await _unitOfWork.ActivityScheduleRepository.GetAllAsync();
                var associatedSchedules = schedules
                    .Where(a => a.ActivityId == activityId)
                    .Select(a => new GetAllScheduleModel
                    {
                        ActivityId = activity.ActivityId,
                        ElderlyId = activity.ElderlyId,
                        ActivityName = activity.ActivityName,
                        StartTime = a.StartTime,
                        EndTime = a.EndTime
                    })
                    .ToList();

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, associatedSchedules);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }

        }
    }
}
