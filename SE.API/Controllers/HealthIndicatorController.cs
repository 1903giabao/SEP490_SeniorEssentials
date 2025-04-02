using Google.Cloud.Firestore.V1;
using Microsoft.AspNetCore.Mvc;
using SE.Common.DTO;
using SE.Common.Request;
using SE.Common.Request.HealthIndicator;
using SE.Data.Models;
using SE.Service.Services;
using static SE.Common.Request.HealthIndicator.UpdateHealthIndicatorRequest;

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

    [HttpGet("healthIndicator/sleepTime/detail/{accountId}")]
    public async Task<IActionResult> GetSleepTimeDetail(int accountId)
    {
        var result = await _healthIndicatorService.GetSleepTimeDetail(accountId);
        return Ok(result);
    }

    [HttpGet("healthIndicator/bloodOxygen/detail/{accountId}")]
    public async Task<IActionResult> GetBloodOxygenDetail(int accountId)
    {
        var result = await _healthIndicatorService.GetBloodOxygenDetail(accountId);
        return Ok(result);
    }

    [HttpGet("healthIndicator/footStep/detail/{accountId}")]
    public async Task<IActionResult> GetFootStepDetail(int accountId)
    {
        var result = await _healthIndicatorService.GetFootStepDetail(accountId);
        return Ok(result);
    }

    [HttpGet("healthIndicator/caloriesConsumption/detail/{accountId}")]
    public async Task<IActionResult> GetCaloriesConsumptionDetail(int accountId)
    {
        var result = await _healthIndicatorService.GetCaloriesConsumptionDetail(accountId);
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

    [HttpGet("healthIndicator/evaluation/blood-oxygen")]
    public async Task<IActionResult> EvaluateBMI(decimal? bloodOxygen)
    {
        var result = await _healthIndicatorService.EvaluateBloodOxygen(bloodOxygen);
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

    [HttpGet("healthIndicator/evaluation/liver-enzymes")]
    public async Task<IActionResult> EvaluateLiverEnzymes(decimal alt, decimal ast, decimal alp, decimal ggt)
    {
        var result = await _healthIndicatorService.EvaluateLiverEnzymes(alt, ast, alp, ggt);
        return Ok(result);
    }
    [HttpGet("healthIndicator/evaluation/lipid-profile")]
    public async Task<IActionResult> EvaluateLipidProfile(decimal totalCholesterol, decimal ldlCholesterol, decimal hdlCholesterol, decimal triglycerides)
    {
        var result = await _healthIndicatorService.EvaluateLipidProfile(totalCholesterol, ldlCholesterol, hdlCholesterol, triglycerides);
        return Ok(result);
    }
    [HttpGet("healthIndicator/evaluation/kidney-function")]
    public async Task<IActionResult> EvaluateKidneyFunction(decimal creatinine, decimal BUN, decimal eGFR)
    {
        var result = await _healthIndicatorService.EvaluateKidneyFunction(creatinine, BUN, eGFR);
        return Ok(result);
    }
    [HttpGet("healthIndicator/evaluation/blood-glucose")]
    public async Task<IActionResult> EvaluateBloodGlusose(decimal bloodGlucose, string time)
    {
        var result = await _healthIndicatorService.EvaluateBloodGlusose(bloodGlucose, time);
        return Ok(result);
    }
    [HttpGet("healthIndicator/evaluation/log-book/{accountId}")]
    public async Task<IActionResult> GetLogBookResponses(int accountId)
    {
        var result = await _healthIndicatorService.GetLogBookResponses(accountId);
        return Ok(result);
    }

    [HttpPost("calories-consumption")]
    public async Task<IActionResult> CreateCaloriesConsumption([FromBody] CreateCaloriesConsumptionRequest request)
    {
        var result = await _healthIndicatorService.CreateCaloriesComsumption(request);
        return Ok(result);
    }

    [HttpPost("foot-step")]
    public async Task<IActionResult> CreateFootStep([FromBody] CreateFootStepRequest request)
    {
        var result = await _healthIndicatorService.CreateFootstep(request);
        return Ok(result);
    }

    [HttpPost("blood-oxygen")]
    public async Task<IActionResult> CreateBloodOxygen([FromBody] CreateBloodOxygenRequest request)
    {
        var result = await _healthIndicatorService.CreateBloodOxygen(request);
        return Ok(result);
    }

    [HttpPost("sleep-time")]
    public async Task<IActionResult> CreateSleepTime([FromBody] CreateSleepTimeRequest request)
    {
        var result = await _healthIndicatorService.CreateSleepTime(request);
        return Ok(result);
    }
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
    [HttpPut("update-status/sleepTime/{sleepTimeId}")]
    public async Task<IActionResult> UpdateSleepTimeStatus(int sleepTimeId, [FromBody] string status)
    {
        var result = await _healthIndicatorService.UpdateSleepTimeStatus(sleepTimeId, status);
        return Ok(result);
    }

    [HttpPut("update-status/bloodOxygen/{bloodOxygenId}")]
    public async Task<IActionResult> UpdateBloodOxygenStatus(int bloodOxygenId, [FromBody] string status)
    {
        var result = await _healthIndicatorService.UpdateBloodOxygenStatus(bloodOxygenId, status);
        return Ok(result);
    }

    [HttpPut("update-status/footStep/{footStepId}")]
    public async Task<IActionResult> UpdateFootStepStatus(int footStepId, [FromBody] string status)
    {
        var result = await _healthIndicatorService.UpdateFootStepStatus(footStepId, status);
        return Ok(result);
    }

    [HttpPut("update-status/caloriesConsumption/{caloriesConsumptionId}")]
    public async Task<IActionResult> UpdateCaloriesConsumptionStatus(int caloriesConsumptionId, [FromBody] string status)
    {
        var result = await _healthIndicatorService.UpdateCaloriesConsumptionStatus(caloriesConsumptionId, status);
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
    }
    [HttpPut("update-kidney-function/{kidneyFunctionId}")]
    public async Task<IActionResult> UpdateKidneyFunction(int kidneyFunctionId, [FromBody] KidneyFunctionUpdateRequest request, string createdBy)
    {
        var result = await _healthIndicatorService.UpdateKidneyFunction(kidneyFunctionId, request.Creatinine, request.Bun, request.EGfr, createdBy);
        return Ok(result);
    }

    [HttpPut("update-liver-enzymes/{liverEnzymesId}")]
    public async Task<IActionResult> UpdateLiverEnzymes(int liverEnzymesId, [FromBody] LiverEnzymesUpdateRequest request, string createdBy)
    {
        var result = await _healthIndicatorService.UpdateLiverEnzymes(liverEnzymesId, request.Alt, request.Ast, request.Alp, request.Ggt, createdBy);
        return Ok(result);
    }

    [HttpPut("update-lipid-profile/{lipidProfileId}")]
    public async Task<IActionResult> UpdateLipidProfile(int lipidProfileId, [FromBody] LipidProfileUpdateRequest request, string createdBy)
    {
        var result = await _healthIndicatorService.UpdateLipidProfile(lipidProfileId, request.TotalCholesterol, request.LdlCholesterol, request.HdlCholesterol, request.Triglycerides, createdBy);
        return Ok(result);
    }

    [HttpPut("update-blood-glucose/{bloodGlucoseId}")]
    public async Task<IActionResult> UpdateBloodGlucose(int bloodGlucoseId, [FromBody] BloodGlucoseUpdateRequest request, string createdBy)
    {
        var result = await _healthIndicatorService.UpdateBloodGlucose(bloodGlucoseId, request.BloodGlucoseUpdate, request.Time, createdBy);
        return Ok(result);
    }

    [HttpPut("update-heart-rate/{heartRateId}")]
    public async Task<IActionResult> UpdateHeartRate(int heartRateId, [FromBody] int heartRateUpdate, string createdBy)
    {
        var result = await _healthIndicatorService.UpdateHeartRate(heartRateId, heartRateUpdate, createdBy);
        return Ok(result);
    }

    [HttpPut("update-blood-pressure/{bloodPressureId}")]
    public async Task<IActionResult> UpdateBloodPressure(int bloodPressureId, [FromBody] BloodPressureUpdateRequest request, string createdBy)
    {
        var result = await _healthIndicatorService.UpdateBloodPressure(bloodPressureId, request.Systolic, request.Diastolic, createdBy);
        return Ok(result);
    }

    [HttpPut("update-height/{heightId}")]
    public async Task<IActionResult> UpdateHeight(int heightId, [FromBody] decimal heightUpdate, string createdBy)
    {
        var result = await _healthIndicatorService.UpdateHeight(heightId, heightUpdate, createdBy);
        return Ok(result);
    }

    [HttpPut("update-weight/{weightId}")]
    public async Task<IActionResult> UpdateWeight(int weightId, [FromBody] decimal weightUpdate, string createdBy)
    {
        var result = await _healthIndicatorService.UpdateWeight(weightId, weightUpdate, createdBy);
        return Ok(result);
    }



}

