using StayNexus.Core.Enums;
using StayNexus.Core.Interfaces;
using StayNexus.Core.Models;

namespace StayNexus.Infrastructure.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomRepository _roomRepository;

    public BookingService(
        IBookingRepository bookingRepository,
        IRoomRepository roomRepository)
    {
        _bookingRepository = bookingRepository;
        _roomRepository = roomRepository;
    }

    public async Task<Booking?> GetByIdAsync(int id)
    {
        return await _bookingRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Booking>> GetMyBookingsAsync(string guestId)
    {
        return await _bookingRepository.GetByGuestIdAsync(guestId);
    }

    public async Task<bool> CheckAvailabilityAsync(
        int roomId,
        DateTime checkIn,
        DateTime checkOut)
    {
        if (checkIn >= checkOut)
            return false;

        if (checkIn.Date < DateTime.UtcNow.Date)
            return false;

        var room = await _roomRepository.GetByIdAsync(roomId);
        if (room is null || !room.IsAvailable)
            return false;

        var hasOverlap = await _bookingRepository
            .HasOverlappingBookingAsync(roomId, checkIn, checkOut);

        return !hasOverlap;
    }

    public async Task<Booking> CreateBookingAsync(
        string guestId,
        int roomId,
        DateTime checkInDate,
        DateTime checkOutDate,
        int numberOfGuests)
    {
        if (checkInDate >= checkOutDate)
            throw new InvalidOperationException(
                "Check-out date must be after check-in date.");

        if (checkInDate.Date < DateTime.UtcNow.Date)
            throw new InvalidOperationException(
                "Check-in date cannot be in the past.");

        var room = await _roomRepository.GetByIdAsync(roomId)
            ?? throw new InvalidOperationException($"Room {roomId} not found.");

        if (!room.IsAvailable)
            throw new InvalidOperationException(
                "This room is not available for booking.");

        if (numberOfGuests > room.MaxGuests)
            throw new InvalidOperationException(
                $"This room allows a maximum of {room.MaxGuests} guests.");

        var hasOverlap = await _bookingRepository
            .HasOverlappingBookingAsync(roomId, checkInDate, checkOutDate);

        if (hasOverlap)
            throw new InvalidOperationException(
                "This room is already booked for the selected dates.");

        var nights = (checkOutDate.Date - checkInDate.Date).Days;
        var totalPrice = nights * room.PricePerNight;

        var booking = new Booking
        {
            GuestId = guestId,
            RoomId = roomId,
            CheckInDate = checkInDate.Date,
            CheckOutDate = checkOutDate.Date,
            NumberOfGuests = numberOfGuests,
            TotalPrice = totalPrice,
            BookingStatus = BookingStatus.Pending,
            PaymentStatus = PaymentStatus.Unpaid,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _bookingRepository.CreateAsync(booking);

        var result = await _bookingRepository.GetByIdAsync(created.Id)
            ?? throw new InvalidOperationException(
                "Failed to retrieve created booking.");

        return result;
    }

    public async Task<Booking> CancelBookingAsync(
        int bookingId,
        string requestingUserId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId)
            ?? throw new InvalidOperationException(
                $"Booking {bookingId} not found.");

        if (booking.GuestId != requestingUserId)
            throw new UnauthorizedAccessException(
                "You are not authorized to cancel this booking.");

        if (booking.BookingStatus == BookingStatus.Cancelled)
            throw new InvalidOperationException(
                "This booking is already cancelled.");

        booking.BookingStatus = BookingStatus.Cancelled;

        return await _bookingRepository.UpdateAsync(booking);
    }
}