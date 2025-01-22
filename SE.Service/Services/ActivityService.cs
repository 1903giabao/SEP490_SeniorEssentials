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
        Task<IBusinessResult> GetAllScheduleForDay(string date);
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
        public async Task<IBusinessResult> GetAllScheduleForDay(string date)
        {
            try
            {
                // Validate the input date format
                if (!DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Invalid date format. Please use YYYY-MM-DD.");
                }

                // Extract the date part (this will be 2025-01-23 00:00:00)
                DateTime startOfDay = parsedDate.Date; // This gives you the date part

                  var schedules = await _unitOfWork.ActivityScheduleRepository.GetAllAsync();
                var filteredSchedules = schedules
                    .Where(a => a.StartTime.HasValue && a.StartTime.Value.Date == startOfDay) // Compare only the date part
                    .Select(a => new GetAllScheduleModel
                    {
                        ActivityId = a.Activity.ActivityId,
                        ElderlyId = a.Activity.ElderlyId,
                        ActivityName = a.Activity.ActivityName,
                        StartTime = a.StartTime, // Format for output
                        EndTime = a.EndTime // Format for output
                    })
                    .ToList();

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, filteredSchedules);
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here)
                return new BusinessResult(Const.FAIL_READ, ex.Message);
            }
            }
        public async Task<IBusinessResult> CreateActivityWithSchedule(CreateActivityModel model)
        {
            try
            {
                // Validate the input model
                if (model == null || string.IsNullOrWhiteSpace(model.ActivityName) || model.StartTime == null || model.EndTime == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Invalid activity data.");
                }

                // Map the CreateActivityModel to Activity entity
                var activity = _mapper.Map<Activity>(model);
                activity.Status = SD.GeneralStatus.ACTIVE; // Set the status
                await _unitOfWork.ActivityRepository.CreateAsync(activity);

                // Create the ActivitySchedule entity
                var activitySchedule = new ActivitySchedule
                {
                    ActivityId = activity.ActivityId, // Assuming ActivityId is generated after saving the activity
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    Status = SD.GeneralStatus.ACTIVE
                };

                await _unitOfWork.ActivityScheduleRepository.CreateAsync(activitySchedule);

                return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, "Activity and schedule created successfully.");
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here)
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
                // Validate the input parameter
                if (activityId <= 0)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Invalid activity ID.");
                }

                // Fetch the existing activity
                var activity = await _unitOfWork.ActivityRepository.GetByIdAsync(activityId);
                if (activity == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Activity not found.");
                }

                // Fetch associated activity schedules
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

                // Return the result wrapped in a business result object
                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, associatedSchedules);
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here)
                throw new Exception(ex.Message, ex);
            }

        }
    }
}
