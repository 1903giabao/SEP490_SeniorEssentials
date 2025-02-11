using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using Org.BouncyCastle.Ocsp;
using SE.Common;
using SE.Common.DTO;
using SE.Data.Models;
using SE.Data.UnitOfWork;
using SE.Service.Base;
using SE.Service.Helper;

namespace SE.Service.Services
{
    public interface IAccountService
    {
        Task<IBusinessResult> CreateNewTempAccount(CreateNewAccountDTO req);
        Task<IBusinessResult> GetAllUsers();

    }

    public class AccountService : IAccountService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AccountService(UnitOfWork unitOfWork, IMapper mapper
)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
                        IsVerified = false
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
                        IsVerified = false
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

        public async Task<IBusinessResult> GetAllUsers()
        {
            var users = _unitOfWork.AccountRepository.GetAll();

            var rs = _mapper.Map<List<UserDTO>>(users);
            return new BusinessResult (Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, rs);
        }

    }
}
