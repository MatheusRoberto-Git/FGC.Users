using FGC.Users.Domain.Common.Entities;
using FGC.Users.Domain.Enums;
using FGC.Users.Domain.Events;
using FGC.Users.Domain.ValueObjects;

namespace FGC.Users.Domain.Entities
{
    public class User : AggregateRoot
    {
        #region [Properties]

        public Email Email { get; private set; }
        public Password Password { get; private set; }
        public string Name { get; private set; }
        public UserRole Role { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        public bool IsActive { get; private set; }

        #endregion

        #region [Constructor]

        private User() : base() { }

        private User(Email email, Password password, string name, UserRole role = UserRole.User) : base()
        {
            Email = email ?? throw new ArgumentNullException(nameof(email));
            Password = password ?? throw new ArgumentNullException(nameof(password));
            Name = ValidateName(name);
            Role = role;
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
        }

        #endregion

        #region [Factory Methods]

        public static User Create(Email email, Password password, string name)
        {
            var user = new User(email, password, name);

            user.AddDomainEvent(new UserCreatedEvent(
                user.Id,
                user.Email.Value,
                user.Name,
                user.CreatedAt
            ));

            return user;
        }

        public static User CreateAdmin(Email email, Password password, string name, Guid? createdByAdminId = null)
        {
            var user = new User(email, password, name, UserRole.Admin);

            if (createdByAdminId.HasValue && createdByAdminId.Value != Guid.Empty)
            {
                user.AddDomainEvent(new AdminUserCreatedEvent(
                    user.Id,
                    user.Email.Value,
                    user.Name,
                    createdByAdminId.Value,
                    user.CreatedAt
                ));
            }
            else
            {
                user.AddDomainEvent(new UserCreatedEvent(
                    user.Id,
                    user.Email.Value,
                    user.Name,
                    user.CreatedAt
                ));
            }

            return user;
        }

        #endregion

        #region [Business Methods]

        public void ChangePassword(Password newPassword)
        {
            if (newPassword == null)
                throw new ArgumentNullException(nameof(newPassword));

            if (!IsActive)
                throw new InvalidOperationException("Usuário inativo não pode alterar senha");

            Password = newPassword;

            AddDomainEvent(new PasswordChangedEvent(Id, DateTime.UtcNow));
        }

        public void UpdateName(string newName)
        {
            if (!IsActive)
                throw new InvalidOperationException("Usuário inativo não pode alterar dados");

            Name = ValidateName(newName);
        }

        public void RecordLogin()
        {
            if (!IsActive)
                throw new InvalidOperationException("Usuário inativo não pode fazer login");

            LastLoginAt = DateTime.UtcNow;

            AddDomainEvent(new UserAuthenticatedEvent(Id, Email.Value, DateTime.UtcNow));
        }

        public void Deactivate()
        {
            if (!IsActive)
                throw new InvalidOperationException("Usuário já está inativo");

            IsActive = false;

            AddDomainEvent(new UserDeactivatedEvent(Id, Email.Value, DateTime.UtcNow));
        }

        public void Reactivate()
        {
            if (IsActive)
                throw new InvalidOperationException("Usuário já está ativo");

            IsActive = true;

            AddDomainEvent(new UserReactivatedEvent(Id, Email.Value, DateTime.UtcNow));
        }

        public void PromoteToAdmin()
        {
            if (Role == UserRole.Admin)
                throw new InvalidOperationException("Usuário já é administrador");

            if (!IsActive)
                throw new InvalidOperationException("Usuário inativo não pode ser promovido");

            Role = UserRole.Admin;

            AddDomainEvent(new UserPromotedToAdminEvent(
                Id,
                Email.Value,
                Name,
                DateTime.UtcNow
            ));
        }

        public void DemoteToUser()
        {
            if (Role == UserRole.User)
                throw new InvalidOperationException("Usuário já é usuário comum");

            Role = UserRole.User;
        }

        #endregion

        #region [Validações / Suporte]

        public bool IsAdmin() => Role == UserRole.Admin;

        public bool CanLogin() => IsActive;

        private static string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nome é obrigatório", nameof(name));

            if (name.Length < 2)
                throw new ArgumentException("Nome deve ter pelo menos 2 caracteres", nameof(name));

            if (name.Length > 100)
                throw new ArgumentException("Nome deve ter no máximo 100 caracteres", nameof(name));

            return name.Trim();
        }

        #endregion
    }
}