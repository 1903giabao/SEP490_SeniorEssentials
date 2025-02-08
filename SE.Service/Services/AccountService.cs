using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SE.Common;
using SE.Data.Models;
using SE.Data.UnitOfWork;
using SE.Service.Base;

namespace SE.Service.Services
{
    public interface IAccountService
    {
        Task<IBusinessResult> CreateNewTempAccount(string email, string OTP);

    }

    public class AccountService : IAccountService
    {
        private readonly UnitOfWork _unitOfWork;

        public AccountService(UnitOfWork unitOfWork) 
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IBusinessResult> CreateNewTempAccount (string email, string OTP)
        {
            try
            {
                var existedAcc = _unitOfWork.AccountRepository.FindByCondition(e => e.Email.Equals(email)).FirstOrDefault();
                if (existedAcc != null)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE, "Email already existed!", existedAcc);
                }

                var newAccount = new Account
                {
                    RoleId = 2,
                    Email = email,
                    Otp = OTP
                };
                var result = await _unitOfWork.AccountRepository.CreateAsync(newAccount);
                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, newAccount);
                }
                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG,newAccount);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


    }
}
