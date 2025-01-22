using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Firebase.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Pkcs;
using SE.Common;
using SE.Common.DTO;
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
        Task<bool> SendOtpToUser(string email);
        Task<IBusinessResult> SubmitOTP(CreateUserReq req);
        Task<IBusinessResult> SignupForElderly(ElderlySignUpModel req);
        Task<IBusinessResult> Login(string email, string password, string deviceToken);

    }

    public class IdentityService : IIdentityService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly UnitOfWork _unitOfWork;
        private readonly IFirebaseService _firebaseService;
        private readonly string _confirmUrl;
        private readonly string _frontendUrl;
        private readonly IEmailService _emailService;
        private readonly IAccountService _accountService;

        public IdentityService(UnitOfWork unitOfWork, IOptions<JwtSettings> jwtSettingsOptions, IFirebaseService firebaseService, IEmailService emailService, IAccountService accountService)
        {
            _unitOfWork = unitOfWork;
            _jwtSettings = jwtSettingsOptions.Value;
            _firebaseService = firebaseService;
            _emailService = emailService;
            _accountService = accountService;
        }

        public async Task<bool> SendOtpToUser(string email)
        {
            try
            {
               
                var otp = new Random().Next(100000, 999999);
                var mailData = new EmailData
                {
                    EmailToId = email,
                    EmailToName = "Senior Essentials",
                    EmailBody = otp.ToString(),
                    EmailSubject = "XÁC NHẬN MÃ OTP"
                };

                var emailResult = await _emailService.SendEmailAsync(mailData);
                if (!emailResult)
                {
                    return false;
                }

                var createUserResponse = await _accountService.CreateNewTempAccount(email, otp.ToString());
                var userReponse =(Account) createUserResponse.Data;

                if (userReponse.Otp != otp.ToString())
                {
                    var mailUpdateData = new EmailData
                    {
                        EmailToId = email,
                        EmailToName = "Senior Essentials",
                        EmailBody = otp.ToString(),
                        EmailSubject = "XÁC NHẬN MÃ OTP"
                    };

                    var rsUpdate = await _emailService.SendEmailAsync(mailUpdateData);
                    if (!rsUpdate)
                    {
                        return false;
                    }
                }

                return true;
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
                var user = _unitOfWork.AccountRepository.FindByCondition(u => u.Email.Equals(req.Email)).FirstOrDefault();

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
                user.Status ="Active";
                var result = await _unitOfWork.AccountRepository.UpdateAsync(user);
                rs.Message = "XÁC MINH OTP THÀNH CÔNG!";
                if (result > 0)
                {
                    return rs;
                }
                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Cannot submit OTP");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<IBusinessResult> SignupForElderly(ElderlySignUpModel req)
        {
            try
            {
                if (!FunctionCommon.IsValidEmail(req.Email))
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Wrong email format!");
                }
                var user = _unitOfWork.AccountRepository.FindByCondition(u => u.Email == req.Email).FirstOrDefault();

                if (user != null)
                {
                    if (user.IsVerified == true)
                    {
                        return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Existed email");

                    }
                    else
                    {
                        return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Email is registered but not verified!");
                    }
                }

                var newUser = new Account
                {
                    Email = req.Email,
                    Password = SecurityUtil.Hash(req.Password),
                    FullName = req.FullName,
                    Status = "Active",
                    IsVerified = false,
                    Avatar = "https://static-00.iconduck.com/assets.00/avatar-icon-2048x2048-aiocer4i.png",
                    RoleId = 2
                };

                var res = await _unitOfWork.AccountRepository.CreateAsync(newUser);

       /*         var mailUpdateData = new EmailData
                {
                    EmailToId = req.Email,
                    EmailToName = "Senior Essentials",
                    EmailBody = "Dang ki thanh cong",
                    EmailSubject = "XAC NHAN DANG KI THANH CONG"
                };

                var rsUpdate = await _emailService.SendEmailAsync(mailUpdateData);*/

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
                return new BusinessResult(Const.SUCCESS_LOGIN, Const.SUCCESS_LOGIN_MSG,CreateJwtToken(user));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public SecurityToken CreateJwtToken(Account user)
        {
            try
            {
                var utcNow = DateTime.UtcNow;
                var userRole = _unitOfWork.RoleRepository.FindByCondition(u => u.RoleId == user.RoleId).FirstOrDefault();
                var authClaims = new List<Claim>
          {
              new(JwtRegisteredClaimNames.NameId, user.AccountId.ToString()),
              new(JwtRegisteredClaimNames.Email, user.Email),
              new(ClaimTypes.Role, userRole.RoleName),
              new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
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

                return token;
            }
            catch (Exception ex)
            {
                return null;

            }
        }




    }
}
