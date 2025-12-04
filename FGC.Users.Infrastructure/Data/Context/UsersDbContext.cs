using FGC.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FGC.Users.Infrastructure.Data.Context
{
    public class UsersDbContext : DbContext
    {
        public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options) { }

        public UsersDbContext() : base() { }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsersDbContext).Assembly);
        }
    }
}