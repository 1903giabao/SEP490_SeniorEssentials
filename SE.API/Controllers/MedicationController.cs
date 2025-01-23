using Microsoft.AspNetCore.Mvc;
using SE.Service.Services;

namespace SE.API.Controllers
{
    [Route("medication-management")]
    [ApiController]
    public class MedicationController : Controller
    {
        private readonly IMedicationService _medicationService;

        public MedicationController(IMedicationService medicationService)
        {
            _medicationService = medicationService;
        }
    }
}
