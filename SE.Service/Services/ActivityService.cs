using System.Globalization;
using AutoMapper;
using SE.Common;
using SE.Common.DTO;
using SE.Common.Enums;
using SE.Data.Models;
using SE.Data.Repository;
using SE.Data.UnitOfWork;
using SE.Service.Base;

namespace SE.Service.Services
{
    public interface IActivityService
    {
        Task<IBusinessResult> GetAllScheduleForDay(DateTime date);
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
        public async Task<IBusinessResult> GetAllScheduleForDay(DateTime date)
        {
            try
            {
              /*  if (!DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Invalid date format. Please use YYYY-MM-DD.");
                }

                DateTime startOfDay = parsedDate.Date;*/

                  var schedules = await _unitOfWork.ActivityScheduleRepository.GetAllAsync();
                var filteredSchedules = schedules
                    .Where(a => a.StartTime.HasValue && a.StartTime.Value.Date >= date)
                    .Select(a => new GetAllScheduleModel
                    {
                        ActivityId = a.Activity.ActivityId,
                        ElderlyId = a.Activity.ElderlyId,
                        ActivityName = a.Activity.ActivityName,
                        StartTime = a.StartTime,
                        EndTime = a.EndTime
                    })
                    .ToList();

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, filteredSchedules);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, ex.Message);
            }
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
