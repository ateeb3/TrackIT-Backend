using TrackIT.Core.Entities;

namespace TrackIT.Core.Interfaces
{
    public interface ITokenService
    {
        Task<string> CreateTokenAsync(AppUser user);
    }
}