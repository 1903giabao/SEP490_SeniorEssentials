using AutoMapper;
using SE.Common.Enums;
using SE.Common.Request;
using SE.Common;
using SE.Data.Models;
using SE.Data.UnitOfWork;
using SE.Service.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SE.Common.Response.Professor;
using static Google.Cloud.Vision.V1.ProductSearchResults.Types;
using SE.Common.Request.Professor;

namespace SE.Service.Services
{
    public interface IProfessorService
    {
        Task<IBusinessResult> CreateSchedule(List<ProfessorScheduleRequest> req);
        Task<IBusinessResult> GetAllProfessor();
        Task<IBusinessResult> GetProfessorDetail(int professorId);
        Task<IBusinessResult> GetTimeSlot(int professorId, DateOnly date);
        Task<IBusinessResult> GetFilteredProfessors(FilterProfessorRequest request);
        Task<IBusinessResult> GetProfessorSchedule(int accountId, string type);

    }

    public class ProfessorService : IProfessorService
        {
            private readonly UnitOfWork _unitOfWork;
            private readonly IMapper _mapper;

            public ProfessorService(UnitOfWork unitOfWork, IMapper mapper)
            {
                _unitOfWork = unitOfWork;
                _mapper = mapper;
            }

            public async Task<IBusinessResult> CreateSchedule(List<ProfessorScheduleRequest> req)
            {
                try
                {
                    var professorIds = req.Select(r => r.ProfessorId).ToList();

                    var existingProfessors = _unitOfWork.ProfessorRepository
                        .FindByCondition(p => professorIds.Contains(p.ProfessorId))
                        .ToList();

                    if (!existingProfessors.Any())
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "NGƯỜI DÙNG KHÔNG TỒN TẠI!");
                    }

                    var scheduleCreateList = new List<ProfessorSchedule>();

                    foreach (var scheduleReq in req)
                    {
                        var schedule = _mapper.Map<ProfessorSchedule>(scheduleReq);
                        schedule.Status = SD.GeneralStatus.ACTIVE;
                        scheduleCreateList.Add(schedule);
                    }

                    var result = await _unitOfWork.ProfessorScheduleRepository.CreateRangeAsync(scheduleCreateList);

                    if (result > 0)
                    {
                        return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, req);
                    }

                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

            public async Task<IBusinessResult> GetAllProfessor()
            {
                try
                {
                    var getAllProfessor = _unitOfWork.ProfessorRepository.GetAll();
                    var result = new List<GetAllProfessorReponse>();
                    var currentDate = DateTime.Now;
                    var currentDayOfWeek = currentDate.DayOfWeek;

                    foreach (var item in getAllProfessor)
                    {
                        var professor = new GetAllProfessorReponse();
                        var professorInfor = await _unitOfWork.AccountRepository.GetProfessorByAccountIDAsync(item.AccountId);

                        professor.ProfessorName = professorInfor.FullName;
                        professor.ProfessorId = professorInfor.Professor.ProfessorId;
                        professor.Major = professorInfor.Professor.Knowledge;
                        professor.Rating = (decimal)professorInfor.Professor.Rating;

                        // Fetch the professor's schedule
                        var professorSchedules = await _unitOfWork.ProfessorScheduleRepository.GetByProfessorIdAsync(professor.ProfessorId);

                        // Find the nearest schedule
                        var nearestSchedule = professorSchedules
                            .Where(ps => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), ps.DayOfWeek, true) >= currentDayOfWeek)
                            .OrderBy(ps => ps.DayOfWeek)
                            .FirstOrDefault();

                        if (nearestSchedule != null)
                        {
                            var timeSlots = await _unitOfWork.TimeSlotRepository.GetByProfessorScheduleIdAsync(nearestSchedule.ProfessorScheduleId);

                            if (timeSlots.Any())
                            {
                                var daysUntilNearestDay = ((DayOfWeek)Enum.Parse(typeof(DayOfWeek), nearestSchedule.DayOfWeek, true) - DateTime.Now.DayOfWeek + 7) % 7;
                                var nearestDate = DateTime.Now.AddDays(daysUntilNearestDay);

                                var nearestTimeSlot = timeSlots.OrderBy(ts => ts.StartTime).First();
                                professor.Date = nearestDate.ToString("dd-MM-yyyy");
                                professor.DateTime = $"{nearestSchedule.DayOfWeek}/{nearestTimeSlot.StartTime}-{nearestTimeSlot.EndTime}";
                            }
                        }

                        // Only add the professor to the result if DateTime is not null or empty
                        if (!string.IsNullOrEmpty(professor.DateTime))
                        {
                            result.Add(professor);
                        }
                    }

                    return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            public async Task<IBusinessResult> GetProfessorDetail(int professorId)
            {
                try
                {
                    var getProfessor = await _unitOfWork.ProfessorRepository.GetAccountByProfessorId(professorId);

                    var getProfessorInfor = await _unitOfWork.AccountRepository.GetProfessorByAccountIDAsync(getProfessor.AccountId);
                    var professor = _mapper.Map<GetProfessorDetail>(getProfessor);
                    professor.ProfessorId = professorId;
                    professor.FullName = getProfessorInfor.FullName;
                    professor.Avatar = getProfessorInfor.Avatar;
                    return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, professor);

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            public async Task<IBusinessResult> GetTimeSlot(int professorId, DateOnly date)
            {
                try
                {
                    // Fetch the professor's schedules for the given date
                    var professorSchedules = await _unitOfWork.ProfessorScheduleRepository.GetByProfessorIdAsync(professorId);

                    // Filter schedules for the given date's day of the week
                    var dayOfWeek = date.DayOfWeek.ToString();
                    var schedulesForDate = professorSchedules
                        .Where(ps => ps.DayOfWeek.Equals(dayOfWeek, StringComparison.OrdinalIgnoreCase) && date.ToDateTime(TimeOnly.MinValue) <= ps.EndDate)
                        .ToList();


                    var result = new ViewProfessorScheduleResponse
                    {
                        Date = date.ToString("dddd dd-MM-yy"),
                        TimeEachSlots = new List<TimeEachSlot>()
                    };

                    foreach (var schedule in schedulesForDate)
                    {
                        var timeSlots = await _unitOfWork.TimeSlotRepository.GetByProfessorScheduleIdAsync(schedule.ProfessorScheduleId);

                        foreach (var timeSlot in timeSlots)
                        {
                            result.TimeEachSlots.Add(new TimeEachSlot
                            {
                                StartTime = timeSlot.StartTime.ToString(), // Format: "HH:mm"
                                EndTime = timeSlot.EndTime.ToString()// Format: "HH:mm"
                            });
                        }
                    }


                    return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            public async Task<IBusinessResult> GetFilteredProfessors(FilterProfessorRequest request)
            {
                try
                {
                    var getAllProfessor = _unitOfWork.ProfessorRepository.GetAll();
                    var result = new List<GetAllProfessorReponse>();
                    var currentDate = DateTime.Now;
                    var currentDayOfWeek = currentDate.DayOfWeek;

                    foreach (var item in getAllProfessor)
                    {
                        var professor = new GetAllProfessorReponse();
                        var professorInfor = await _unitOfWork.AccountRepository.GetProfessorByAccountIDAsync(item.AccountId);

                        professor.ProfessorName = professorInfor.FullName;
                        professor.ProfessorId = professorInfor.Professor.ProfessorId;
                        professor.Major = professorInfor.Professor.Knowledge;
                        professor.Rating = (decimal)professorInfor.Professor.Rating;

                        // Fetch the professor's schedule
                        var professorSchedules = await _unitOfWork.ProfessorScheduleRepository.GetByProfessorIdAsync(professor.ProfessorId);

                        // Find the nearest schedule
                        var nearestSchedule = professorSchedules
                            .Where(ps => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), ps.DayOfWeek, true) >= currentDayOfWeek)
                            .OrderBy(ps => ps.DayOfWeek)
                            .FirstOrDefault();

                        if (nearestSchedule != null)
                        {
                            var timeSlots = await _unitOfWork.TimeSlotRepository.GetByProfessorScheduleIdAsync(nearestSchedule.ProfessorScheduleId);

                            if (timeSlots.Any())
                            {
                                var daysUntilNearestDay = ((DayOfWeek)Enum.Parse(typeof(DayOfWeek), nearestSchedule.DayOfWeek, true) - DateTime.Now.DayOfWeek + 7) % 7;
                                var nearestDate = DateTime.Now.AddDays(daysUntilNearestDay);

                                var nearestTimeSlot = timeSlots.OrderBy(ts => ts.StartTime).First();
                                professor.Date = nearestDate.ToString("dd-MM-yyyy");
                                professor.DateTime = $"{nearestSchedule.DayOfWeek}/{nearestTimeSlot.StartTime}-{nearestTimeSlot.EndTime}";
                            }
                        }

                        // Only add the professor to the result if DateTime is not null or empty
                        if (!string.IsNullOrEmpty(professor.DateTime))
                        {
                            result.Add(professor);
                        }
                    }

                    // Apply filters
                    if (!string.IsNullOrEmpty(request.NameSortOrder))
                    {
                        result = request.NameSortOrder.ToLower() == "asc"
                            ? result.OrderBy(p => p.ProfessorName).ToList()
                            : result.OrderByDescending(p => p.ProfessorName).ToList();
                    }

                    if (request.DayOfWeekFilter != null && request.DayOfWeekFilter.Any())
                    {
                        result = result
                            .Where(p => p.DateTime != null && request.DayOfWeekFilter.Contains(p.DateTime.Split('/')[0], StringComparer.OrdinalIgnoreCase))
                            .ToList();
                    }

                    if (request.TimeOfDateFilter != null && request.TimeOfDateFilter.Any())
                    {
                        var filteredResult = new List<GetAllProfessorReponse>();

                        foreach (var timeRange in request.TimeOfDateFilter)
                        {
                            var timeRangeParts = timeRange.Split('-');
                            if (timeRangeParts.Length == 2 && TimeSpan.TryParse(timeRangeParts[0], out var startTime) && TimeSpan.TryParse(timeRangeParts[1], out var endTime))
                            {
                                var matchingProfessors = result
                                    .Where(p => p.DateTime != null)
                                    .Where(p =>
                                    {
                                        var timeSlot = p.DateTime.Split('/')[1].Split('-');
                                        if (timeSlot.Length == 2 && TimeSpan.TryParse(timeSlot[0], out var slotStartTime) && TimeSpan.TryParse(timeSlot[1], out var slotEndTime))
                                        {
                                            return slotStartTime >= startTime && slotEndTime <= endTime;
                                        }
                                        return false;
                                    })
                                    .ToList();

                                filteredResult.AddRange(matchingProfessors);
                            }
                        }

                        result = filteredResult.Distinct().ToList(); // Ensure no duplicates
                    }

                    if (!string.IsNullOrEmpty(request.RatingSortOrder))
                    {
                        result = request.RatingSortOrder.ToLower() == "asc"
                            ? result.OrderBy(p => p.Rating).ToList()
                            : result.OrderByDescending(p => p.Rating).ToList();
                    }

                    return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

        public async Task<IBusinessResult> GetProfessorSchedule(int accountId, string type)
        {
            try
            {
                // Fetch all appointments for the given elderlyId

                var getElderly  = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(accountId);
                var appointments = await _unitOfWork.ProfessorAppointmentRepository
                    .GetByElderlyIdAsync(getElderly.Elderly.ElderlyId, type);

                var result = new List<GetProfessorScheduleOfElderly>();

                foreach (var appointment in appointments)
                {
                    // Fetch the professor's name from the Account table
                    var professor = await _unitOfWork.ProfessorRepository
                        .GetByIdAsync(appointment.TimeSlot.ProfessorSchedule.ProfessorId);

                    var professorAccount = await _unitOfWork.AccountRepository
                        .GetByIdAsync(professor.AccountId);

                    // Format the response
                    var schedule = new GetProfessorScheduleOfElderly
                    {
                        ProfessorName = professorAccount.FullName,
                        DateTime = $"{appointment.AppointmentTime:dd/MM/yyyy HH:mm}",
                        Status = appointment.Status
                    };

                    result.Add(schedule);
                }

              
                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result.OrderBy(p=>p.Status));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
    
}
