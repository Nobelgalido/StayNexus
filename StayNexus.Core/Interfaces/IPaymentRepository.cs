using StayNexus.Core.Models;

namespace StayNexus.Core.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(int id);
    Task<Payment?> GetByBookingIdAsync(int bookingId);
    Task<Payment?> GetByGatewayReferenceIdAsync(string gatewayReferenceId);
    Task<Payment> CreateAsync(Payment payment);
    Task<Payment> UpdateAsync(Payment payment);
}