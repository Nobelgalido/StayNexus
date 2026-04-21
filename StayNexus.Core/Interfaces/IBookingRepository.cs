using StayNexus.Core.Models;

namespace StayNexus.Core.Interfaces;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(int id);
    Task<IEnumerable<Booking>> GetByGuestIdAsync(string guestId);
    Task<IEnumerable<Booking>> GetByRoomIdAsync(int roomId);
    Task<bool> HasOverlappingBookingAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeBookingId = null);
    Task<Booking> CreateAsync(Booking booking);
    Task<Booking> UpdateAsync(Booking booking);
}