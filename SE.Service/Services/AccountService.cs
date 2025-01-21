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

        public AccountService() {

            _unitOfWork ??= new UnitOfWork();

        }

        public async Task<IBusinessResult> CreateNewTempAccount (string email, string OTP)
        {
            try
            {
                var tmp = _unitOfWork.AccountRepository.GetAll();


                var existedAcc = await _unitOfWork.AccountRepository.GetByEmailAsync(email);
                if (existedAcc != null)
                {
                    throw new InvalidOperationException("Email is already existed!");
                }

                var newAccount = new Account
                {
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
