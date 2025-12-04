using FGC.Users.Domain.Entities;
using FGC.Users.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FGC.Users.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                .HasColumnName("Id")
                .IsRequired();

            builder.Property(u => u.Email)
                .HasConversion(
                    email => email.Value,
                    value => new Email(value))
                .HasColumnName("Email")
                .HasColumnType("NVARCHAR(254)")
                .HasMaxLength(254)
                .IsRequired();

            builder.Property(u => u.Password)
                .HasConversion(
                    password => password.Value,
                    value => new Password(value))
                .HasColumnName("Password")
                .HasColumnType("NVARCHAR(500)")
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(u => u.Name)
                .HasColumnName("Name")
                .HasColumnType("NVARCHAR(100)")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.Role)
                .HasConversion<int>()
                .HasColumnName("Role")
                .HasColumnType("INT")
                .IsRequired();

            builder.Property(u => u.CreatedAt)
                .HasColumnName("CreatedAt")
                .HasColumnType("DATETIME2")
                .IsRequired();

            builder.Property(u => u.LastLoginAt)
                .HasColumnName("LastLoginAt")
                .HasColumnType("DATETIME2")
                .IsRequired(false);

            builder.Property(u => u.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("BIT")
                .IsRequired()
                .HasDefaultValue(true);

            builder.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");

            builder.HasIndex(u => u.IsActive)
                .HasDatabaseName("IX_Users_IsActive");

            builder.HasIndex(u => u.CreatedAt)
                .HasDatabaseName("IX_Users_CreatedAt");

            builder.Ignore(u => u.DomainEvents);
        }
    }
}