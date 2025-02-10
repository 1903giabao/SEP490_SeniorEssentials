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
        Task<IBusinessResult> SubmitOTP(CreateUserReq req);
        Task<IBusinessResult> Signup(SignUpModel req);
        Task<IBusinessResult> Login(string email, string password, string deviceToken);
        Task<UserModel> GetUserInToken(string token);
        Task<IBusinessResult> GetUserByEmail(string username);

    }

    public class IdentityService : IIdentityService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly UnitOfWork _unitOfWork;
        private readonly IFirebaseService _firebaseService;
        private readonly string _confirmUrl;
        private readonly string _frontendUrl;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly ISmsService _smsService;

        private readonly IAccountService _accountService;

        public IdentityService(IMapper mapper, UnitOfWork unitOfWork, IOptions<JwtSettings> jwtSettingsOptions, IFirebaseService firebaseService, IEmailService emailService, IAccountService accountService, ISmsService smsService)
        {
            _unitOfWork = unitOfWork;
            _jwtSettings = jwtSettingsOptions.Value;
            _firebaseService = firebaseService;
            _emailService = emailService;
            _accountService = accountService;
                _mapper = mapper;
            _smsService = smsService;
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
                        return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Your email is created! Please login to fill information");

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
                    var sendPhoneOTP = await _smsService.SendSmsAsync(account, otp.ToString());
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

                    rs = new
                    {
                        Message = "Send OTP successfully!",
                        Method = "Phone number"
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

        public async Task<IBusinessResult> SubmitOTP(CreateUserReq req)
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
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Wrong email or phone number format format!");
                }
                var user = _unitOfWork.AccountRepository.GetById(req.AccountId);
                
                if (user != null && user.IsVerified == false)
                {
                        return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Email is registered but not verified!");
                }

                var urlLink = await CloudinaryHelper.UploadImageAsync(req.Avatar);

                user.Avatar = urlLink.Url;
                user.Email = req.Email;
                user.PhoneNumber = req.PhoneNumber;
                user.Status = SD.GeneralStatus.ACTIVE;
                user.RoleId = 2;
                user.FullName = req.FullName;
                user.Gender = req.Gender;
                user.DateOfBirth = DateTime.Parse(req.DateOfBirth);
                user.CreatedDate = DateTime.Now;

                var res = await _unitOfWork.AccountRepository.UpdateAsync(user);

                if (req.RoleId == 2)
                {
                    string medicalRecordsPassage = string.Join(".", req.MedicalRecord);

                    var newElderly = new Elderly
                    {
                        AccountId = user.AccountId,
                        MedicalRecord = medicalRecordsPassage,
                        Weight = Decimal.Parse(req.Weight),
                        Height = Decimal.Parse(req.Height),
                        Status = SD.GeneralStatus.ACTIVE,
                    };
                    var rsE = _unitOfWork.ElderlyRepository.CreateAsync(newElderly);
                }

              

                if (res < 0)
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Fail to create new account!");
                }

                return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, "Create new account successfully!");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<IBusinessResult> Login(string email, string password, string deviceToken)
        {
            try
            {
                if (!FunctionCommon.IsValidEmail(email))
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Wrong email format!");
                }
                var user = _unitOfWork.AccountRepository
                                            .FindByCondition(u => u.Email == email)
                                            .FirstOrDefault();
                var hash = SecurityUtil.Hash(password);

                if (user == null || !SecurityUtil.Hash(password).Equals(user.Password))
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Wrong email or password!");
                }

                if (user.IsVerified == false)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Email is registered but not verified!");
                }

                var userRole = _unitOfWork.RoleRepository
                                                .FindByCondition(ur => ur.RoleId == user.RoleId)
                                                .FirstOrDefault();
                user.Role = userRole!;
                if (deviceToken != null)
                {
                    user.DeviceToken = deviceToken;
                }
                _unitOfWork.AccountRepository.Update(user);

                return new BusinessResult(Const.SUCCESS_LOGIN, Const.SUCCESS_LOGIN_MSG, CreateJwtToken(user));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private string CreateJwtToken(Account user)
        {
            var utcNow = DateTime.UtcNow;
            var userRole = _unitOfWork.RoleRepository.FindByCondition(u => u.RoleId == user.RoleId).FirstOrDefault();
            var isInformation = _unitOfWork.AccountRepository
                .FindByCondition(u => u.FullName != null && u.Email.Equals(user.Email))
                .FirstOrDefault();
            var authClaims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.NameId, user.AccountId.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(ClaimTypes.Role, userRole.RoleName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Name, user.FullName ?? "null"),
                new("Avatar", user.Avatar ?? "null"),
                new("IsInformation", isInformation != null ? "true" : "false")
            };

            var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(authClaims),
                SigningCredentials =
                    new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256),
                Expires = utcNow.Add(TimeSpan.FromHours(1)),
            };

            var handler = new JwtSecurityTokenHandler();

            var token = handler.CreateToken(tokenDescriptor);

            var writeToken = new JwtSecurityTokenHandler().WriteToken(token);

            return writeToken;
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
