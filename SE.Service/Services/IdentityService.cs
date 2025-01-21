using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

namespace SE.Service.Services
{
    
    public interface IIdentityService
    {
        Task<bool> SendOtpToUser(string email);
        Task<IBusinessResult> SubmitOTP(CreateUserReq req);

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
    }
}
