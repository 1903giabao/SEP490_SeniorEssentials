using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Execution;
using CloudinaryDotNet;
using Firebase.Auth;
using Google.Api;
using Google.Cloud.Firestore;
using Org.BouncyCastle.Ocsp;
using SE.Common;
using SE.Common.DTO;
using SE.Common.Enums;
using SE.Common.Request.Account;
using SE.Data.Models;
using SE.Data.UnitOfWork;
using SE.Service.Base;
using SE.Service.Helper;
using static System.Net.WebRequestMethods;

namespace SE.Service.Services
{
    public interface IAccountService
    {
        Task<IBusinessResult> CreateNewTempAccount(CreateNewAccountDTO req);
        Task<IBusinessResult> GetAllUsers(int roleId = 0);
        Task<IBusinessResult> GetUserById(int id);
        Task<IBusinessResult> GetUserByPhoneNumber(string phoneNumber, int userId);
        Task<IBusinessResult> CreateSystemAccount(CreateSystemAccountRequest req);
        Task<IBusinessResult> CreateProfessorAccount(CreateProfessorAccountRequest req);
        Task<IBusinessResult> ChangeAccountStatus(ChangeAccountStatusReq req);
        Task<IBusinessResult> GetUserByPhoneNumberNotFriend(string phoneNumber, int userId);
    }

    public class AccountService : IAccountService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly ISmsService _smsService;
        private readonly FirestoreDb _firestoreDb;

        public AccountService(UnitOfWork unitOfWork, IMapper mapper, FirestoreDb firestoreDb, ISmsService smsService, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _firestoreDb = firestoreDb;
            _smsService = smsService;
            _emailService = emailService;
        }

        public async Task<IBusinessResult> CreateSystemAccount(CreateSystemAccountRequest req)
        {
            try
            {
                if (req.RoleId != 1 && req.RoleId != 5)
                {
                    return new BusinessResult(Const.FAIL_READ, "Invalid role!");
                }

                if (!FunctionCommon.IsValidEmail(req.Email) || !FunctionCommon.IsValidPhoneNumber(req.PhoneNumber))
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Wrong email or phone number format!");
                }

                var newAccount = new Data.Models.Account
                {
                    RoleId = req.RoleId,
                    Avatar = "https://icons.veryicon.com/png/o/miscellaneous/standard/avatar-15.png",
                    CreatedDate = DateTime.UtcNow.AddHours(7),
                    DateOfBirth = req.DateOfBirth,
                    Email = req.Email,
                    Password = SecurityUtil.Hash(req.Password),
                    FullName = req.FullName,
                    Gender = req.Gender,
                    IsVerified = true,
                    PhoneNumber = req.PhoneNumber,
                    Status = SD.GeneralStatus.ACTIVE,
                    IsSuperAdmin = false,
                    Otp = null,
                };

                var createRs = await _unitOfWork.AccountRepository.CreateAsync(newAccount);

                if (createRs > 0)
                {
                    var newContentProvider = new ContentProvider
                    {
                        AccountId = newAccount.AccountId,
                        Organization = "System",
                        Status = SD.GeneralStatus.ACTIVE,
                    };

                    var createContentProviderRs = await _unitOfWork.ContentProviderRepository.CreateAsync(newContentProvider);

                    if (createContentProviderRs < 1)
                    {
                        return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Failed to create content provider!");
                    }

                    var mailData = new EmailData
                    {
                        EmailToId = req.Email,
                        EmailToName = "Senior Essentials",
                        EmailBody = $"Chào {newAccount.FullName}, tài khoản của bạn trên Senior Essentials đã sẵn sàng! Thông tin đăng nhập: Email: {newAccount.Email}, Mật khẩu: {req.Password}. Hãy truy cập https://senior-essentials-manage.vercel.app/login để bắt đầu chia sẻ nội dung chất lượng. Nếu gặp vấn đề, vui lòng liên hệ senioressentialsco@gmail.com. Chúc bạn có trải nghiệm tuyệt vời!",
                        EmailSubject = "TẠO TÀI KHOẢN THÀNH CÔNG"
                    };

                    var emailResult = await _emailService.SendEmailAsync(mailData);
                    if (!emailResult)
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Can not send email!");
                    }

                    var result = await _smsService.SendSmsAsync(req.PhoneNumber, $"Chào {newAccount.FullName}, tài khoản của bạn trên Senior Essentials đã sẵn sàng! Thông tin đăng nhập: Email: {newAccount.Email}, Mật khẩu: {req.Password}. Hãy truy cập https://senior-essentials-manage.vercel.app/login để bắt đầu chia sẻ nội dung chất lượng. Nếu gặp vấn đề, vui lòng liên hệ senioressentialsco@gmail.com. Chúc bạn có trải nghiệm tuyệt vời!");

                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG);
                }

                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IBusinessResult> CreateProfessorAccount(CreateProfessorAccountRequest req)
        {
            try
            {
                if (!FunctionCommon.IsValidEmail(req.Email) || !FunctionCommon.IsValidPhoneNumber(req.PhoneNumber))
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Wrong email or phone number format!");
                }

                var avatar = ("", "");

                if (req.Avatar != null)
                {
                    avatar = await CloudinaryHelper.UploadImageAsync(req.Avatar);
                }

                var newAccount = new Data.Models.Account
                {
                    RoleId = 4,
                    Avatar = req.Avatar == null ? "https://icons.veryicon.com/png/o/miscellaneous/standard/avatar-15.png" : avatar.Item2,
                    CreatedDate = DateTime.UtcNow.AddHours(7),
                    DateOfBirth = req.DateOfBirth,
                    Email = req.Email,
                    Password = SecurityUtil.Hash(req.Password),
                    FullName = req.FullName,
                    Gender = req.Gender,
                    PhoneNumber = req.PhoneNumber,
                    IsVerified = true,
                    IsSuperAdmin = false,
                    Status = SD.GeneralStatus.ACTIVE,
                    DeviceToken = null,
                    Otp = null,
                };

                var createRs = await _unitOfWork.AccountRepository.CreateAsync(newAccount);

                if (createRs > 0)
                {
                    var newProfessor = new Professor
                    {
                        AccountId = newAccount.AccountId,
                        Achievement = req.Achievement,
                        Career = req.Career,
                        ConsultationFee = req.ConsultationFee,
                        ClinicAddress = req.ClinicAddress,
                        ExperienceYears = req.ExperienceYears,
                        Knowledge = req.Knowledge,
                        Qualification = req.Qualification,
                        Specialization = req.Specialization,
                        Status = SD.GeneralStatus.ACTIVE,
                        Rating = 0
                    };

                    var createProfessorRs = await _unitOfWork.ProfessorRepository.CreateAsync(newProfessor);

                    if (createProfessorRs < 1)
                    {
                        return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Failed to create professor!");
                    }

                    var mailData = new EmailData
                    {
                        EmailToId = req.Email,
                        EmailToName = "Senior Essentials",
                        EmailBody = $"Xin chào {newAccount.FullName}, tài khoản của bạn trên Senior Essentials đã được tạo thành công! Thông tin đăng nhập: Email: {newAccount.Email}, Mật khẩu: {req.Password}. Bạn có thể đăng nhập ngay tại https://senior-essentials-manage.vercel.app/login để bắt đầu sử dụng hệ thống, quản lý hồ sơ bệnh nhân và cập nhật thông tin. Nếu cần hỗ trợ, vui lòng liên hệ senioressentialsco@gmail.com. Cảm ơn bạn đã đồng hành cùng chúng tôi!",
                        EmailSubject = "TẠO TÀI KHOẢN THÀNH CÔNG"
                    };

                    var emailResult = await _emailService.SendEmailAsync(mailData);
                    if (!emailResult)
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Can not send email!");
                    }

                    var result = await _smsService.SendSmsAsync(req.PhoneNumber, $"Xin chào {newAccount.FullName}, tài khoản của bạn trên Senior Essentials đã được tạo thành công! Thông tin đăng nhập: Email: {newAccount.Email}, Mật khẩu: {req.Password}. Bạn có thể đăng nhập ngay tại https://senior-essentials-manage.vercel.app/login để bắt đầu sử dụng hệ thống, quản lý hồ sơ bệnh nhân và cập nhật thông tin. Nếu cần hỗ trợ, vui lòng liên hệ senioressentialsco@gmail.com. Cảm ơn bạn đã đồng hành cùng chúng tôi!");

                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG);
                }

                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Failed to create account!");

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IBusinessResult> CreateNewTempAccount(CreateNewAccountDTO req)
        {
            try
            {
                var existedAcc = _unitOfWork.AccountRepository.FindByCondition(e => e.Email.Equals(req.Account)).FirstOrDefault();

                int rs;
                var newAccount = new Data.Models.Account();
                if (existedAcc != null)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE, "Email already existed!", existedAcc);
                }

                if (FunctionCommon.IsValidPhoneNumber(req.Account))
                {
                    newAccount = new Data.Models.Account
                    {
                        RoleId = req.RoleId,
                        PhoneNumber = req.Account,
                        Otp = req.OTP,
                        Password = SecurityUtil.Hash(req.Password),
                        IsVerified = false,
                        Status = SD.GeneralStatus.ACTIVE,
                        IsSuperAdmin = false
                    };
                    rs = await _unitOfWork.AccountRepository.CreateAsync(newAccount);
                }
                else
                {
                    newAccount = new Data.Models.Account
                    {
                        RoleId = req.RoleId,
                        Email = req.Account,
                        Otp = req.OTP,
                        Password = SecurityUtil.Hash(req.Password),
                        IsVerified = false,
                        Status = SD.GeneralStatus.ACTIVE,
                        IsSuperAdmin = false

                    };
                    rs = await _unitOfWork.AccountRepository.CreateAsync(newAccount);
                }
                if (rs > 0)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, newAccount);
                }
                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, newAccount);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IBusinessResult> GetAllUsers(int roleId = 0)
        {
            var users = new List<Data.Models.Account>();

            if (roleId == 0)
            {
                users = _unitOfWork.AccountRepository.FindByCondition(a=>a.FullName != null).ToList();
            }
            else
            {
                users = _unitOfWork.AccountRepository.FindByCondition(a => a.RoleId == roleId && a.FullName != null).ToList();
            }

            var rs = _mapper.Map<List<UserDTO>>(users);

            return new BusinessResult (Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, rs);
        }

        public async Task<IBusinessResult> GetUserById(int id)
        {
            try
            {
                var user = await _unitOfWork.AccountRepository.GetByIdAsync(id);

                if (user == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "User does not exist!");
                }

                var rs = _mapper.Map<UserDTO>(user);

                return new BusinessResult (Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, rs);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetUserByPhoneNumber(string phoneNumber, int userId)
        {
            try
            {
                if (!FunctionCommon.IsValidPhoneNumber(phoneNumber))
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Wrong phone number format!");
                }

                var userPhone = await _unitOfWork.AccountRepository.GetByPhoneNumberAsync(phoneNumber);

                if (userPhone == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "User does not exist!");
                }

                if (userPhone.AccountId == userId)
                {
                    return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, new { isMe = true });
                }

                var userLink = await _unitOfWork.UserLinkRepository.GetByUserIdsAsync(userId, userPhone.AccountId);

                if (userLink != null)
                {
                    if (userLink.RelationshipType.Equals("Friend") && userLink.Status.Equals(SD.UserLinkStatus.ACCEPTED))
                    {
                        return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, new { isFriend = true });
                    }

                    if (userLink.RelationshipType.Equals("Family") && userLink.Status.Equals(SD.UserLinkStatus.ACCEPTED))
                    {
                        return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, new { isFamily = true });
                    }

                    if (userLink.Status.Equals(SD.UserLinkStatus.PENDING))
                    {
                        var result = _mapper.Map<GetUserPhoneNumberDTO>(userPhone);
                        result.RequestUserId = userLink.AccountId1;
                        
                        return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
                    }
                }

                var rs = _mapper.Map<UserDTO>(userPhone);


                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, rs);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }        
        
        public async Task<IBusinessResult> GetUserByPhoneNumberNotFriend(string phoneNumber, int userId)
        {
            try
            {
                if (!FunctionCommon.IsValidPhoneNumber(phoneNumber))
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Wrong phone number format!");
                }

                var user = await _unitOfWork.AccountRepository.GetAccountAsync(userId);

                if (user == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "User does not exist!");
                }

                var userPhone = await _unitOfWork.AccountRepository.GetByPhoneNumberAsync(phoneNumber);

                if (userPhone == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "User does not exist!");
                }                
                
                if (userPhone.AccountId == userId)
                {
                    return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, new { isMe = true });
                }

                var userLink = await _unitOfWork.UserLinkRepository.GetByUserIdsAsync(userId, userPhone.AccountId);

                if (userLink != null)
                {
                    if (userLink.RelationshipType.Equals("Friend") && userLink.Status.Equals(SD.UserLinkStatus.ACCEPTED))
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG);
                    }

                    if (userLink.RelationshipType.Equals("Family") && userLink.Status.Equals(SD.UserLinkStatus.ACCEPTED))
                    {
                        return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, new { isFamily = true });
                    }

                    if (userLink.Status.Equals(SD.UserLinkStatus.PENDING))
                    {
                        var result = _mapper.Map<GetUserPhoneNumberDTO>(userPhone);
                        result.RequestUserId = userLink.AccountId1;
                        
                        return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
                    }
                }

                var rs = _mapper.Map<UserDTO>(userPhone);


                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, rs);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> ChangeAccountStatus(ChangeAccountStatusReq req)
        {
            try
            {
                var user = await _unitOfWork.AccountRepository.GetAccountAsync(req.AccountId);

                if (user == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "User does not exist!");
                }

                if (!req.Status.Equals(SD.GeneralStatus.ACTIVE) && !req.Status.Equals(SD.GeneralStatus.INACTIVE))
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Status does not support!");
                }

                user.Status = req.Status;

                var updateRs = await _unitOfWork.AccountRepository.UpdateAsync(user);

                if (updateRs > 0)
                {
                    if (user.RoleId == 2)
                    {
                        var elderly = await _unitOfWork.ElderlyRepository.GetByIdAsync(user.Elderly.ElderlyId);

                        if (elderly == null)
                        {
                            return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                        }               
                        
                        elderly.Status = req.Status;

                        var updateElderlyRs = await _unitOfWork.ElderlyRepository.UpdateAsync(elderly);

                        if (updateElderlyRs < 1)
                        {
                            return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                        }
                    }
                    else if (user.RoleId == 5)
                    {
                        var contentProvider = await _unitOfWork.ContentProviderRepository.GetByIdAsync(user.ContentProvider.ContentProviderId);

                        if (contentProvider == null)
                        {
                            return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Content does not exist!");
                        }

                        contentProvider.Status = req.Status;

                        var updateContentProviderRs = await _unitOfWork.ContentProviderRepository.UpdateAsync(contentProvider);

                        if (updateContentProviderRs < 1)
                        {
                            return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                        }
                    }                    
                    else if (user.RoleId == 4)
                    {
                        var professor = await _unitOfWork.ProfessorRepository.GetByIdAsync(user.Professor.ProfessorId);

                        if (professor == null)
                        {
                            return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Professor does not exist!");
                        }

                        professor.Status = req.Status;

                        var updateProfessorRs = await _unitOfWork.ProfessorRepository.UpdateAsync(professor);

                        if (updateProfessorRs < 1)
                        {
                            return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                        }
                    }

                    return new BusinessResult(Const.SUCCESS_UPDATE, Const.SUCCESS_UPDATE_MSG);
                }

                return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, "An unexpected error occurred: " + ex.Message);
            }
        }
    }
}
