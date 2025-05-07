using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using SE.Common;
using SE.Common.DTO;
using SE.Common.Enums;
using SE.Common.Response.Subscription;
using SE.Data.Models;
using SE.Data.Repository;
using SE.Data.UnitOfWork;
using SE.Service.Base;

namespace SE.Service.Services
{
    public interface ISubscriptionService
    {
        Task<IBusinessResult> CreateCombo(CreateComboModel req);
        Task<IBusinessResult> UpdateCombo(int comboId, CreateComboModel req);
        Task<IBusinessResult> GetAllCombos();
        Task<IBusinessResult> GetComboById(int comboId);
        Task<IBusinessResult> UpdateComboStatus(int comboId, string status);
        Task<IBusinessResult> GetAllUserInCombo(int comboId);
        Task<IBusinessResult> BackSubscription(int userSubscriptionId);
        Task<IBusinessResult> CheckIfUserHasOneTimeSubscription(int elderlyId);
    }

    public class SubscriptionService : ISubscriptionService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public SubscriptionService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IBusinessResult> CreateCombo(CreateComboModel req)
        {
            try
            {
                if (req.Fee <= 0)
                {
                    return new BusinessResult(Const.FAIL_READ, "FEE MUST > 0");
                }            

                if (string.IsNullOrWhiteSpace(req.Name))
                {
                    return new BusinessResult(Const.FAIL_READ, "NAME MUST NOT BE EMPTY");
                }

                if (string.IsNullOrWhiteSpace(req.Description))
                {
                    return new BusinessResult(Const.FAIL_READ, "DESCRIPTION MUST NOT BE EMPTY");
                }

                var combo = _mapper.Map<Subscription>(req);

                combo.CreatedDate = DateTime.UtcNow.AddHours(7);
                combo.UpdatedDate = DateTime.UtcNow.AddHours(7);
                combo.Status = SD.GeneralStatus.ACTIVE;

                var result = await _unitOfWork.SubscriptionRepository.CreateAsync(combo);

                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, req);
                }

                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, ex.Message);
            }
        }

        public async Task<IBusinessResult> UpdateCombo(int comboId, CreateComboModel req)
        {
            try
            {
                if (req.Fee <= 0)
                {
                    return new BusinessResult(Const.FAIL_READ, "FEE MUST > 0");
                }

                if (string.IsNullOrWhiteSpace(req.Name))
                {
                    return new BusinessResult(Const.FAIL_READ, "NAME MUST NOT BE EMPTY");
                }

                if (string.IsNullOrWhiteSpace(req.Description))
                {
                    return new BusinessResult(Const.FAIL_READ, "DESCRIPTION MUST NOT BE EMPTY");
                }

                var combo = await _unitOfWork.SubscriptionRepository.GetByIdAsync(comboId);

                if (combo == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "CANNOT FIND COMBO");
                }

                var currentDate = DateTime.Now;
                var activeUsers = _unitOfWork.UserServiceRepository.CheckIsAvailable(comboId);

                if (activeUsers)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Có người đang dùng, không thể chỉnh sửa");
                }

                combo.Name = req.Name;
                combo.Description = req.Description;
                combo.Fee = req.Fee;
                combo.UpdatedDate = DateTime.UtcNow.AddHours(7);

                var result = await _unitOfWork.SubscriptionRepository.UpdateAsync(combo);

                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_UPDATE, Const.SUCCESS_UPDATE_MSG);
                }

                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, ex.Message);
            }
        }
        public async Task<IBusinessResult> GetAllCombos()
        {
            try
            {
                var subscriptions = _unitOfWork.SubscriptionRepository.GetAll();
                var bookings = _unitOfWork.BookingRepository.GetAll(); // Assuming you have access to bookings
                var subscriptionDtos = new List<ComboDto>();
                foreach (var s in subscriptions)
                {
                    var users = await _unitOfWork.UserServiceRepository.GetAllUserInSubscriptionsInUse(s.SubscriptionId);
                    subscriptionDtos.Add(new ComboDto
                    {
                        SubscriptionId = s.SubscriptionId,
                        Name = s.Name,
                        Description = s.Description,
                        Fee = s.Fee,
                        ValidityPeriod = s.ValidityPeriod,
                        CreatedDate = s.CreatedDate.ToString("dd-MM-yyyy"),
                        CreatedTime = s.CreatedDate.ToString("HH:mm"),
                        UpdatedDate = s.UpdatedDate.ToString("dd-MM-yyyy"),
                        UpdatedTime = s.UpdatedDate.ToString("HH:mm"),
                        Status = s.Status,
                        AccountId = s.AccountId,
                        NumberOfMeeting = s.NumberOfMeeting,
                        NumberOfUsers = users.Count
                    });
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, subscriptionDtos);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, ex.Message);
            }
        }
        public async Task<IBusinessResult> GetAllUserInCombo(int comboId)
        {
            try
            {
                var subscriptions = await _unitOfWork.UserServiceRepository.GetAllUserInSubscriptions(comboId);
                var subscriptionDtos = subscriptions.Select(s => new UserInSubVM
                {
                    SubscriptionId = (int)s.Booking.SubscriptionId,
                    UserSubscriptionId = s.UserSubscriptionId,
                    PurchaseDate = s.Booking.BookingDate.ToString("dd-MM-yyyy"),
                    SubName = s.Booking.Subscription.Name,
                    ValidityPeriod = s.Booking.Subscription.ValidityPeriod,
                    NumberOfMeeting = (int)s.Booking.Subscription.NumberOfMeeting,
                    PaymentCode = s.Booking.Transaction.PaymentCode,
                    NumberOfMeetingLeft = (int)s.NumberOfMeetingLeft,
                    UsersInSubscriptions = new List<GetUsersInSubscription>
                    {
                        new GetUsersInSubscription
                        {
                            Buyer = _mapper.Map<UserDTO>(s.Booking.Account),
                            Elderly = _mapper.Map<UserDTO>(s.Booking.Elderly.Account)
                        }
                    }
                }).ToList();

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, subscriptionDtos);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, ex.Message);
            }
        }

        public async Task<IBusinessResult> GetComboById(int id)
        {
            try
            {
                var subscription = await _unitOfWork.SubscriptionRepository.GetByIdAsync(id);

                if (subscription == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "Subscription not found");
                }

                var subscriptionDto = new ComboDto
                {
                    SubscriptionId = subscription.SubscriptionId,
                    Name = subscription.Name,
                    Description = subscription.Description,
                    Fee = subscription.Fee,
                    ValidityPeriod = subscription.ValidityPeriod,
                    CreatedDate = subscription.CreatedDate.ToString("dd-MM-yyyy"),
                    CreatedTime = subscription.CreatedDate.ToString("HH:mm"),
                    UpdatedDate = subscription.UpdatedDate.ToString("dd-MM-yyyy"),
                    UpdatedTime = subscription.UpdatedDate.ToString("HH:mm"),
                    Status = subscription.Status,
                    AccountId = subscription.AccountId,
                    NumberOfMeeting = subscription.NumberOfMeeting
                };

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, subscriptionDto);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, ex.Message);
            }
        }

        public async Task<IBusinessResult> UpdateComboStatus(int comboId, string status)
        {
            try
            {
                var checkComboExisted = _unitOfWork.SubscriptionRepository.FindByCondition(c => c.SubscriptionId == comboId).FirstOrDefault();

                if (checkComboExisted == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "Combo not found.");
                }
                if (status == "Inactive")
                {
                    var currentDate = DateTime.Now;
                    var activeUsers = _unitOfWork.UserServiceRepository.CheckIsAvailable(comboId);

                    if (activeUsers)
                    {
                        return new BusinessResult(Const.FAIL_UPDATE, "Có người đang dùng, không thể chuyển trạng thái");
                    }
                }

                checkComboExisted.Status = status;
                checkComboExisted.UpdatedDate = DateTime.Now; 

                var result = await _unitOfWork.SubscriptionRepository.UpdateAsync(checkComboExisted);

                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_UPDATE, "Combo status updated successfully.");
                }

                return new BusinessResult(Const.FAIL_UPDATE, "Failed to update combo status.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, ex.Message);
            }
        }

        public async Task<IBusinessResult> BackSubscription(int userSubscriptionId)
        {
            try
            {
                var userSubscription = _unitOfWork.UserServiceRepository.FindByCondition(us => (us.Status.Equals(SD.UserSubscriptionStatus.AVAILABLE) || us.Status.Equals(SD.UserSubscriptionStatus.BOOKED)) && us.UserSubscriptionId == userSubscriptionId).FirstOrDefault();

                if (userSubscription == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Cannot find user subscription");
                }

                if (userSubscription.Status.Equals(SD.UserSubscriptionStatus.BOOKED))
                {
                    userSubscription.Status = SD.UserSubscriptionStatus.ONETIME;
                }

                userSubscription.NumberOfMeetingLeft++;
                userSubscription.ProfessorId = null;

                var updateRs = await _unitOfWork.UserServiceRepository.UpdateAsync(userSubscription);

                if (updateRs > 0)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, "Status: " + userSubscription.Status + ", NumberOfMeetingLeft: " + userSubscription.NumberOfMeetingLeft);
                }

                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, ex.Message);
            }
        }
        public async Task<IBusinessResult> CheckIfUserHasOneTimeSubscription(int elderlyId)
        {
            try
            {
                // 1. Validate Elderly account
                var accountElderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(elderlyId);
                if (accountElderly == null || accountElderly.RoleId != 2)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly doesn't exist!");
                }

                // 2. Verify elderly exists
                var elderly = _unitOfWork.ElderlyRepository
                    .FindByCondition(p => p.ElderlyId == accountElderly.Elderly.ElderlyId)
                    .FirstOrDefault();
                if (elderly == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly doesn't exist!");
                }

                // 3. Check valid bookings
                var bookings = _unitOfWork.BookingRepository
                    .FindByCondition(b => b.ElderlyId == elderly.ElderlyId && b.Status.Equals(SD.BookingStatus.PAID))
                    .Select(b => b.BookingId)
                    .ToList();
                if (!bookings.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Bookings of elderly not found.");
                }

                var userSubscription = await _unitOfWork.UserServiceRepository.GetAppointmentUserSubscriptionByBookingIdAsync(bookings, SD.UserSubscriptionStatus.ONETIME);

                if (userSubscription?.Booking == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Booking details not found.");
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, ex.Message);
            }
        }
    }
}
