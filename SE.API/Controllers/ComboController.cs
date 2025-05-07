using Microsoft.AspNetCore.Mvc;
using SE.Common.DTO;
using SE.Service.Services;

namespace SE.API.Controllers
{
    [Route("combo-management")]
    [ApiController]
    public class ComboController : ControllerBase
    {
        private readonly ISubscriptionService _comboService;

        public ComboController(ISubscriptionService comboService)
        {
            _comboService = comboService;
        }

        // POST: combo-management
        [HttpPost]
        public async Task<IActionResult> CreateCombo([FromBody] CreateComboModel model)
        {
            var result = await _comboService.CreateCombo(model);
            return Ok(result);
        }

        [HttpPut("{comboId}")]
        public async Task<IActionResult> UpdateCombo([FromRoute] int comboId, [FromBody] CreateComboModel model)
        {
            var result = await _comboService.UpdateCombo(comboId, model);
            return Ok(result);
        }

        // GET: combo-management
        [HttpGet]
        public async Task<IActionResult> GetAllCombos()
        {
            var result = await _comboService.GetAllCombos();
            return Ok(result);
        }

        // GET: combo-management/{id}
        [HttpGet("{comboId}")]
        public async Task<IActionResult> GetComboById(int comboId)
        {
            var result = await _comboService.GetComboById(comboId);
            return Ok(result);
        }

        [HttpGet("user/{comboId}")]
        public async Task<IActionResult> GetAllUserInCombo(int comboId)
        {
            var result = await _comboService.GetAllUserInCombo(comboId);
            return Ok(result);
        }

        // PUT: combo-management/update/{id}
        [HttpPut("update/{comboId}")]
        public async Task<IActionResult> UpdateComboStatus(int comboId, string status)
        {
            var result = await _comboService.UpdateComboStatus(comboId,status);
            return Ok(result);
        }

        [HttpPut("back/{userSubscriptionId}")]
        public async Task<IActionResult> BackSubscription([FromRoute] int userSubscriptionId)
        {
            var rs = await _comboService.BackSubscription(userSubscriptionId);
            return Ok(rs);
        }        
        
        [HttpGet("one-time-subscription/{elderlyId}")]
        public async Task<IActionResult> CheckIfUserHasOneTimeSubscription([FromRoute] int elderlyId)
        {
            var rs = await _comboService.CheckIfUserHasOneTimeSubscription(elderlyId);
            return Ok(rs);
        }
    }
}
