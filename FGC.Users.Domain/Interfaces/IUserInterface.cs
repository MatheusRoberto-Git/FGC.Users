using FGC.Users.Domain.Entities;
using FGC.Users.Domain.ValueObjects;
using System.Security.Claims;

namespace FGC.Users.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(Guid id);
        Task<User> GetByEmailAsync(Email email);
        Task<bool> ExistsByEmailAsync(Email email);
        Task SaveAsync(User user);
        Task DeleteAsync(Guid id);
        Task<User> GetByIdForUpdateAsync(Guid id);
    }

    public interface IUserUniquenessService
    {
        Task<bool> IsEmailTakenAsync(Email email);
    }

    public interface IJwtService
    {
        string GenerateToken(User user);
        ClaimsPrincipal ValidateToken(string token);
        Guid? GetUserIdFromToken(string token);
    }
}