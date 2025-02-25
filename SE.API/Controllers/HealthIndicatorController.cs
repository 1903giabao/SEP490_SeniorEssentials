using Microsoft.AspNetCore.Mvc;
using SE.Common.DTO;
using SE.Common.Request;
using SE.Service.Services;

[ApiController]
[Route("api/[controller]")]
public class HealthIndicatorController : ControllerBase
{
    private readonly IHealthIndicatorService _healthIndicatorService;

    public HealthIndicatorController(IHealthIndicatorService healthIndicatorService)
    {
        _healthIndicatorService = healthIndicatorService;
    }

    // Get endpoints
    [HttpGet("weight-height/elderlyId")]
    public async Task<IActionResult> GetWeightHeightByElderlyId(int elderlyId)
    {
        var result = await _healthIndicatorService.GetWeightHeightByElderlyId(elderlyId);
        return Ok(result);
    }

    [HttpGet("blood-pressure/elderlyId")]
    public async Task<IActionResult> GetBloodPressureByElderlyId(int elderlyId)
    {
        var result = await _healthIndicatorService.GetBloodPressureByElderlyId(elderlyId);
        return Ok(result);
    }

    [HttpGet("heart-rate/elderlyId")]
    public async Task<IActionResult> GetHeartRateByElderlyId(int elderlyId)
    {
        var result = await _healthIndicatorService.GetHeartRateByElderlyId(elderlyId);
        return Ok(result);
    }

    [HttpGet("blood-glucose/elderlyId")]
    public async Task<IActionResult> GetBloodGlucoseByElderlyId(int elderlyId)
    {
        var result = await _healthIndicatorService.GetBloodGlucoseByElderlyId(elderlyId);
        return Ok(result);
    }

    [HttpGet("lipid-profile/elderlyId")]
    public async Task<IActionResult> GetLipidProfileByElderlyId(int elderlyId)
    {
        var result = await _healthIndicatorService.GetLipidProfileByElderlyId(elderlyId);
        return Ok(result);
    }

    [HttpGet("liver-enzymes/elderlyId")]
    public async Task<IActionResult> GetLiverEnzymesByElderlyId(int elderlyId)
    {
        var result = await _healthIndicatorService.GetLiverEnzymesByElderlyId(elderlyId);
        return Ok(result);
    }

    [HttpGet("kidney-function/elderlyId")]
    public async Task<IActionResult> GetKidneyFunctionByElderlyId(int elderlyId)
    {
        var result = await _healthIndicatorService.GetKidneyFunctionByElderlyId(elderlyId);
        return Ok(result);
    }

    [HttpGet("weight-height/weightHeightId")]
    public async Task<IActionResult> GetWeightHeightById(int weightHeightId)
    {
        var result = await _healthIndicatorService.GetWeightHeightById(weightHeightId);
        return Ok(result);
    }

    [HttpGet("blood-pressure/bloodPressureId")]
    public async Task<IActionResult> GetBloodPressureById(int bloodPressureId)
    {
        var result = await _healthIndicatorService.GetBloodPressureById(bloodPressureId);
        return Ok(result);
    }

    [HttpGet("heart-rate/heartRateId")]
    public async Task<IActionResult> GetHeartRateById(int heartRateId)
    {
        var result = await _healthIndicatorService.GetHeartRateById(heartRateId);
        return Ok(result);
    }

    [HttpGet("blood-glucose/bloodGlucoseId")]
    public async Task<IActionResult> GetBloodGlucoseById(int bloodGlucoseId)
    {
        var result = await _healthIndicatorService.GetBloodGlucoseById(bloodGlucoseId);
        return Ok(result);
    }

    [HttpGet("lipid-profile/lipidProfileId")]
    public async Task<IActionResult> GetLipidProfileById(int lipidProfileId)
    {
        var result = await _healthIndicatorService.GetLipidProfileById(lipidProfileId);
        return Ok(result);
    }

    [HttpGet("liver-enzymes/liverEnzymeId")]
    public async Task<IActionResult> GetLiverEnzymesById(int liverEnzymeId)
    {
        var result = await _healthIndicatorService.GetLiverEnzymesById(liverEnzymeId);
        return Ok(result);
    }

    [HttpGet("kidney-function/kidneyFunctionId")]
    public async Task<IActionResult> GetKidneyFunctionById(int kidneyFunctionId)
    {
        var result = await _healthIndicatorService.GetKidneyFunctionById(kidneyFunctionId);
        return Ok(result);
    }

    // Create endpoints
    [HttpPost("weight-height")]
    public async Task<IActionResult> CreateWeightHeight([FromBody] CreateWeightHeightRequest request)
    {
        var result = await _healthIndicatorService.CreateWeightHeight(request);
        return Ok(result);
    }

    [HttpPost("blood-pressure")]
    public async Task<IActionResult> CreateBloodPressure([FromBody] CreateBloodPressureRequest request)
    {
        var result = await _healthIndicatorService.CreateBloodPressure(request);
        return Ok(result);
    }

    [HttpPost("heart-rate")]
    public async Task<IActionResult> CreateHeartRate([FromBody] CreateHeartRateRequest request)
    {
        var result = await _healthIndicatorService.CreateHeartRate(request);
        return Ok(result);
    }

    [HttpPost("blood-glucose")]
    public async Task<IActionResult> CreateBloodGlucose([FromBody] CreateBloodGlucoseRequest request)
    {
        var result = await _healthIndicatorService.CreateBloodGlucose(request);
        return Ok(result);
    }

    [HttpPost("lipid-profile")]
    public async Task<IActionResult> CreateLipidProfile([FromBody] CreateLipidProfileRequest request)
    {
        var result = await _healthIndicatorService.CreateLipidProfile(request);
        return Ok(result);
    }

    [HttpPost("liver-enzymes")]
    public async Task<IActionResult> CreateLiverEnzymes([FromBody] CreateLiverEnzymesRequest request)
    {
        var result = await _healthIndicatorService.CreateLiverEnzymes(request);
        return Ok(result);
    }

    [HttpPost("kidney-function")]
    public async Task<IActionResult> CreateKidneyFunction([FromBody] CreateKidneyFunctionRequest request)
    {
        var result = await _healthIndicatorService.CreateKidneyFunction(request);
        return Ok(result);
    }

    [HttpPut("update-status/weight-height/{weightHeightId}")]
    public async Task<IActionResult> UpdateWeightHeightStatus(int weightHeightId, [FromBody] string status)
    {
        var result = await _healthIndicatorService.UpdateWeightHeightStatus(weightHeightId, status);
        return Ok(result);
    }

    [HttpPut("update-status/blood-pressure/{bloodPressureId}")]
    public async Task<IActionResult> UpdateBloodPressureStatus(int bloodPressureId, [FromBody] string status)
    {
        var result = await _healthIndicatorService.UpdateBloodPressureStatus(bloodPressureId, status);
        return Ok(result);
    }

    [HttpPut("update-status/heart-rate/{heartRateId}")]
    public async Task<IActionResult> UpdateHeartRateStatus(int heartRateId, [FromBody] string status)
    {
        var result = await _healthIndicatorService.UpdateHeartRateStatus(heartRateId, status);
        return Ok(result);
    }

    [HttpPut("update-status/blood-glucose/{bloodGlucoseId}")]
    public async Task<IActionResult> UpdateBloodGlucoseStatus(int bloodGlucoseId, [FromBody] string status)
    {
        var result = await _healthIndicatorService.UpdateBloodGlucoseStatus(bloodGlucoseId, status);
        return Ok(result);
    }

    [HttpPut("update-status/lipid-profile/{lipidProfileId}")]
    public async Task<IActionResult> UpdateLipidProfileStatus(int lipidProfileId, [FromBody] string status)
    {
        var result = await _healthIndicatorService.UpdateLipidProfileStatus(lipidProfileId, status);
        return Ok(result);
    }

    [HttpPut("update-status/liver-enzymes/{liverEnzymesId}")]
    public async Task<IActionResult> UpdateLiverEnzymesStatus(int liverEnzymesId, [FromBody] string status)
    {
        var result = await _healthIndicatorService.UpdateLiverEnzymesStatus(liverEnzymesId, status);
        return Ok(result);
    }
    [HttpPut("update-status/kidney-function/{kidneyFunctionId}")]
    public async Task<IActionResult> UpdateKidneyFunctionStatus(int kidneyFunctionId, [FromBody] string status)
    {
        var result = await _healthIndicatorService.UpdateKidneyFunctionStatus(kidneyFunctionId, status);
        return Ok(result);
    }
}

