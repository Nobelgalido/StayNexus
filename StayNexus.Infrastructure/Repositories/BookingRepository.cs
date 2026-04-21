using Microsoft.EntityFrameworkCore;
using StayNexus.Core.Enums;
using StayNexus.Core.Interfaces;
using StayNexus.Core.Models;
using StayNexus.Infrastructure.Data;

namespace StayNexus.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _context;

    public BookingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Booking?> GetByIdAsync(int id)
    {
        return await _context.Bookings
            .Include(b => b.Guest)
            .Include(b => b.Room)
                .ThenInclude(r => r.Property)
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IEnumerable<Booking>> GetByGuestIdAsync(string guestId)
    {
        return await _context.Bookings
            .Include(b => b.Room)
                .ThenInclude(r => r.Property)
            .Include(b => b.Payment)
            .Where(b => b.GuestId == guestId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetByRoomIdAsync(int roomId)
    {
        return await _context.Bookings
            .Include(b => b.Guest)
            .Where(b => b.RoomId == roomId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> HasOverlappingBookingAsync(
        int roomId,
        DateTime checkIn,
        DateTime checkOut,
        int? excludeBookingId = null)
    {
        return await _context.Bookings
            .Where(b => b.RoomId == roomId
                && b.BookingStatus != BookingStatus.Cancelled
                && (excludeBookingId == null || b.Id != excludeBookingId)
                && b.CheckInDate < checkOut
                && b.CheckOutDate > checkIn)
            .AnyAsync();
    }

    public async Task<Booking> CreateAsync(Booking booking)
    {
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
        return booking;
    }

    public async Task<Booking> UpdateAsync(Booking booking)
    {
        _context.Bookings.Update(booking);
        await _context.SaveChangesAsync();
        return booking;
    }
}