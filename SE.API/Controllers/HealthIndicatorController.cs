using Microsoft.AspNetCore.Mvc;
using SE.Common.DTO;
using SE.Common.Request;
using SE.Common.Request.HealthIndicator;
using SE.Data.Models;
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

    [HttpGet("healthIndicator/weight/detail/{accountId}")]
    public async Task<IActionResult> GetAllHealthIndicatorsByElderlyId(int accountId)
    {
        var result = await _healthIndicatorService.GetWeightDetail(accountId);
        return Ok(result);
    }

    [HttpGet("healthIndicator/height/detail/{accountId}")]
    public async Task<IActionResult> GetHeightDetail(int accountId)
    {
        var result = await _healthIndicatorService.GetHeightDetail(accountId);
        return Ok(result);
    }

    [HttpGet("healthIndicator/heartRate/detail/{accountId}")]
    public async Task<IActionResult> GetHeartRateDetail(int accountId)
    {
        var result = await _healthIndicatorService.GetHeartRateDetail(accountId);
        return Ok(result);
    }

    [HttpGet("healthIndicator/blood-pressure/detail/{accountId}")]
    public async Task<IActionResult> GetBloodPressureDetail(int accountId)
    {
        var result = await _healthIndicatorService.GetBloodPressureDetail(accountId);
        return Ok(result);
    }
    [HttpGet("healthIndicator/blood-glucose/detail/{accountId}")]
    public async Task<IActionResult> GetBloodGlucoseDetail(int accountId)
    {
        var result = await _healthIndicatorService.GetBloodGlucoseDetail(accountId);
        return Ok(result);
    }
    [HttpGet("healthIndicator/lipid-profile/detail/{accountId}")]
    public async Task<IActionResult> GetLipidProfileDetail(int accountId)
    {
        var result = await _healthIndicatorService.GetLipidProfileDetail(accountId);
        return Ok(result);
    }

    [HttpGet("healthIndicator/liver-enzymes/detail/{accountId}")]
    public async Task<IActionResult> GetLiverEnzymesDetail(int accountId)
    {
        var result = await _healthIndicatorService.GetLiverEnzymesDetail(accountId);
        return Ok(result);
    }

    [HttpGet("healthIndicator/kidney-function/detail/{accountId}")]
    public async Task<IActionResult> GetKidneyFunctionDetail(int accountId)
    {
        var result = await _healthIndicatorService.GetKidneyFunctionDetail(accountId);
        return Ok(result);
    }
    [HttpGet("healthIndicator/{accountId}")]
    public async Task<IActionResult> GetAllHealthIndicators(int accountId)
    {
        var result = await _healthIndicatorService.GetAllHealthIndicators(accountId);
        return Ok(result);
    }

    [HttpGet("healthIndicator/evaluation/bmi")]
    public async Task<IActionResult> EvaluateBMI(decimal? height,decimal? weight,int accountId)
    {
        var result = await _healthIndicatorService.EvaluateBMI(height,weight,accountId);
        return Ok(result);
    }
    [HttpGet("healthIndicator/evaluation/heart-rate")]
    public async Task<IActionResult> EvaluateHeartRate(int heartRate)
    {
        var result = await _healthIndicatorService.EvaluateHeartRate(heartRate);
        return Ok(result);
    }
    [HttpGet("healthIndicator/evaluation/blood-pressure")]
    public async Task<IActionResult> GetAllHealthIndicators(int systolic, int diastolic )
    {
        var result = await _healthIndicatorService.EvaluateBloodPressure(systolic, diastolic);
        return Ok(result);
    }
    /*
        [HttpGet("weight/weightId")]
        public async Task<IActionResult> GetWeightHeightById(int weightId)
        {
            var result = await _healthIndicatorService.GetWeightById(weightId);
            return Ok(result);
        }    

        [HttpGet("height/heightId")]
        public async Task<IActionResult> GetHeightById(int heightId)
        {
            var result = await _healthIndicatorService.GetHeightById(heightId);
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
        [HttpPost("weight")]
        public async Task<IActionResult> CreateWeight([FromBody] CreateWeightRequest request)
        {
            var result = await _healthIndicatorService.CreateWeight(request);
            return Ok(result);
        }    

        [HttpPost("height")]
        public async Task<IActionResult> CreateHeight([FromBody] CreateHeightRequest request)
        {
            var result = await _healthIndicatorService.CreateHeight(request);
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

        [HttpPut("update-status/weight/{weightId}")]
        public async Task<IActionResult> UpdateWeightStatus(int weightId, [FromBody] string status)
        {
            var result = await _healthIndicatorService.UpdateWeightStatus(weightId, status);
            return Ok(result);
        }    

        [HttpPut("update-status/height/{heightId}")]
        public async Task<IActionResult> UpdateHeightStatus(int heightId, [FromBody] string status)
        {
            var result = await _healthIndicatorService.UpdateHeightStatus(heightId, status);
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
        }*/
}

