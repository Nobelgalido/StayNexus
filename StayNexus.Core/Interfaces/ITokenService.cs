using StayNexus.Core.Models;

namespace StayNexus.Core.Interfaces;

public interface ITokenService
{
    string GenerateToken(ApplicationUser user, string role);
}