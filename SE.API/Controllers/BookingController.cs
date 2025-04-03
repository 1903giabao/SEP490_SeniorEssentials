using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using SE.Common.Request.Booking;
using SE.Common.Request.SE.Common.Request;
using SE.Service.Services;
using ZaloPay.Helper.Crypto;

namespace SE.API.Controllers
{
    [Route("booking-management")]
    [ApiController]
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBookingOrder([FromBody] BookingOrderRequest request)
        {
            var result = await _bookingService.CreateBookingOrder(request);
            return Ok(result);
        }

        [HttpGet("confirm")]
        public async Task<IActionResult> ConfirmOrder([FromQuery] string? amount, [FromQuery] string? appid,
                        [FromQuery] string? apptransid,[FromQuery] string? bankcode,[FromQuery] string? checksum, [FromQuery] string? discountamount,
                        [FromQuery] string? pmcid, [FromQuery] string? status)
        {
            var result = await _bookingService.ConfirmOrder(apptransid);
            return Ok(result);
        }

        [HttpGet("order-status")]
        public async Task<IActionResult> CheckOrderStatus([FromRoute] string appTransId)
        {
            var result = await _bookingService.CheckOrderStatus(appTransId);
            return Ok(result);
        }
    }
}
