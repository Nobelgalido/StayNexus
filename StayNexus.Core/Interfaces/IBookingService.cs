using StayNexus.Core.Models;

namespace StayNexus.Core.Interfaces;

public interface IBookingService
{
    Task<Booking?> GetByIdAsync(int id);
    Task<IEnumerable<Booking>> GetMyBookingsAsync(string guestId);
    Task<bool> CheckAvailabilityAsync(int roomId, DateTime checkIn, DateTime checkOut);
    Task<Booking> CreateBookingAsync(
        string guestId,
        int roomId,
        DateTime checkInDate,
        DateTime checkOutDate,
        int numberOfGuests);
    Task<Booking> CancelBookingAsync(int bookingId, string requestingUserId);
}