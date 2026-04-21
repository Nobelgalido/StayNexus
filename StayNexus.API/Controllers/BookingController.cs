using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayNexus.Core.Interfaces;
using StayNexus.Core.Models;
using StayNexus.Shared.DTOs;
using StayNexus.Shared.Requests;
using System.Security.Claims;

namespace StayNexus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var booking = await _bookingService.GetByIdAsync(id);

        if (booking is null)
            return NotFound();

        return Ok(MapToDto(booking));
    }

    [HttpGet("my-bookings")]
    public async Task<IActionResult> GetMyBookings()
    {
        var guestId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(guestId))
            return Unauthorized();

        var bookings = await _bookingService.GetMyBookingsAsync(guestId);
        return Ok(bookings.Select(MapToDto));
    }

    [HttpPost("check-availability")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckAvailability(
        [FromBody] CheckAvailabilityRequest request)
    {
        var isAvailable = await _bookingService.CheckAvailabilityAsync(
            request.RoomId,
            request.CheckInDate,
            request.CheckOutDate);

        return Ok(new { isAvailable });
    }

    [HttpPost]
    public async Task<IActionResult> CreateBooking(
        [FromBody] CreateBookingRequest request)
    {
        var guestId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(guestId))
            return Unauthorized();

        try
        {
            var booking = await _bookingService.CreateBookingAsync(
                guestId,
                request.RoomId,
                request.CheckInDate,
                request.CheckOutDate,
                request.NumberOfGuests);

            return CreatedAtAction(
                nameof(GetById),
                new { id = booking.Id },
                MapToDto(booking));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/cancel")]
    public async Task<IActionResult> CancelBooking(int id)
    {
        var requestingUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(requestingUserId))
            return Unauthorized();

        try
        {
            var booking = await _bookingService.CancelBookingAsync(
                id, requestingUserId);

            return Ok(MapToDto(booking));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    private static BookingDto MapToDto(Booking booking)
    {
        return new BookingDto
        {
            Id = booking.Id,
            GuestId = booking.GuestId,
            GuestName = $"{booking.Guest.FirstName} {booking.Guest.LastName}",
            RoomId = booking.RoomId,
            RoomName = booking.Room.Name,
            PropertyName = booking.Room.Property.Name,
            CheckInDate = booking.CheckInDate,
            CheckOutDate = booking.CheckOutDate,
            NumberOfGuests = booking.NumberOfGuests,
            TotalPrice = booking.TotalPrice,
            BookingStatus = booking.BookingStatus,
            PaymentStatus = booking.PaymentStatus,
            CreatedAt = booking.CreatedAt
        };
    }
}