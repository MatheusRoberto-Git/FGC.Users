using FGC.Users.Domain.Common.Events;

namespace FGC.Users.Domain.Events
{
    public class UserCreatedEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime OccurredAt { get; }
        public Guid UserId { get; }
        public string Email { get; }
        public string Name { get; }
        public DateTime CreatedAt { get; }

        public UserCreatedEvent(Guid userId, string email, string name, DateTime createdAt)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId não pode ser vazio", nameof(userId));

            Id = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            UserId = userId;
            Email = email ?? throw new ArgumentNullException(nameof(email));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            CreatedAt = createdAt;
        }
    }

    public class AdminUserCreatedEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime OccurredAt { get; }
        public Guid NewAdminId { get; }
        public string Email { get; }
        public string Name { get; }
        public Guid CreatedByAdminId { get; }
        public DateTime CreatedAt { get; }

        public AdminUserCreatedEvent(Guid newAdminId, string email, string name, Guid createdByAdminId, DateTime createdAt)
        {
            if (newAdminId == Guid.Empty)
                throw new ArgumentException("NewAdminId não pode ser vazio", nameof(newAdminId));

            if (createdByAdminId == Guid.Empty)
                throw new ArgumentException("CreatedByAdminId não pode ser vazio", nameof(createdByAdminId));

            Id = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            NewAdminId = newAdminId;
            Email = email ?? throw new ArgumentNullException(nameof(email));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            CreatedByAdminId = createdByAdminId;
            CreatedAt = createdAt;
        }
    }

    public class UserAuthenticatedEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime OccurredAt { get; }
        public Guid UserId { get; }
        public string Email { get; }
        public DateTime LoginAt { get; }

        public UserAuthenticatedEvent(Guid userId, string email, DateTime loginAt)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId não pode ser vazio", nameof(userId));

            Id = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            UserId = userId;
            Email = email ?? throw new ArgumentNullException(nameof(email));
            LoginAt = loginAt;
        }
    }

    public class PasswordChangedEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime OccurredAt { get; }
        public Guid UserId { get; }
        public DateTime ChangedAt { get; }

        public PasswordChangedEvent(Guid userId, DateTime changedAt)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId não pode ser vazio", nameof(userId));

            Id = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            UserId = userId;
            ChangedAt = changedAt;
        }
    }

    public class UserDeactivatedEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime OccurredAt { get; }
        public Guid UserId { get; }
        public string Email { get; }
        public DateTime DeactivatedAt { get; }

        public UserDeactivatedEvent(Guid userId, string email, DateTime deactivatedAt)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId não pode ser vazio", nameof(userId));

            Id = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            UserId = userId;
            Email = email ?? throw new ArgumentNullException(nameof(email));
            DeactivatedAt = deactivatedAt;
        }
    }

    public class UserReactivatedEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime OccurredAt { get; }
        public Guid UserId { get; }
        public string Email { get; }
        public DateTime ReactivatedAt { get; }

        public UserReactivatedEvent(Guid userId, string email, DateTime reactivatedAt)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId não pode ser vazio", nameof(userId));

            Id = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            UserId = userId;
            Email = email ?? throw new ArgumentNullException(nameof(email));
            ReactivatedAt = reactivatedAt;
        }
    }

    public class UserPromotedToAdminEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime OccurredAt { get; }
        public Guid UserId { get; }
        public string Email { get; }
        public string Name { get; }
        public DateTime PromotedAt { get; }

        public UserPromotedToAdminEvent(Guid userId, string email, string name, DateTime promotedAt)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId não pode ser vazio", nameof(userId));

            Id = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            UserId = userId;
            Email = email ?? throw new ArgumentNullException(nameof(email));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            PromotedAt = promotedAt;
        }
    }
}