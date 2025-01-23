using Microsoft.AspNetCore.Mvc;
using SE.Service.Services;

namespace SE.API.Controllers
{
    [Route("family-tie-management")]
    [ApiController]
    public class FamilyTieController : Controller
    {
        private readonly IFamilyTieService _familyTieService;

        public FamilyTieController(IFamilyTieService familyTieService)
        {
            _familyTieService = familyTieService;
        }
    }
}
