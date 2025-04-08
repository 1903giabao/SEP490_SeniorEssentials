using AutoMapper;
using SE.Common.DTO;
using SE.Common.Enums;
using SE.Common.Request;
using SE.Common;
using SE.Data.UnitOfWork;
using SE.Service.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SE.Common.Request.Booking;
using System.Security.Cryptography;
using CloudinaryDotNet.Core;
using Newtonsoft.Json;
using SE.Common.Response.Booking;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ZaloPay.Helper.Crypto;
using ZaloPay.Helper;
using FirebaseAdmin.Messaging;
using SE.Data.Models;
using SE.Common.DTO.Content;
using static Google.Cloud.Vision.V1.ProductSearchResults.Types;
using Org.BouncyCastle.Ocsp;
using CloudinaryDotNet;

namespace SE.Service.Services
{
    public interface IBookingService
    {
        Task<IBusinessResult> CreateBookingOrder(BookingOrderRequest req);
        Task<IBusinessResult> CheckOrderStatus(string appTransId);
        Task<IBusinessResult> ConfirmOrder(string appTransId);
        Task<IBusinessResult> CheckIfUserHasBooking(int accountId);
    }

    public class BookingService : IBookingService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly string _appId;
        private readonly string _key1;
        private readonly string _key2;
        public BookingService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _appId = Environment.GetEnvironmentVariable("ZalopayAppId");
            _key1 = Environment.GetEnvironmentVariable("ZalopayKey1");
            _key2 = Environment.GetEnvironmentVariable("ZalopayKey2");
        }

        public async Task<IBusinessResult> CreateBookingOrder(BookingOrderRequest req)
        {
            try
            {
                if (req.AccountId <= 0)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Invalid account ID.");
                }

                var buyer = await _unitOfWork.AccountRepository.GetByIdAsync(req.AccountId);
                if (buyer == null || buyer.RoleId != 3 || !buyer.Status.Equals(SD.GeneralStatus.ACTIVE))
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Account does not exist.");
                }

                var elderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(req.ElderlyId);
                if (elderly == null || elderly.RoleId != 2 || !elderly.Status.Equals(SD.GeneralStatus.ACTIVE))
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist.");
                }

                var subscription = await _unitOfWork.SubscriptionRepository.GetByIdAsync(req.SubscriptionId);
                if (subscription == null || !subscription.Status.Equals(SD.GeneralStatus.ACTIVE))
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Subscription does not exist.");
                }

                var currentDatetime = DateTime.UtcNow.AddHours(7);
                Random rnd = new Random();
                var uniqueNumber = ZaloPay.Helper.Utils.Generate7DigitUniqueId();
                string appTransId = currentDatetime.ToString("yyMMdd") + "_" + uniqueNumber;

                long appTime = Utils.GetTimeStampUtc7();

                var embedData = new { /*redirecturl = $"https://localhost:7288/booking-management/confirm/" */};
                var items = Array.Empty<string>();

                var appItem = JsonConvert.SerializeObject(items);
                var appEmbedData = JsonConvert.SerializeObject(embedData);

                var data = _appId + "|" + appTransId + "|" + buyer.FullName + "|" + (long)Math.Round(subscription.Fee) + "|"
                                  + appTime + "|" + appEmbedData + "|" + appItem;

                var mac = HmacHelper.Compute(_key1, data);

                var orderData = new
                {
                    app_id = int.Parse(_appId),
                    app_user = buyer.FullName,
                    app_trans_id = appTransId,
                    app_time = appTime,
                    amount = (long)Math.Round(subscription.Fee),
                    item = appItem,
                    expire_duration_seconds = (long)Math.Round(300.0),
                    description = $"Senior Essentials - Thanh toán đơn hàng #{appTransId}",
                    embed_data = appEmbedData,
                    bank_code = "zalopayapp",
                    mac = mac,
                };

                string jsonData = JsonConvert.SerializeObject(orderData);

                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync("https://sb-openapi.zalopay.vn/v2/create",
                        new StringContent(jsonData, Encoding.UTF8, "application/json"));

                    var responseString = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ZaloPayOrderResponse>(responseString);
                    result.app_trans_id = appTransId;

                    if (result.return_code == 1)
                    {
                        var booking = new Booking
                        {
                            AccountId = buyer.AccountId,
                            ElderlyId = elderly.Elderly.ElderlyId,
                            BookingDate = currentDatetime,
                            Note = "",
                            PaymentMethod = "ZaloPay",
                            Price = subscription.Fee,
                            SubscriptionId = subscription.SubscriptionId,
                            Status = SD.BookingStatus.PENDING,
                        };

                        var createBookingRs = await _unitOfWork.BookingRepository.CreateAsync(booking);

                        if (createBookingRs < 1)
                        {
                            return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Cannot create booking");
                        }

                        var transaction = new Transaction
                        {
                            BookingId = booking.BookingId,
                            AccountId = booking.AccountId,
                            PaymentDate = currentDatetime,
                            PaymentLink = result.order_url,
                            PaymentStatus = SD.BookingStatus.PENDING,
                            Price = booking.Price,
                            PaymentCode = appTransId,
                        };

                        var createTransactionRs = await _unitOfWork.TransactionRepository.CreateAsync(transaction);

                        if (createTransactionRs < 1)
                        {
                            return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Cannot create transaction");
                        }

                        booking.TransactionId = transaction.TransactionId;

                        var updateBookingRs = await _unitOfWork.BookingRepository.UpdateAsync(booking);

                        if (updateBookingRs < 1)
                        {
                            return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Cannot update booking");
                        }

                        var userSubscription = new UserSubscription
                        {
                            NumberOfMeetingLeft = subscription.NumberOfMeeting,
                            BookingId = booking.BookingId,
                            StartDate = booking.BookingDate,
                            EndDate = booking.BookingDate.AddDays(subscription.ValidityPeriod),
                            Status = SD.GeneralStatus.ACTIVE,
                        };

                        var createUserSubscription = await _unitOfWork.UserServiceRepository.CreateAsync(userSubscription);

                        if (createUserSubscription < 1)
                        {
                            return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Cannot create user subscription");
                        }

                        return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, result);
                    }
                    else
                    {
                        return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, result.return_message + " : " + result.sub_return_message);
                    }
                }
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> CheckOrderStatus(string appTransId)
        {
            try
            {
                var data = _appId + "|" + appTransId + "|" + _key1;

                var mac = HmacHelper.Compute(_key1, data);

                var checkStatusRequest = new
                {
                    app_id = int.Parse(_appId),
                    app_trans_id = appTransId,
                    mac = mac,
                };

                string jsonData = JsonConvert.SerializeObject(checkStatusRequest);

                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync("https://sb-openapi.zalopay.vn/v2/query",
                        new StringContent(jsonData, Encoding.UTF8, "application/json"));

                    var responseString = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<CheckOrderStatusResponse>(responseString);

                    return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
                }
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }        
        
        public async Task<IBusinessResult> ConfirmOrder(string appTransId)
        {
            try
            {
                var transaction = _unitOfWork.TransactionRepository.FindByCondition(t => t.PaymentCode.Equals(appTransId)).FirstOrDefault();

                if (transaction == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Cannot find transaction");
                }

                transaction.PaymentStatus = SD.BookingStatus.PAID;

                var updateTransactionRs = await _unitOfWork.TransactionRepository.UpdateAsync(transaction);

                if (updateTransactionRs < 1)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG, "Cannot update transaction");
                }

                var booking = _unitOfWork.BookingRepository.FindByCondition(b => b.TransactionId == transaction.TransactionId).FirstOrDefault();

                if (booking == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Cannot find booking");
                }

                booking.Status = SD.BookingStatus.PAID;

                var updateBookingRs = await _unitOfWork.BookingRepository.UpdateAsync(booking);

                if (updateBookingRs < 1)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG, "Cannot update booking");
                }

                return new BusinessResult(Const.SUCCESS_UPDATE, Const.SUCCESS_UPDATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> CheckIfUserHasBooking(int accountId)
        {
            try
            {
                if (accountId <= 0)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Invalid account ID.");
                }

                var elderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(accountId);
                if (elderly == null || elderly.RoleId != 2 || !elderly.Status.Equals(SD.GeneralStatus.ACTIVE))
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist.");
                }

                var bookings = _unitOfWork.BookingRepository.FindByCondition(b => b.ElderlyId == elderly.Elderly.ElderlyId && b.Status.Equals(SD.BookingStatus.PAID))
                                                            .Select(b => b.BookingId).ToList();

                if (bookings.Any())
                {
                    var userSubscription = await _unitOfWork.UserServiceRepository.GetUserSubscriptionByBookingIdAsync(bookings, SD.GeneralStatus.ACTIVE);

                    var mapperBooking = _mapper.Map<BookingDTO>(userSubscription.Booking);
                    mapperBooking.ProfessorId = userSubscription.ProfessorId;

                    return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, mapperBooking);
                }

                return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }
    }
}
