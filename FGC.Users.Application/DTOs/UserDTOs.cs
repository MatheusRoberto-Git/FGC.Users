namespace FGC.Users.Application.DTOs
{
    public class RegisterUserDTO
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class LoginDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthenticatedUserDTO
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserResponseDTO
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class ChangePasswordDTO
    {
        public Guid UserId { get; set; }
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class GetUserProfileDTO
    {
        public Guid UserId { get; set; }
    }

    public class DeactivateUserDTO
    {
        public Guid UserId { get; set; }
    }

    public class ReactivateUserDTO
    {
        public Guid UserId { get; set; }
    }

    public class CreateAdminUserDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid CreatedByAdminId { get; set; }
    }

    public class PromoteUserToAdminDTO
    {
        public Guid UserId { get; set; }
        public Guid AdminId { get; set; }
    }

    public class DemoteAdminToUserDTO
    {
        public Guid AdminId { get; set; }
        public Guid RequestingAdminId { get; set; }
    }
}