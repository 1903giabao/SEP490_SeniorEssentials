using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Firebase.Auth;
using Google.Api;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Ocsp;
using SE.Common;
using SE.Common.DTO;
using SE.Common.Enums;
using SE.Common.Request;
using SE.Common.Setting;
using SE.Data.Models;
using SE.Data.UnitOfWork;
using SE.Service.Base;
using SE.Service.Helper;
using static System.Net.WebRequestMethods;

namespace SE.Service.Services
{

    public interface IIdentityService
    {
        Task<IBusinessResult> SendOtpToUser(string email,string password, int role);
        Task<IBusinessResult> SubmitOTP(CreateUserRequest req);
        Task<IBusinessResult> Signup(SignUpModel req);
        Task<IBusinessResult> Login(string email, string password, string deviceToken, string ipAddress);
        Task<UserModel> GetUserInToken(string token);
        Task<IBusinessResult> GetUserByEmail(string username);

    }

    public class IdentityService : IIdentityService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IJwtService _jwtService;
        private readonly UnitOfWork _unitOfWork;
        private readonly IFirebaseService _firebaseService;
        private readonly string _confirmUrl;
        private readonly string _frontendUrl;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly ISmsService _smsService;
        private readonly FirestoreDb _firestoreDb;

        private readonly IAccountService _accountService;

        public IdentityService(IMapper mapper, UnitOfWork unitOfWork, IOptions<JwtSettings> jwtSettingsOptions, IFirebaseService firebaseService, IEmailService emailService, IAccountService accountService, ISmsService smsService, FirestoreDb firestoreDb, IJwtService jwtService)
        {
            _unitOfWork = unitOfWork;
            _jwtSettings = jwtSettingsOptions.Value;
            _firebaseService = firebaseService;
            _emailService = emailService;
            _accountService = accountService;
            _mapper = mapper;
            _smsService = smsService;
            _firestoreDb = firestoreDb;
            _jwtService = jwtService;
        }

        public async Task<IBusinessResult> SendOtpToUser(string account, string password, int role)
        {
            try
            {
                var otp = new Random().Next(100000, 999999);
                var rs = new Object();
                if (FunctionCommon.IsValidEmail(account))
                {
                    var existedEmail = await _unitOfWork.AccountRepository.GetByEmailAsync(account);

                    if (existedEmail!=null && existedEmail.IsVerified == true && existedEmail.FullName== null)
                    {
                        var remove =await _unitOfWork.AccountRepository.RemoveAsync(existedEmail);
                    }

                    var mailData = new EmailData
                    {
                        EmailToId = account,
                        EmailToName = "Senior Essentials",
                        EmailBody = otp.ToString(),
                        EmailSubject = "XÁC NHẬN MÃ OTP"
                    };

                    var emailResult = await _emailService.SendEmailAsync(mailData);
                    if (!emailResult)
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Can not send email!");
                    }

                    var newAccount = new CreateNewAccountDTO
                    {
                        Account = account,
                        OTP = otp.ToString(),
                        Password = password,
                        RoleId = role
                    };

                    var createUserResponse = await _accountService.CreateNewTempAccount(newAccount );

                    var userReponse = (Account)createUserResponse.Data;

                 
                    rs = new
                    {
                        Message = "Send OTP successfully!",
                        Method = "Email",
                        AccountId = userReponse.AccountId,
                    };
                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, rs);

                }

                else if (FunctionCommon.IsValidPhoneNumber(account))
                { 
                    var existedPhone = _unitOfWork.AccountRepository.FindByCondition(a => a.PhoneNumber == account).FirstOrDefault();

                    if (existedPhone != null && existedPhone.FullName != null)
                    {
                        return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Số điện thoại đã tồn tại trong hệ thống!");

                    }
                    else if (existedPhone != null && existedPhone.IsVerified == true && existedPhone.FullName == null)
                    {
                        var remove = await _unitOfWork.AccountRepository.RemoveAsync(existedPhone);
                    }
                        var sendPhoneOTP = await _smsService.SendOTPSmsAsync(account, otp.ToString());
                    if (sendPhoneOTP == null)
                    {
                        return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Cannot send sms.");
                    }
                    var newAccount = new CreateNewAccountDTO
                    {
                        Account = account,
                        OTP = otp.ToString(),
                        Password = password,
                        RoleId = role
                    };
                    var createUserResponse = await _accountService.CreateNewTempAccount(newAccount);
                    var user = (Account)createUserResponse.Data;
                    rs = new
                    {
                        Message = "Send OTP successfully!",
                        Method = "Phone number",
                        AccountId = user.AccountId

                    };

                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, rs);
                }


                rs = new
                {
                    Message = "Wrong format of Email or Phone number",
                    Method = ""
                };

                return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, rs);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<IBusinessResult> SubmitOTP(CreateUserRequest req)
        {
            try
            {

                var rs = new BusinessResult();
                var user = _unitOfWork.AccountRepository.FindByCondition(u => u.Email.Equals(req.Account) || u.PhoneNumber.Equals(req.Account)).FirstOrDefault();

                if (user == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Wrong Email");
                }
                if (!user.Otp.Equals(req.OTPCode))
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Wrong OTP");
                }
                user.Otp = "0";
                user.IsVerified = true;
                user.Status = "Active";
                var result = await _unitOfWork.AccountRepository.UpdateAsync(user);
                rs.Message = "XÁC MINH OTP THÀNH CÔNG!";
                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, "Verify successfully!");
                }
                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Cannot submit OTP");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<IBusinessResult> Signup(SignUpModel req)
        {
            try
            {
                if (!FunctionCommon.IsValidEmail(req.Email) && !FunctionCommon.IsValidPhoneNumber(req.PhoneNumber))
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Wrong email or phone number format!");
                }
                var user = _unitOfWork.AccountRepository.GetById(req.AccountId);
              
                if (user != null && user.IsVerified == false)
                {
                        return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Email is registered but not verified!");
                }
                

                if (req.Avatar == null) {
                    user.Avatar = "https://icons.veryicon.com/png/o/miscellaneous/standard/avatar-15.png";
                }
                
                else
                {
                    var urlLink = await CloudinaryHelper.UploadImageAsync(req.Avatar);
                    user.Avatar = urlLink.Url;

                }

                user.Email = req.Email;
                user.PhoneNumber = req.PhoneNumber;
                user.Status = SD.GeneralStatus.ACTIVE;
                user.RoleId = req.RoleId;
                user.FullName = req.FullName;
                user.Gender = req.Gender;
                user.DateOfBirth = DateTime.Parse(req.DateOfBirth);
                user.CreatedDate = DateTime.UtcNow.AddHours(7);
                user.IsSuperAdmin = false;

                var res = await _unitOfWork.AccountRepository.UpdateAsync(user);

                if (req.RoleId == 2)
                {
                    string medicalRecordsPassage = string.Join(".", req.MedicalRecord);

                    var newElderly = new Elderly
                    {
                        AccountId = user.AccountId,
                        MedicalRecord = medicalRecordsPassage,
                        Status = SD.GeneralStatus.ACTIVE,
                    };
                    var rsE = await _unitOfWork.ElderlyRepository.CreateAsync(newElderly);

                    var weight = new Weight
                    {
                        ElderlyId = newElderly.ElderlyId,
                        DateRecorded = DateTime.UtcNow.AddHours(7),
                        Status  = SD.GeneralStatus.ACTIVE,
                        Weight1 = Decimal.Parse(req.Weight),
                        WeightSource = "Manually",
                        CreatedBy = req.FullName
                    };

                    var saveWeight = await _unitOfWork.WeightRepository.CreateAsync(weight);

                    var height = new Height
                    {
                        ElderlyId = newElderly.ElderlyId,
                        DateRecorded = DateTime.UtcNow.AddHours(7),
                        Status = SD.GeneralStatus.ACTIVE,
                        Height1 = Decimal.Parse(req.Height),
                        HeightSource = "Manually",
                        CreatedBy = req.FullName
                    };

                    var saveHeight = await _unitOfWork.HeightRepository.CreateAsync(height);
                }
                if (req.RoleId == 3)
                {
                    var newFamilyMember = new FamilyMember
                    {
                        AccountId = req.AccountId,
                        Status = SD.GeneralStatus.ACTIVE,
                        
                    };
                    var saveFM = await _unitOfWork.FamilyMemberRepository.CreateAsync(newFamilyMember);
                }

                var onlineMembersRef = _firestoreDb.Collection("OnlineMembers");

                var onlineMemberData = new Dictionary<string, object>
                            {
                                { "IsOnline", true }
                            };

                await onlineMembersRef.Document(user.AccountId.ToString()).SetAsync(onlineMemberData);

                if (res < 0)
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Fail to create new account!");
                }

                if (req.CreatorAccountId != 0)
                {
                    var createUserLink = new UserLink
                    {
                        AccountId1 = req.CreatorAccountId,
                        AccountId2 = req.AccountId,
                        CreatedAt = DateTime.UtcNow.AddHours(7),
                        RelationshipType = "Family",
                        Status = SD.UserLinkStatus.ACCEPTED,
                        UpdatedAt = DateTime.UtcNow.AddHours(7),
                    };

                    await _unitOfWork.UserLinkRepository.CreateAsync(createUserLink);
                }

                return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, "Create new account successfully!");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<IBusinessResult> Login(string email, string password, string deviceToken, string ipAddress)
        {
            try
            {
                if (!FunctionCommon.IsValidEmail(email) && !FunctionCommon.IsValidPhoneNumber(email))
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Invalid email or phone number format!");
                }
                var user = _unitOfWork.AccountRepository
                                            .FindByCondition(u => u.Email == email || u.PhoneNumber == email )
                                            .FirstOrDefault();
                if (user == null || !SecurityUtil.Hash(password).Equals(user.Password))
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Sai Email hoặc mật khẩu!");
                }
                var hash = SecurityUtil.Hash(password);
                if (user.Status.Equals(SD.GeneralStatus.INACTIVE))
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Your account has been banned!");
                }
                

                if (user.IsVerified == false)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Email is registered but not verified!");
                }

                var userRole = _unitOfWork.RoleRepository
                                                .FindByCondition(ur => ur.RoleId == user.RoleId)
                                                .FirstOrDefault();
                user.Role = userRole!;

                var findDevice = _unitOfWork.AccountRepository.FindByCondition(a=>a.DeviceToken == deviceToken && a.AccountId != user.AccountId).FirstOrDefault();
                if(findDevice != null)
                {
                    findDevice.DeviceToken = null;
                    await _unitOfWork.AccountRepository.UpdateAsync(findDevice);

                }
                if (deviceToken != null)
                {
                    user.DeviceToken = deviceToken;
                }
                _unitOfWork.AccountRepository.Update(user);

                var tokenResponse = await _jwtService.GenerateTokens(user, ipAddress);

                return new BusinessResult(Const.SUCCESS_LOGIN, Const.SUCCESS_LOGIN_MSG, tokenResponse);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<UserModel> GetUserInToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new Exception("Authorization header is missing or invalid.");
            }
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                throw new Exception("Token has expired.");
            }
            string userName = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

            var user = await _unitOfWork.AccountRepository.GetByEmailAsync(userName);
            if (user is null)
            {
                throw new Exception("Cannot find User");
            }
            return _mapper.Map<UserModel>(user);
        }

        public async Task<IBusinessResult> GetUserByEmail(string username)
        {
            try
            {
                var account = await _unitOfWork.AccountRepository.GetByEmailAsync(username);
                var result = _mapper.Map<UserModel>(account);
                return new BusinessResult(Const.SUCCESS_READ,Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(500, ex.Message);
            }
        }

    }
}
