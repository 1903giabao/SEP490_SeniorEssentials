using AutoMapper;
using FirebaseAdmin.Messaging;
using Newtonsoft.Json.Linq;
using SE.Common;
using SE.Common.Response.Notification;
using SE.Data.UnitOfWork;
using SE.Service.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SE.Service.Services
{
    public interface INotificationService
    {
        Task<string> SendNotification(string token, string title, string body);
        Task<IBusinessResult> GetAllNotiInAccount(int accountId);
        Task<IBusinessResult> UpdateStatusNotificaction(int notiId, string status);
        Task<IBusinessResult> SendNotiToGetLocation(int familyMemberId, int elderlyId);
        Task<IBusinessResult> SendNotiLocation(int familyMemberId, int elderlyId, string longitude, string latitude);
        Task<IBusinessResult> SendNotToGetHealthIndicator(int accountId);
    }

    public class NotificationService : INotificationService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public NotificationService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IBusinessResult> GetAllNotiInAccount (int accountId)
        {
            try
            {
                var result = _unitOfWork.NotificationRepository.FindByCondition(n=>n.AccountId == accountId)
                    .OrderByDescending(n=>n.NotificationId).ToList();

                var final = new List<GetNotificationVM>();

                foreach (var item in result)
                {
                    var noti = new GetNotificationVM
                    {
                        AccountId = item.AccountId,
                        CreatedDate = item.CreatedDate,
                        Message = item.Message,
                        NotificationType = item.NotificationType,
                        NotificationId = item.NotificationId,
                        Status = item.Status,
                        Title = item.Title,
                        Data = item.Data,
                        ElderlyId = item.ElderlyId ?? 0
                    };

                    if (item.Title == "Cảnh báo sức khỏe")
                    {
                        
                        var elderly =await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(noti.ElderlyId);
                        if (elderly != null)
                        {
                            //return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, " Không tìm thấy người già trong thông báo");
                            noti.ElderlyId = elderly.Elderly.ElderlyId;
                            noti.FullName = elderly.FullName;
                        }
                    }
                    final.Add(noti);
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, final);
            }
            catch (Exception ex) 
            {
                throw new Exception(ex.Message);

            }
        }

        public async Task<IBusinessResult> UpdateStatusNotificaction(int notiId, string status)
        {
            try
            {
                var getNoti =await _unitOfWork.NotificationRepository.GetByIdAsync(notiId);
                if (getNoti == null) 
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Notification not found!");
                }
                getNoti.Status = status;
                var rs = await _unitOfWork.NotificationRepository.UpdateAsync(getNoti);
                if (rs < 1)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Cannot update");

                }
                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, "Update sucessfully");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }
        }
        public async Task<string> SendNotification(string token, string title, string body)
        {
            var message = new Message()
            {
                Token = token,
                Notification = new Notification()
                {
                    Title = title,
                    Body = body
                },
                Data = new Dictionary<string, string>()
                {
                    { "key1", "value1" }
                }
            };
            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            return response;
        }

        public async Task<IBusinessResult> SendNotiToGetLocation(int familyMemberId, int elderlyId)
        {
            try
            {
                var account = await _unitOfWork.AccountRepository.GetAccountAsync(familyMemberId);

                if (account == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Account does not exist!");
                }                
                
                var elderly = await _unitOfWork.AccountRepository.GetAccountAsync(elderlyId);

                if (elderly == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }
                if (!string.IsNullOrEmpty(elderly.DeviceToken) && elderly.DeviceToken != "string")
                {
                    var message = new Message()
                    {
                        Token = elderly.DeviceToken,
                        Notification = new Notification()
                        {
                            Title = "Yêu cầu lấy vị trí",
                            Body = $"Bạn nhận được yêu cầu lấy vị trí từ người thân {account.FullName}"
                        },
                        Data = new Dictionary<string, string>()
                        {
                            { "FamilyMemberId", account.AccountId.ToString() },
                        }
                    };
                    string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                }
                else
                {
                    var message = new Message()
                    {
                        Token = account.DeviceToken,
                        Notification = new Notification()
                        {
                            Title = "Lấy vị trí thất bại",
                            Body = $"Không Không xác định được vị trí của người già {elderly.FullName}"
                        },
                        Data = new Dictionary<string, string>()
                        {
                            { "FamilyMemberId", account.AccountId.ToString() },
                        }
                    };
                    string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, "Send notification to get location sucessfully");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }        
        
        public async Task<IBusinessResult> SendNotiLocation(int familyMemberId, int elderlyId, string longitude, string latitude)
        {
            try
            {
                var account = await _unitOfWork.AccountRepository.GetAccountAsync(familyMemberId);

                if (account == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Account does not exist!");
                }                
                
                var elderly = await _unitOfWork.AccountRepository.GetAccountAsync(elderlyId);

                if (elderly == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                if (!string.IsNullOrEmpty(account.DeviceToken) && account.DeviceToken != "string")
                {
                    if (!string.IsNullOrEmpty(longitude) && !string.IsNullOrEmpty(latitude))
                    {
                        var message = new Message()
                        {
                            Token = account.DeviceToken,
                            Notification = new Notification()
                            {
                                Title = "Lấy vị trí thành công",
                                Body = $"Đã xác định được vị trí của {elderly.FullName}"
                            },
                            Data = new Dictionary<string, string>()
                            {
                                { "Longitude",longitude },
                                { "Latitude", latitude },
                            }
                        };

                        string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                    }
                    else
                    {
                        var message = new Message()
                        {
                            Token = account.DeviceToken,
                            Notification = new Notification()
                            {
                                Title = "Lấy vị trí thất bại",
                                Body = $"Không xác định được vị trí của người già {elderly.FullName}"
                            },
                            Data = new Dictionary<string, string>()
                            {
                                { "", ""},
                            }
                        };

                        string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                    }
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, "Send location sucessfully");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }        
        
        public async Task<IBusinessResult> SendNotToGetHealthIndicator(int accountId)
        {
            try
            {
                var account = await _unitOfWork.AccountRepository.GetAccountAsync(accountId);

                if (account == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Account does not exist!");
                }                               

                if (!string.IsNullOrEmpty(account.DeviceToken) && account.DeviceToken != "string")
                {
                    var message = new Message()
                    {
                        Token = account.DeviceToken,
                        Notification = new Notification()
                        {
                            Title = "Cập nhật chỉ số sức khỏe",
                            Body = $"Đã đến giờ cập nhật chỉ số sức khỏe"
                        },
                        Data = new Dictionary<string, string>()
                        {
                            { "", ""},
                        }
                    };

                    string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, "Send location sucessfully");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }
    }
}
