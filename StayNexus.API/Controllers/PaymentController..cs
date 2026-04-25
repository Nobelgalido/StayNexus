using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StayNexus.Core.Interfaces;
using StayNexus.Shared.Requests;
using StayNexus.Shared.Responses;
using System.Security.Claims;

namespace StayNexus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IBookingRepository _bookingRepository;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IPaymentService paymentService,
        IBookingRepository bookingRepository,
        ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _bookingRepository = bookingRepository;
        _logger = logger;
    }

    [HttpPost("checkout")]
    [Authorize]
    public async Task<IActionResult> CreateCheckout(
        [FromBody] CreatePaymentRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Verify the booking belongs to the requesting user
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId);

        if (booking is null)
            return NotFound(new { message = "Booking not found." });

        if (booking.GuestId != userId)
            return Forbid();

        try
        {
            var result = await _paymentService
                .CreateCheckoutSessionAsync(request.BookingId);

            var response = new CheckoutSessionResponse
            {
                CheckoutUrl = result.CheckoutUrl,
                SessionId = result.SessionId
            };

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex,
                "Checkout creation failed for booking {BookingId}",
                request.BookingId);
            return BadRequest(new { message = ex.Message });
        }
    }
}