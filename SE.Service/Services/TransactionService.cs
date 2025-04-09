using AutoMapper;
using SE.Common.DTO;
using SE.Common.Response.Subscription;
using SE.Common;
using SE.Data.UnitOfWork;
using SE.Service.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SE.Common.Response.Transaction;

namespace SE.Service.Services
{
    public interface ITransactionService
    {
        Task<IBusinessResult> GetAllTransaction();
    }

    public class TransactionService : ITransactionService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public TransactionService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IBusinessResult> GetAllTransaction()
        {
            try
            {
                var listTransactions = await _unitOfWork.TransactionRepository.GetAllTransaction();
                var result = listTransactions.Select(t => new GetAllTransactionResponse
                {
                    TransactionId = t.TransactionId,
                    PaymentCode = t.PaymentCode,
                    PaymentDate = t.PaymentDate,
                    PaymentMethod = t.Booking.PaymentMethod,
                    PaymentStatus = t.PaymentStatus,
                    Price = (double)t.Price,
                    SubscriptionDescription = t.Booking.Subscription.Description,
                    SubscriptionName = t.Booking.Subscription.Name,
                    Account = _mapper.Map<UserDTO>(t.Account),
                });

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, ex.Message);
            }
        }
    }
}
