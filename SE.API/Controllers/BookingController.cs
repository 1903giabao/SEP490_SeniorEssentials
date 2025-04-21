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

        [HttpPut("confirm")]
        public async Task<IActionResult> ConfirmOrder([FromQuery] string apptransid)
        {
            var result = await _bookingService.ConfirmOrder(apptransid);
            return Ok(result);
        }

        [HttpGet("order-status")]
        public async Task<IActionResult> CheckOrderStatus([FromQuery] string appTransId)
        {
            var result = await _bookingService.CheckOrderStatus(appTransId);
            return Ok(result);
        }        
        
        [HttpGet("user-booking/{accountId}")]
        public async Task<IActionResult> CheckIfUserHasBooking([FromRoute] int accountId)
        {
            var result = await _bookingService.CheckIfUserHasBooking(accountId);
            return Ok(result);
        }         
        
        [HttpGet("user-subscription/{accountId}")]
        public async Task<IActionResult> CheckSubscriptionByUser([FromRoute] int accountId)
        {
            var result = await _bookingService.CheckSubscriptionByUser(accountId);
            return Ok(result);
        }       
        
        [HttpGet("bookings/family-member/{familyMemberId}")]
        public async Task<IActionResult> GetListBookingOfFamilyMember([FromRoute] int familyMemberId)
        {
            var result = await _bookingService.GetListBookingOfFamilyMember(familyMemberId);
            return Ok(result);
        }
    }
}
