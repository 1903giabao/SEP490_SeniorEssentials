using Microsoft.AspNetCore.Mvc;
using SE.Common.DTO;
using SE.Common.Request;
using SE.Service.Services;
using System.Threading.Tasks;

namespace SE.API.Controllers
{
    [Route("family-ties")]
    [ApiController]
    public class FamilyTieController : ControllerBase
    {
        private readonly IFamilyTieService _familyTieService;

        public FamilyTieController(IFamilyTieService familyTieService)
        {
            _familyTieService = familyTieService;
        }

        // POST: family-ties
        [HttpPost]
        public async Task<IActionResult> CreateFamilyTie([FromBody] CreateFamilyTieRequest request)
        {
            var result = await _familyTieService.CreateFamilyTie(request);
            return Ok(result);
        }

        // GET: family-ties/elderly/{elderlyId}
        [HttpGet("elderly/{elderlyId}")]
        public async Task<IActionResult> GetAllFamilyTiesByElderlyId(int elderlyId)
        {
            var result = await _familyTieService.GetAllFamilyTiesByElderlyId(elderlyId);
            return Ok(result);
        }

        // PUT: family-ties/note/{familyFamilyTieId}
        [HttpPut("note/{familyFamilyTieId}")]
        public async Task<IActionResult> UpdateFamilyTieNote(int familyFamilyTieId, [FromBody] string newNote)
        {
            var result = await _familyTieService.UpdateFamilyTieNote(familyFamilyTieId, newNote);
            return Ok(result);
        }

        // PUT: family-ties/status/{familyFamilyTieId}
        [HttpPut("status/{familyFamilyTieId}")]
        public async Task<IActionResult> UpdateFamilyTieStatus(int familyFamilyTieId)
        {
            var result = await _familyTieService.UpdateFamilyTieStatus(familyFamilyTieId);
            return Ok(result);
        }
    }
}