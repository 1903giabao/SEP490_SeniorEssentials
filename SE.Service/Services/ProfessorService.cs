﻿using AutoMapper;
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
using SE.Common.DTO;
using Firebase.Auth;
using SE.Service.Helper;
using Microsoft.Identity.Client;
using SE.Common.Request.Subscription;
using Google.Api.Gax;
using TagLib.Ape;
using Org.BouncyCastle.Ocsp;

namespace SE.Service.Services
{
    public interface IProfessorService
    {
        Task<IBusinessResult> GetNumberOfMeetingLeftByElderly(int elderlyId);
        Task<IBusinessResult> GetListElderlyByProfessorId(int professorId);
        Task<IBusinessResult> CreateSchedule(ProfessorScheduleRequest req);
        Task<IBusinessResult> UpdateSchedule(ProfessorScheduleRequest req);
        Task<IBusinessResult> AddProfessorToSubscriptionByElderly(AddProfessorToSubscriptionRequest req);
        Task<IBusinessResult> GetAllProfessor();
        Task<IBusinessResult> GetProfessorDetail(int professorId);
        Task<IBusinessResult> GetTimeSlot(int professorId, DateOnly date);
        Task<IBusinessResult> GetFilteredProfessors(FilterProfessorRequest request);
        Task<IBusinessResult> GetProfessorSchedule(int accountId, string type);
        Task<IBusinessResult> GetReportInAppointment(int appointmentId);
        Task<IBusinessResult> GetProfessorDetailOfElderly(int elderlyId);
        Task<IBusinessResult> GetProfessorDetailByAccountId(int accountId);
        Task<IBusinessResult> UpdateProfessorInfor(UpdateProfessorRequest req);
        Task<IBusinessResult> GetScheduleOfElderlyByProfessorId(int professorAccountId);
        Task<IBusinessResult> GetProfessorWeeklyTimeSlots(int accountId);
        Task<IBusinessResult> GetProfessorScheduleInProfessor(int professorId);
        Task<IBusinessResult> BookProfessorAppointment(BookProfessorAppointmentRequest req);
        Task<IBusinessResult> CreateAppointmentReport(CreateReportRequest request);
        Task<IBusinessResult> GiveProfessorFeedbackByAccount(GiveProfessorFeedbackByAccountVM request);
        Task<IBusinessResult> GetAllRatingsByProfessorId(int professorId);
        Task<IBusinessResult> CancelMeeting(int appointmentId);
        Task<IBusinessResult> ConfirmMeeting(int appointmentId);


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

        public async Task<IBusinessResult> CancelMeeting(int appointmentId)
        {
            try
            {
                var appointment = await _unitOfWork.ProfessorAppointmentRepository.GetByIdAsync(appointmentId);
                if (appointment == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Appointment not found.");
                }

                // Kiểm tra thời gian hủy có trước 6 tiếng so với thời gian hẹn không
                var currentTime = DateTime.UtcNow.AddHours(7);
                var timeDifference = appointment.AppointmentTime - currentTime;

                if (timeDifference.TotalHours < 6)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG,
                        "Chỉ có thể hủy lịch trước 6 tiếng");
                }

                // Kiểm tra nếu cuộc hẹn đã bị hủy rồi
                if (appointment.Status == SD.ProfessorAppointmentStatus.CANCELLED)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Appointment already cancelled.");
                }

                appointment.Status = SD.ProfessorAppointmentStatus.CANCELLED;
                var rs = await _unitOfWork.ProfessorAppointmentRepository.UpdateAsync(appointment);
                if (rs < 1)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Failed to cancel");
                }
                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, "Cancel successfully");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, ex.Message);
            }
        }

        public async Task<IBusinessResult> ConfirmMeeting(int appointmentId)
        {
            try
            {
                var appointment = await _unitOfWork.ProfessorAppointmentRepository.GetByIdAsync(appointmentId);
                if (appointment == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Appointment not found.");
                }

                appointment.Status = SD.ProfessorAppointmentStatus.JOINED;
                var rs = await _unitOfWork.ProfessorAppointmentRepository.UpdateAsync(appointment);
                if (rs < 1)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Failed to cancel");
                }
                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, "Confirm joined successfully");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, ex.Message);
            }
        }


        public async Task<IBusinessResult> GetNumberOfMeetingLeftByElderly(int elderlyId)
        {
            try
            {
                var elderlyAccount = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(elderlyId);

                if (elderlyAccount == null || elderlyAccount.RoleId != 2)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                var bookings = _unitOfWork.BookingRepository
                                .FindByCondition(b => b.ElderlyId == elderlyAccount.Elderly.ElderlyId && b.Status.Equals(SD.BookingStatus.PAID))
                                .Select(b => b.BookingId)
                                .ToList();

                if (!bookings.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Bookings of elderly not found.");
                }

                var userSubscription = await _unitOfWork.UserServiceRepository.GetUserSubscriptionByBookingIdAsync(bookings, SD.GeneralStatus.ACTIVE);

                if (userSubscription?.Booking == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Booking details not found.");
                }

                var result = userSubscription.NumberOfMeetingLeft;

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, ex.Message);
            }
        }

        public async Task<IBusinessResult> GetListElderlyByProfessorId(int professorId)
        {
            try
            {
                var getProfessorInfor = await _unitOfWork.AccountRepository.GetProfessorByAccountIDAsync(professorId);

                if (getProfessorInfor == null || getProfessorInfor.RoleId != 4)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Professor does not exist!");
                }

                var listUserElderly = await _unitOfWork.UserServiceRepository.GetListElderlyByProfessorId(getProfessorInfor.Professor.ProfessorId);

                var result = _mapper.Map<List<AccountElderlyDTO>>(listUserElderly);

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, ex.Message);
            }
        }        
        
        public async Task<IBusinessResult> CreateAppointmentReport(CreateReportRequest request)
        {
            try
            {
                var getAppointment =await _unitOfWork.ProfessorAppointmentRepository.GetByIdAsync(request.ProfessorAppointmentId);
                if (getAppointment == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Cannot find appointment");
                }

                getAppointment.Content = request.Content;
                getAppointment.Solution = request.Solution;

                var rs = await _unitOfWork.ProfessorAppointmentRepository.UpdateAsync(getAppointment);
                if (rs < 1)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Cannot create report");

                }
                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, "Created report succesfully!");


            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, ex.Message);
            }
        }

        public async Task<IBusinessResult> GetAllRatingsByProfessorId(int professorId)
        {
            try
            {
                var ratings = new List<GetProfessorRatingVM>();
                ratings = _unitOfWork.ProfessorRatingRepository
                    .FindByCondition(r => r.ProfessorId == professorId && r.Status == SD.GeneralStatus.ACTIVE)
                    .Select(r => new GetProfessorRatingVM
                    {
                        CreatedBy = r.CreatedBy,
                        Content = r.RatingComment,
                        Star = (int)r.Rating
                    })
                    .ToList();

               
                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, ratings);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<IBusinessResult> AddProfessorToSubscriptionByElderly(AddProfessorToSubscriptionRequest req)
        {
            try
            {
                var getProfessorInfor = await _unitOfWork.AccountRepository.GetProfessorByAccountIDAsync(req.ProfessorId);

                if (getProfessorInfor == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Professor does not exist!");
                }

                var elderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(req.ElderlyId);

                if (elderly == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Professor does not exist!");
                }

                var bookings = _unitOfWork.BookingRepository.FindByCondition(b => b.ElderlyId == elderly.Elderly.ElderlyId && b.Status.Equals(SD.BookingStatus.PAID))
                                                            .Select(b => b.BookingId).ToList();

                if (bookings.Any())
                {
                    var userSubscription = await _unitOfWork.UserServiceRepository.GetUserSubscriptionByBookingIdAsync(bookings, SD.GeneralStatus.ACTIVE);

                    if (userSubscription.ProfessorId != null)
                    {                        
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly already had professor!");
                    }

                    userSubscription.ProfessorId = getProfessorInfor.Professor.ProfessorId;

                    var updateRs = await _unitOfWork.UserServiceRepository.UpdateAsync(userSubscription);

                    if (updateRs < 1) 
                    {
                        return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                    }

                    return new BusinessResult(Const.SUCCESS_UPDATE, Const.SUCCESS_UPDATE_MSG);
                }

                return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, ex.Message);
            }
        }

      
        public async Task<IBusinessResult> CreateSchedule(ProfessorScheduleRequest req)

        {
            try
            {
                // Check if request is valid
                if (req == null || req.ListTime == null || !req.ListTime.Any())
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Invalid schedule data provided");
                }

                var accountProfessor = await _unitOfWork.AccountRepository.GetProfessorByAccountIDAsync(req.ProfessorId);

                if (accountProfessor == null || accountProfessor.RoleId != 4)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Professor doesn't exist!");
                }

                // Verify professor exists
                var professor = _unitOfWork.ProfessorRepository
                    .FindByCondition(p => p.ProfessorId == accountProfessor.Professor.ProfessorId).FirstOrDefault();

                if (professor == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Professor doesn't exist!");
                }

                var scheduleCreateList = new List<ProfessorSchedule>();
                var timeSlotCreateList = new List<TimeSlot>();

                foreach (var timeReq in req.ListTime)
                {
                    // Create the schedule for each day
                    var schedule = new ProfessorSchedule
                    {
                        ProfessorId = professor.ProfessorId,
                        DayOfWeek = timeReq.DayOfWeek, // Convert DateOnly to day name (e.g., "Monday")
                        StartDate = DateTime.UtcNow.AddHours(7), // Set these as needed
                        EndDate = null,
                        Status = SD.GeneralStatus.ACTIVE
                    };

                    // Parse the time range
                    if (TimeOnly.TryParse(timeReq.StartTime, out var startTime) &&
                        TimeOnly.TryParse(timeReq.EndTime, out var endTime))
                    {
                        // Create 1-hour time slots between start and end time
                        var currentSlotStart = startTime;

                        while (currentSlotStart < endTime)
                        {
                            var currentSlotEnd = currentSlotStart.AddHours(1);
                            // Ensure we don't go past the end time
                            if (currentSlotEnd > endTime)
                            {
                                currentSlotEnd = endTime;
                            }

                            timeSlotCreateList.Add(new TimeSlot
                            {
                                ProfessorSchedule = schedule,
                                StartTime = currentSlotStart,
                                EndTime = currentSlotEnd,
                                Status = SD.GeneralStatus.ACTIVE,
                                Note = $"Auto-generated slot for {timeReq.DayOfWeek:dddd}"
                            });

                            currentSlotStart = currentSlotEnd;
                        }
                    }

                    scheduleCreateList.Add(schedule);
                }

                // Begin transaction

                try
                {
                    // Create schedules
                    var scheduleResult = await _unitOfWork.ProfessorScheduleRepository.CreateRangeAsync(scheduleCreateList);

                    if (scheduleResult <= 0)
                    {
                        return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
                    }

                    // Create time slots
                    var timeSlotResult = await _unitOfWork.TimeSlotRepository.CreateRangeAsync(timeSlotCreateList);

                    if (timeSlotResult <= 0)
                    {
                        return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
                    }

                    // Commit transaction

                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, new
                    {
                        ProfessorId = professor.ProfessorId,
                        SchedulesCreated = scheduleResult,
                        TimeSlotsCreated = timeSlotResult
                    });
                }
                catch (Exception ex)
                {
                    throw ex;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<IBusinessResult> GiveProfessorFeedbackByAccount(GiveProfessorFeedbackByAccountVM request)
        {
            try
            {
                // Get the appointment by ID
                var appointment =  _unitOfWork.ProfessorAppointmentRepository
                    .FindByCondition(a => a.ProfessorAppointmentId == request.AppointmentId)
                    .FirstOrDefault();
                var rs = 0;
                if (appointment == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Cuộc hẹn không tồn tại!");
                }

                // Get TimeSlot
                var timeSlot = _unitOfWork.TimeSlotRepository
                    .FindByCondition(t => t.TimeSlotId == appointment.TimeSlotId)
                    .FirstOrDefault();

                if (timeSlot == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Khung giờ không tồn tại!");
                }

                // Get ProfessorSchedule
                var professorSchedule =  _unitOfWork.ProfessorScheduleRepository
                    .FindByCondition(ps => ps.ProfessorScheduleId == timeSlot.ProfessorScheduleId)
                    .FirstOrDefault();

                if (professorSchedule == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Lịch làm việc không tồn tại!");
                }

                var professorId = professorSchedule.ProfessorId;
                var elderlyId = appointment.ElderlyId;

                // Create feedback
                var feedback = new ProfessorRating
                {
                    ProfessorId = professorId,
                    ElderlyId = elderlyId,
                    RatingComment = request.Content,
                    Rating = request.Star,
                    Status = SD.GeneralStatus.ACTIVE,
                    CreatedBy = request.CreatedBy,
                    ProfessorAppointmentId = request.AppointmentId
                };

                await _unitOfWork.ProfessorRatingRepository.CreateAsync(feedback);

                // Update professor average rating
                var allRatings = _unitOfWork.ProfessorRatingRepository
                    .FindByCondition(r => r.ProfessorId == professorId && r.Status == SD.GeneralStatus.ACTIVE)
                    .ToList();

                if (allRatings.Any())
                {
                    var averageRating = allRatings.Average(r => r.Rating);
                    var professor =  _unitOfWork.ProfessorRepository
                        .FindByCondition(p => p.ProfessorId == professorId)
                        .FirstOrDefault();

                    if (professor != null)
                    {
                        professor.Rating = averageRating;
                        rs = await _unitOfWork.ProfessorRepository.UpdateAsync(professor);
                    }
                }


                if (rs > 0)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, "Create succesffuly");
                }

                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Fail to create");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<IBusinessResult> UpdateSchedule(ProfessorScheduleRequest req)
        {
            try
            {
                // Check if request is valid
                if (req == null || req.ListTime == null || !req.ListTime.Any())
                {
                    return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG, "Invalid schedule data provided");
                }

                // Verify professor exists
                var accountProfessor = await _unitOfWork.AccountRepository.GetProfessorByAccountIDAsync(req.ProfessorId);

                if (accountProfessor == null || accountProfessor.RoleId != 4)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Professor doesn't exist!");
                }

                // Verify professor exists
                var professor = _unitOfWork.ProfessorRepository
                    .FindByCondition(p => p.ProfessorId == accountProfessor.Professor.ProfessorId).FirstOrDefault();

                if (professor == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Professor doesn't exist!");
                }

                // Get existing schedules for this professor
                var existingSchedules = await _unitOfWork.ProfessorScheduleRepository
                    .FindByConditionAsync(s => s.ProfessorId == professor.ProfessorId && s.Status == SD.GeneralStatus.ACTIVE);

                var existingTimeSlots = await _unitOfWork.TimeSlotRepository.GetByAndContainProfessorScheduleIdAsync(existingSchedules.ToList(), SD.GeneralStatus.ACTIVE);

                // Convert request times to comparable format
                var requestTimes = req.ListTime.Select(t => new
                {
                    DayOfWeek = t.DayOfWeek,
                    StartTime = TimeOnly.Parse(t.StartTime),
                    EndTime = TimeOnly.Parse(t.EndTime)
                }).ToList();

                // Convert existing schedules to comparable format
                var existingScheduleGroups = existingTimeSlots
                    .GroupBy(t => t.ProfessorScheduleId)
                    .SelectMany(g => g.Select(t => new
                    {
                        ProfessorScheduleId = g.Key,
                        DayOfWeek = existingSchedules.First(s => s.ProfessorScheduleId == g.Key).DayOfWeek,
                        StartTime = t.StartTime,
                        EndTime = t.EndTime,
                        Status = t.Status
                    }))
                    .ToList();

                // Identify schedules to add, keep, and remove
                var schedulesToAdd = requestTimes
                    .Where(rt => !existingScheduleGroups.Any(es =>
                        es.DayOfWeek == rt.DayOfWeek &&
                        es.Status.Equals(SD.GeneralStatus.ACTIVE) &&
                        rt.StartTime == es.StartTime &&
                        rt.EndTime == es.EndTime))
                    .ToList();

                var schedulesToKeep = existingScheduleGroups
                    .Where(es => requestTimes.Any(rt =>
                        rt.DayOfWeek == es.DayOfWeek &&
                        rt.StartTime == es.StartTime &&
                        rt.EndTime == es.EndTime))
                    .ToList();

                var schedulesToRemove = existingScheduleGroups
                    .Where(es => !requestTimes.Any(rt =>
                        rt.DayOfWeek == es.DayOfWeek))
                    .ToList();

                // Create new schedules and time slots (similar to CreateSchedule)
                var scheduleCreateList = new List<ProfessorSchedule>();
                var timeSlotCreateList = new List<TimeSlot>();

                foreach (var timeReq in schedulesToAdd)
                {
                    var existedDayOfWeek = existingSchedules.Where(ps => ps.DayOfWeek.Equals(timeReq.DayOfWeek) && ps.Status.Equals(SD.GeneralStatus.ACTIVE)).FirstOrDefault();

                    if (existedDayOfWeek == null)
                    {
                        var schedule = new ProfessorSchedule
                        {
                            ProfessorId = professor.ProfessorId,
                            DayOfWeek = timeReq.DayOfWeek,
                            StartDate = DateTime.UtcNow.AddHours(7),
                            EndDate = null,
                            Status = SD.GeneralStatus.ACTIVE
                        };

                        var currentSlotStart = timeReq.StartTime;
                        while (currentSlotStart < timeReq.EndTime)
                        {
                            var currentSlotEnd = currentSlotStart.AddHours(1);
                            if (currentSlotEnd > timeReq.EndTime)
                            {
                                currentSlotEnd = timeReq.EndTime;
                            }

                            timeSlotCreateList.Add(new TimeSlot
                            {
                                ProfessorSchedule = schedule,
                                StartTime = currentSlotStart,
                                EndTime = currentSlotEnd,
                                Status = SD.GeneralStatus.ACTIVE,
                                Note = $"Auto-generated slot for {timeReq.DayOfWeek}"
                            });
                            currentSlotStart = currentSlotEnd;
                        }

                        scheduleCreateList.Add(schedule);
                    }
                    else
                    {
                        var currentSlotStart = timeReq.StartTime;
                        while (currentSlotStart < timeReq.EndTime)
                        {
                            var currentSlotEnd = currentSlotStart.AddHours(1);
                            if (currentSlotEnd > timeReq.EndTime)
                            {
                                currentSlotEnd = timeReq.EndTime;
                            }

                            if (!existingTimeSlots.Where(et => et.StartTime == currentSlotStart && et.EndTime == currentSlotEnd).Any())
                            {
                                timeSlotCreateList.Add(new TimeSlot
                                {
                                    ProfessorScheduleId = existedDayOfWeek.ProfessorScheduleId,
                                    StartTime = currentSlotStart,
                                    EndTime = currentSlotEnd,
                                    Status = SD.GeneralStatus.ACTIVE,
                                    Note = $"Auto-generated slot for {timeReq.DayOfWeek}"
                                });
                            }
                            currentSlotStart = currentSlotEnd;
                        }
                    }
                }

                // Mark schedules to remove as inactive (soft delete)
                var schedulesToRemoveIds = schedulesToRemove.Select(s => s.ProfessorScheduleId).ToList();
                var schedulesToDeactivate = existingSchedules
                    .Where(s => schedulesToRemoveIds.Contains(s.ProfessorScheduleId))
                    .ToList();

                foreach (var schedule in schedulesToDeactivate)
                {
                    schedule.Status = SD.GeneralStatus.INACTIVE;
                    schedule.EndDate = DateTime.UtcNow.AddHours(7);
                }

                try
                {
                    // Create new schedules
                    int scheduleResult = 0;
                    if (scheduleCreateList.Any())
                    {
                        scheduleResult = await _unitOfWork.ProfessorScheduleRepository.CreateRangeAsync(scheduleCreateList);
                        if (scheduleResult <= 0)
                        {
                            return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
                        }
                    }

                    // Create new time slots
                    int timeSlotResult = 0;
                    if (timeSlotCreateList.Any())
                    {
                        timeSlotResult = await _unitOfWork.TimeSlotRepository.CreateRangeAsync(timeSlotCreateList);
                        if (timeSlotResult <= 0 && timeSlotCreateList.Any())
                        {
                            return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
                        }
                    }

                    // Update schedules to deactivate
                    int deactivateResult = 0;
                    if (schedulesToDeactivate.Any())
                    {
                        foreach (var schedule in schedulesToDeactivate)
                        {
                            var existingSchedule = await _unitOfWork.ProfessorScheduleRepository.GetByIdAsync(schedule.ProfessorScheduleId);
                            if (existingSchedule != null)
                            {
                                existingSchedule.Status = schedule.Status;

                                var result = await _unitOfWork.ProfessorScheduleRepository.UpdateAsync(existingSchedule);
                                if (result <= 0)
                                {
                                    return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                                }
                            }
                        }
                    }

                    // Convert request times to TimeOnly for comparison
                    var requestTimeSlots = req.ListTime.Select(rt => new {
                        DayOfWeek = rt.DayOfWeek,
                        StartTime = TimeOnly.Parse(rt.StartTime),
                        EndTime = TimeOnly.Parse(rt.EndTime)
                    }).ToList();

                    // Find time slots to inactivate (exist in DB but not in request)
                    var timeslotsToInactivate = timeSlotCreateList
                        .Where(ets =>
                            ets.Status == SD.GeneralStatus.ACTIVE &&  // Only consider active slots
                            !timeSlotCreateList.Any(rts =>
                                /*rts.DayOfWeek == ets.ProfessorSchedule.DayOfWeek &&*/
                                rts.StartTime == ets.StartTime &&
                                rts.EndTime == ets.EndTime))
                        .ToList();

                    if (timeslotsToInactivate.Any())
                    {
                        timeslotsToInactivate = timeslotsToInactivate.Select(ts => { ts.Status = SD.GeneralStatus.INACTIVE; return ts; } ).ToList();
                        deactivateResult = await _unitOfWork.TimeSlotRepository.UpdateRangeAsync(timeslotsToInactivate);
                        if (deactivateResult <= 0 && schedulesToDeactivate.Any())
                        {
                            return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                        }
                    }

                    return new BusinessResult(Const.SUCCESS_UPDATE, Const.SUCCESS_UPDATE_MSG, new
                    {
                        ProfessorId = professor.ProfessorId,
                        SchedulesAdded = scheduleResult,
                        TimeSlotsAdded = timeSlotResult,
                        SchedulesRemoved = deactivateResult
                    });
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<IBusinessResult> GetAllProfessor()
        {
            try
            {
                var getAllProfessor = _unitOfWork.ProfessorRepository.FindByCondition(p=>p.Status.Equals(SD.GeneralStatus.ACTIVE));
                var result = new List<GetAllProfessorReponse>();
                var currentDate = DateTime.Now;
                var currentDayOfWeek = currentDate.DayOfWeek;

                foreach (var item in getAllProfessor)
                {
                    var professor = new GetAllProfessorReponse();
                    var professorInfor = await _unitOfWork.AccountRepository.GetProfessorByAccountIDAsync(item.AccountId);

                    professor.ProfessorAvatar = professorInfor.Avatar;
                    professor.ProfessorName = professorInfor.FullName;
                    professor.ProfessorId = professorInfor.Professor.ProfessorId;
                    professor.Major = professorInfor.Professor.Knowledge;
                    professor.Rating = (decimal)professorInfor.Professor.Rating;
                    professor.TotalRating = 0;

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


                    result.Add(professor);

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

        public async Task<IBusinessResult> GetProfessorDetailByAccountId(int accountId)
        {
            try
            {
                var getProfessorInfor = await _unitOfWork.AccountRepository.GetProfessorByAccountIDAsync(accountId);
                if (getProfessorInfor == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Professor does not existed!");
                }
                var professor = _mapper.Map<GetProfessorDetail>(getProfessorInfor.Professor);

                professor.ProfessorId = getProfessorInfor.Professor.ProfessorId;
                professor.FullName = getProfessorInfor.FullName;
                professor.Avatar = getProfessorInfor.Avatar;

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, professor);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, ex.Message);
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

                // Fetch all appointments for the given date
                var appointmentsOnDate = await _unitOfWork.ProfessorAppointmentRepository.GetByDateAsync(date);

                foreach (var schedule in schedulesForDate)
                {
                    var timeSlots = await _unitOfWork.TimeSlotRepository.GetByProfessorScheduleIdAsync(schedule.ProfessorScheduleId);

                    foreach (var timeSlot in timeSlots)
                    {
                        // Check if the time slot is booked
                        var isBooked = appointmentsOnDate.Any(a => a.TimeSlotId == timeSlot.TimeSlotId);

                        // If the time slot is not booked, add it to the result
                        if (!isBooked)
                        {
                            result.TimeEachSlots.Add(new TimeEachSlot
                            {
                                TimeSlotId = timeSlot.TimeSlotId,
                                StartTime = timeSlot.StartTime.ToString(),
                                EndTime = timeSlot.EndTime.ToString()
                            });
                        }
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
                    professor.ProfessorAvatar = professorInfor.Avatar;
                    professor.ProfessorId = professorInfor.Professor.ProfessorId;
                    professor.Major = professorInfor.Professor.Knowledge;
                    professor.Rating = professorInfor.Professor == null ? 0 : (decimal)professorInfor.Professor.Rating;
                    professor.TotalRating = 0;

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

                            // Convert TimeOnly? to TimeSpan
                            var startTimeSpan = nearestTimeSlot.StartTime.HasValue
                                ? new TimeSpan(nearestTimeSlot.StartTime.Value.Hour, nearestTimeSlot.StartTime.Value.Minute, 0)
                                : TimeSpan.Zero;

                            var endTimeSpan = nearestTimeSlot.EndTime.HasValue
                                ? new TimeSpan(nearestTimeSlot.EndTime.Value.Hour, nearestTimeSlot.EndTime.Value.Minute, 0)
                                : TimeSpan.Zero;

                            // Format time in 24-hour format
                            var startTime24H = DateTime.Today.Add(startTimeSpan).ToString("HH:mm");
                            var endTime24H = DateTime.Today.Add(endTimeSpan).ToString("HH:mm");

                            // Assign the formatted time to DateTime property
                            professor.DateTime = $"{nearestSchedule.DayOfWeek}/{startTime24H}-{endTime24H}";
                        }
                    }

                    result.Add(professor);
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
                                    var timeSlotPart = p.DateTime.Split('/')[1]; // Get the time part (e.g., "15:00-16:00")
                                    var timeSlotParts = timeSlotPart.Split('-');
                                    if (timeSlotParts.Length == 2 && TimeSpan.TryParse(timeSlotParts[0], out var slotStartTime) && TimeSpan.TryParse(timeSlotParts[1], out var slotEndTime))
                                    {
                                        // Check if the time slot overlaps with the filter range
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
                var getElderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(accountId);
                var appointments = await _unitOfWork.ProfessorAppointmentRepository
                    .GetByElderlyIdAsync(getElderly.Elderly.ElderlyId, type);

                var result = new List<GetProfessorScheduleOfElderly>();

                foreach (var appointment in appointments)
                {
                    var peopleInAppointment = new List<PeopleOfSchedule>();

                    var professor = await _unitOfWork.ProfessorRepository
                        .GetByIdAsync(appointment.TimeSlot.ProfessorSchedule.ProfessorId);

                    var professorAccount = await _unitOfWork.AccountRepository
                        .GetByIdAsync(professor.AccountId);

                    // Check if there is a report (Content in ProfessorAppointment)
                    bool isReport = !string.IsNullOrEmpty(appointment.Content);

                    // Check if feedback exists for this appointment
                    bool isFeedback = _unitOfWork.ProfessorRatingRepository
                        .FindByCondition(r => r.ProfessorAppointmentId == appointment.ProfessorAppointmentId)
                        .Any();

                    var schedule = new GetProfessorScheduleOfElderly
                    {
                        ProfessorName = professorAccount.FullName,
                        DateTime = $"{appointment.AppointmentTime:dd/MM/yyyy HH:mm}",
                        Status = appointment.Status,
                        ProfessorAppointmentId = appointment.ProfessorAppointmentId,
                        ProfessorAvatar = professorAccount.Avatar,
                        IsOnline = (bool)appointment.IsOnline,
                        IsReport = isReport,   // Set IsReport
                        IsFeedback = isFeedback,  // Set IsFeedback
                        People = new List<PeopleOfSchedule>()
                    };

                    if (schedule.Status == SD.ProfessorAppointmentStatus.NOTYET)
                    {
                        // Add professor
                        peopleInAppointment.Add(new PeopleOfSchedule
                        {
                            Id = professorAccount.AccountId,
                            Name = professorAccount.FullName
                        });

                        // Add elderly
                        var elderlyId = await _unitOfWork.ElderlyRepository.GetAccountByElderlyId(appointment.ElderlyId);
                        peopleInAppointment.Add(new PeopleOfSchedule
                        {
                            Id = elderlyId.Account.AccountId,
                            Name = elderlyId.Account.FullName
                        });

                        // Add family members
                        var familyMemberAccountIds = await GetAllFamilyMemberByElderlyId(elderlyId.AccountId);
                        var familyMembers = _unitOfWork.AccountRepository.GetAll()
                            .Where(a => familyMemberAccountIds.Contains(a.AccountId))
                            .Select(a => new PeopleOfSchedule
                            {
                                Id = a.AccountId,
                                Name = a.FullName
                            })
                            .ToList();

                        peopleInAppointment.AddRange(familyMembers);

                        schedule.People.AddRange(peopleInAppointment);
                    }

                    result.Add(schedule);
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result.OrderBy(p => p.Status));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<IBusinessResult> GetProfessorScheduleInProfessor(int professorId)
        {
            try
            {
                var accountProfessor = await _unitOfWork.AccountRepository.GetProfessorByAccountIDAsync(professorId);

                if (accountProfessor == null || accountProfessor.RoleId != 4)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Professor doesn't exist!");
                }

                // Verify professor exists
                var professor = _unitOfWork.ProfessorRepository
                    .FindByCondition(p => p.ProfessorId == accountProfessor.Professor.ProfessorId).FirstOrDefault();

                if (professor == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Professor doesn't exist!");
                }

                // Get all schedules for this professor with their time slots
                var schedules = await _unitOfWork.ProfessorScheduleRepository
                    .GetProfessorIncludeTimeSlot(professor.ProfessorId);

                // Prepare days of week in order
                var daysOfWeek = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
                var result = new List<GetScheduleOfProfessorVM>();
                // Process each day of week
                foreach (var day in daysOfWeek)
                {
                    var newDay = new GetScheduleOfProfessorVM();
                    newDay.DayOfWeek = day;
                    var daySchedule = schedules.FirstOrDefault(s => s.DayOfWeek.Equals(day, StringComparison.OrdinalIgnoreCase));

                    if (daySchedule != null && daySchedule.TimeSlots.Any())
                    {
                        // Add each time slot individually (sorted by time)
                        foreach (var slot in daySchedule.TimeSlots.OrderBy(t => t.StartTime))
                        {
                            var newTime = new Time();
                            newTime.Start = slot.StartTime.ToString();
                            newTime.End = slot.EndTime.ToString();
                            newDay.Times.Add(newTime);
                        }
                    }
                    else
                    {
                        newDay.Times = [];
                    }
                    result.Add(newDay);
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, ex.Message);
            }
        }
        public async Task<List<int>> GetAllFamilyMemberByElderlyId(int userId)
        {

            var groupId = _unitOfWork.GroupMemberRepository.GetAll()
                .Where(gm => gm.AccountId == userId && gm.Status == SD.GeneralStatus.ACTIVE)
                .Select(gm => gm.GroupId)
                .FirstOrDefault();


            var group = await _unitOfWork.GroupRepository.GetByIdAsync(groupId);

            var groupMembers = _unitOfWork.GroupMemberRepository.GetAll()
                .Where(gm => gm.GroupId == groupId &&
                             gm.Status == SD.GeneralStatus.ACTIVE &&
                             gm.AccountId != userId)
                .Select(gm => gm.AccountId)
                .ToList();

            var users = _unitOfWork.AccountRepository.GetAll()
                     .Where(a => groupMembers.Contains(a.AccountId) && a.RoleId == 3)
                     .Select(a => a.AccountId)
                     .ToList();

            return users;
        }

       
        public async Task<IBusinessResult> GetProfessorDetailOfElderly(int elderlyId)
        {
            try
            {
                var getElderlyInfor = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(elderlyId);
                var getElderlyEntity = _unitOfWork.ElderlyRepository.FindByCondition(e => e.AccountId == elderlyId).FirstOrDefault();
                var findProfessorId = await _unitOfWork.UserServiceRepository.GetProfessorByElderlyId(getElderlyEntity.ElderlyId);

                var getProfessor = await _unitOfWork.ProfessorRepository.GetAccountByProfessorId((int)findProfessorId.ProfessorId);

                var getProfessorInfor = await _unitOfWork.AccountRepository.GetProfessorByAccountIDAsync(getProfessor.AccountId);


                // Map the professor details to the response model
                var professor = _mapper.Map<GetProfessorDetail>(getProfessor);

                // Set additional properties
                professor.ProfessorId = getProfessor.ProfessorId;
                professor.FullName = getProfessorInfor.FullName;
                professor.Avatar = getProfessorInfor.Avatar;
                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, professor);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<IBusinessResult> GetReportInAppointment(int appointmentId)
        {
            try
            {
                var getAppointment = _unitOfWork.ProfessorAppointmentRepository.GetById(appointmentId);
                var rs = new
                {
                    Content = (getAppointment.Content == null) ? "" : getAppointment.Content,
                    Solution = (getAppointment.Solution == null) ? "" : getAppointment.Solution
                };
                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, rs);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IBusinessResult> UpdateProfessorInfor(UpdateProfessorRequest req)
        {
            try
            {
                var getProfessorAccountInfor = await _unitOfWork.AccountRepository.GetProfessorByAccountIDAsync(req.AccountId);

                if (getProfessorAccountInfor == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Account does not exist");
                }

                var professor = await _unitOfWork.ProfessorRepository.GetByIdAsync(getProfessorAccountInfor.Professor.ProfessorId);

                if (professor == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Professor does not exist");
                }

                if (req.Avatar != null)
                {
                    var avatar = ("", "");

                    if (req.Avatar != null)
                    {
                        avatar = await CloudinaryHelper.UploadImageAsync(req.Avatar);
                    }

                    getProfessorAccountInfor.Avatar = avatar.Item2;

                    var updateAccountRs = await _unitOfWork.AccountRepository.UpdateAsync(getProfessorAccountInfor);

                    if (updateAccountRs < 1)
                    {
                        return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                    }
                }

                professor.Knowledge = req.Knowledge;
                professor.Achievement = req.Achievement;
                professor.ClinicAddress = req.ClinicAddress;
                professor.Career = req.Career;
                professor.ConsultationFee = req.ConsultationFee;
                professor.ExperienceYears = req.ExperienceYears;
                professor.Qualification = req.Qualification;
                professor.Specialization = req.Specialization;

                var updateRs = await _unitOfWork.ProfessorRepository.UpdateAsync(professor);

                if (updateRs < 1)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                }

                var professorRs = _mapper.Map<GetProfessorDetail>(professor);

                professorRs.ProfessorId = getProfessorAccountInfor.Professor.ProfessorId;
                professorRs.FullName = getProfessorAccountInfor.FullName;
                professorRs.Avatar = getProfessorAccountInfor.Avatar;

                return new BusinessResult(Const.SUCCESS_UPDATE, Const.SUCCESS_UPDATE_MSG, professorRs);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, ex.Message);
            }
        }

        public async Task<IBusinessResult> GetScheduleOfElderlyByProfessorId(int professorAccountId)
        {
            try
            {
                // Get the professor by account ID
                var professor = await _unitOfWork.ProfessorRepository
                    .GetByAccountIdAsync(professorAccountId);

                if (professor == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG);
                }

                // Get all appointments for this professor
                var appointments = await _unitOfWork.ProfessorAppointmentRepository
                    .GetByProfessorIdAsync(professor.ProfessorId);

                var result = new List<GetScheduleOfElderlyByProfessorVM>();

                foreach (var appointment in appointments)
                {
                    var elderly = await _unitOfWork.ElderlyRepository
                        .GetByIdAsync(appointment.ElderlyId);

                    var elderlyAccount = await _unitOfWork.AccountRepository
                        .GetByIdAsync(elderly.AccountId);

                    // Check if there is a report (Content in ProfessorAppointment)
                    bool isReport = !string.IsNullOrEmpty(appointment.Content);

                    // Check if feedback exists for this appointment
                    bool isFeedback =  _unitOfWork.ProfessorRatingRepository
                        .FindByCondition(r => r.ProfessorAppointmentId == appointment.ProfessorAppointmentId)
                        .Any();

                    var schedule = new GetScheduleOfElderlyByProfessorVM
                    {
                        ElderlyId = elderly.ElderlyId,
                        ElderlyName = elderlyAccount.FullName,
                        Avatar = elderlyAccount.Avatar,
                        PhoneNumber = elderlyAccount.PhoneNumber,
                        DateTime = $"{appointment.AppointmentTime:dd/MM/yyyy HH:mm}",
                        Status = appointment.Status,
                        IsOnline = (bool)appointment.IsOnline,
                        IsReport = isReport,  // Set IsReport
                        IsFeedback = isFeedback,  // Set IsFeedback
                        People = new List<PeopleOfScheduleVM>()
                    };

                    // Add professor
                    schedule.People.Add(new PeopleOfScheduleVM
                    {
                        Id = professorAccountId,
                        Name = (await _unitOfWork.AccountRepository.GetByIdAsync(professorAccountId)).FullName
                    });

                    // Add elderly
                    schedule.People.Add(new PeopleOfScheduleVM
                    {
                        Id = elderly.AccountId,
                        Name = elderlyAccount.FullName
                    });

                    // Add family members
                    var familyMemberAccountIds = await GetAllFamilyMemberByElderlyId(elderly.AccountId);
                    var familyMembers = _unitOfWork.AccountRepository.GetAll()
                        .Where(a => familyMemberAccountIds.Contains(a.AccountId))
                        .Select(a => new PeopleOfScheduleVM
                        {
                            Id = a.AccountId,
                            Name = a.FullName
                        })
                        .ToList();

                    schedule.People.AddRange(familyMembers);

                    result.Add(schedule);
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result.OrderBy(p => p.Status));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IBusinessResult> GetProfessorWeeklyTimeSlots(int accountId)
        {
            try
            {
                // Get current date and calculate start (Monday) and end (Sunday) of current week
                var today = DateOnly.FromDateTime(DateTime.Now);
                var daysFromMonday = (int)today.DayOfWeek - (int)DayOfWeek.Monday;
                var monday = today.AddDays(-daysFromMonday);
                var sunday = monday.AddDays(6);

                var professorId = _unitOfWork.ProfessorRepository.FindByCondition(p=>p.AccountId == accountId).Select(p=>p.ProfessorId).FirstOrDefault();
                // Fetch all professor schedules
                var professorSchedules = await _unitOfWork.ProfessorScheduleRepository.GetByProfessorIdAsync(professorId);

                // Filter schedules that are active (EndDate is in future)
                var activeSchedules = professorSchedules
                    .Where(ps => ps.EndDate >= DateTime.Now.Date)
                    .ToList();

                var result = new List<ViewProfessorScheduleResponse>();

                // Loop through each day from Monday to Sunday
                for (var date = monday; date <= sunday; date = date.AddDays(1))
                {
                    var dayOfWeek = date.DayOfWeek.ToString();
                    var schedulesForDay = activeSchedules
                        .Where(ps => ps.DayOfWeek.Equals(dayOfWeek, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    var dayResult = new ViewProfessorScheduleResponse
                    {
                        Date = date.ToString("dddd dd-MM-yy"), // Format example: "Monday 01-04-24"
                        TimeEachSlots = new List<TimeEachSlot>()
                    };

                    // Get appointments for this date
                    var appointmentsOnDate = await _unitOfWork.ProfessorAppointmentRepository.GetByDateAsync(date);

                    foreach (var schedule in schedulesForDay)
                    {
                        var timeSlots = await _unitOfWork.TimeSlotRepository.GetByProfessorScheduleIdAsync(schedule.ProfessorScheduleId);

                        foreach (var timeSlot in timeSlots)
                        {
                            // Check if time slot is booked
                            var isBooked = appointmentsOnDate.Any(a => a.TimeSlotId == timeSlot.TimeSlotId);

                            if (!isBooked)
                            {
                                dayResult.TimeEachSlots.Add(new TimeEachSlot
                                {
                                    TimeSlotId = timeSlot.TimeSlotId,
                                    StartTime = timeSlot.StartTime.ToString(), // Format as "09:00"
                                    EndTime = timeSlot.EndTime.ToString()      // Format as "10:00"
                                });
                            }
                        }
                    }

                    // Always add the day to result, even if TimeEachSlots is empty
                    result.Add(dayResult);
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, ex.Message);
            }
        }

        public async Task<IBusinessResult> BookProfessorAppointment(BookProfessorAppointmentRequest req)
        {
            try
            {
                var accountElderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(req.ElderlyId);

                if (accountElderly == null || accountElderly.RoleId != 2)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly doesn't exist!");
                }

                // Verify professor exists
                var elderly = _unitOfWork.ElderlyRepository
                    .FindByCondition(p => p.ElderlyId == accountElderly.Elderly.ElderlyId).FirstOrDefault();

                if (elderly == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly doesn't exist!");
                }

                var bookings = _unitOfWork.BookingRepository
                    .FindByCondition(b => b.ElderlyId == elderly.ElderlyId && b.Status.Equals(SD.BookingStatus.PAID))
                    .Select(b => b.BookingId)
                    .ToList();

                if (!bookings.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Bookings of elderly not found.");
                }

                var userSubscription = await _unitOfWork.UserServiceRepository.GetUserSubscriptionByBookingIdAsync(bookings, SD.GeneralStatus.ACTIVE);

                if (userSubscription?.Booking == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Booking details not found.");
                }

                userSubscription.NumberOfMeetingLeft = userSubscription.NumberOfMeetingLeft - 1;

                var updateUserSubscriptionRs = await _unitOfWork.UserServiceRepository.UpdateAsync(userSubscription);

                if (updateUserSubscriptionRs < 1)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                }

                var professor = _unitOfWork.ProfessorRepository
                    .FindByCondition(p => p.ProfessorId == userSubscription.Professor.ProfessorId).FirstOrDefault();

                if (professor == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Professor doesn't exist!");
                }

                var timeslot = await _unitOfWork.TimeSlotRepository.GetByTimeSlotIdAsync(req.TimeSlotId);

                if (timeslot == null) 
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Time slot doesn't exist!");
                }

                var professorSchedule = timeslot.ProfessorSchedule;

                // Parse the date from req.Day
                DateTime appointmentDate;
                if (!DateTime.TryParse(req.Day, out appointmentDate))
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Invalid date format!");
                }

                // Check if date is before schedule start
                if (appointmentDate < professorSchedule.StartDate)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Date is before schedule start!");
                }

                // Check if day of week matches
                if (appointmentDate.DayOfWeek.ToString().ToLower() != professorSchedule.DayOfWeek.ToString().ToLower())
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Day doesn't match professor's schedule!");
                }

                var professorAppointment = new ProfessorAppointment
                {
                    ElderlyId = elderly.ElderlyId,
                    TimeSlotId = timeslot.TimeSlotId,
                    UserSubscriptionId = userSubscription.UserSubscriptionId,
                    AppointmentTime = (DateTime)(appointmentDate.Date + timeslot.StartTime?.ToTimeSpan()),
                    StartTime = timeslot.StartTime,
                    EndTime = timeslot.EndTime,
                    Description = req.Description == null ? "Nothing" : req.Description,
                    CreatedDate = DateTime.UtcNow.AddHours(7),
                    Status = SD.ProfessorAppointmentStatus.NOTYET,
                    IsOnline = true
                };

                var createRs = await _unitOfWork.ProfessorAppointmentRepository.CreateAsync(professorAppointment);

                if (createRs < 1)
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
                }

                return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG);

            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, ex.Message);
            }
        }
    }

}
