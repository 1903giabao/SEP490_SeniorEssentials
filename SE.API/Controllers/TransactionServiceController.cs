using Microsoft.AspNetCore.Mvc;
using SE.Service.Services;

namespace SE.API.Controllers
{
    [Route("transaction-management")]
    [ApiController]
    public class TransactionServiceController : Controller
    {
        private readonly ITransactionService _transactionService;

        public TransactionServiceController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }
    }
}
