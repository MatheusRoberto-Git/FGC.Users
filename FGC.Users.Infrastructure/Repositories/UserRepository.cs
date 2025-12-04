using FGC.Users.Domain.Entities;
using FGC.Users.Domain.Interfaces;
using FGC.Users.Domain.ValueObjects;
using FGC.Users.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FGC.Users.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UsersDbContext _context;

        public UserRepository(UsersDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User> GetByEmailAsync(Email email)
        {
            if (email == null)
                return null;

            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> ExistsByEmailAsync(Email email)
        {
            if (email == null)
                return false;

            return await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.Email == email);
        }

        public async Task SaveAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var existingUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            if (existingUser == null)
            {
                await _context.Users.AddAsync(user);
            }
            else
            {
                _context.Users.Update(user);
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
                return;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user != null)
            {
                user.Deactivate();
                await SaveAsync(user);
            }
        }

        public async Task<User> GetByIdForUpdateAsync(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}
