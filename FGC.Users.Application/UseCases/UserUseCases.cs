using FGC.Users.Application.DTOs;
using FGC.Users.Domain.Entities;
using FGC.Users.Domain.Interfaces;
using FGC.Users.Domain.ValueObjects;

namespace FGC.Users.Application.UseCases
{
    public class RegisterUserUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserUniquenessService _uniquenessService;

        public RegisterUserUseCase(IUserRepository userRepository, IUserUniquenessService uniquenessService)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _uniquenessService = uniquenessService ?? throw new ArgumentNullException(nameof(uniquenessService));
        }

        public async Task<UserResponseDTO> ExecuteAsync(RegisterUserDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var email = new Email(dto.Email);
            var password = new Password(dto.Password);

            if (await _uniquenessService.IsEmailTakenAsync(email))
                throw new InvalidOperationException($"Email {email.Value} já está em uso");

            var user = User.Create(email, password, dto.Name);

            await _userRepository.SaveAsync(user);
            user.ClearDomainEvents();

            return MapToResponseDto(user);
        }

        private static UserResponseDTO MapToResponseDto(User user)
        {
            return new UserResponseDTO
            {
                Id = user.Id,
                Email = user.Email.Value,
                Name = user.Name,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };
        }
    }

    public class AuthenticateUserUseCase
    {
        private readonly IUserRepository _userRepository;

        public AuthenticateUserUseCase(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<AuthenticatedUserDTO> ExecuteAsync(LoginDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var email = new Email(dto.Email);
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
                throw new UnauthorizedAccessException("Email ou senha inválidos");

            if (!user.CanLogin())
                throw new UnauthorizedAccessException("Usuário inativo");

            if (user.Password.Value != dto.Password)
                throw new UnauthorizedAccessException("Email ou senha inválidos");

            user.RecordLogin();
            await _userRepository.SaveAsync(user);
            user.ClearDomainEvents();

            return new AuthenticatedUserDTO
            {
                Id = user.Id,
                Email = user.Email.Value,
                Name = user.Name,
                Role = user.Role.ToString(),
                LastLoginAt = user.LastLoginAt,
                IsActive = user.IsActive
            };
        }
    }

    public class GetUserProfileUseCase
    {
        private readonly IUserRepository _userRepository;

        public GetUserProfileUseCase(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<UserResponseDTO> ExecuteAsync(GetUserProfileDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var user = await _userRepository.GetByIdAsync(dto.UserId);

            if (user == null)
                throw new InvalidOperationException($"Usuário com ID {dto.UserId} não encontrado");

            if (!user.IsActive)
                throw new InvalidOperationException("Usuário inativo");

            return new UserResponseDTO
            {
                Id = user.Id,
                Email = user.Email.Value,
                Name = user.Name,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };
        }
    }

    public class ChangePasswordUseCase
    {
        private readonly IUserRepository _userRepository;

        public ChangePasswordUseCase(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<UserResponseDTO> ExecuteAsync(ChangePasswordDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var user = await _userRepository.GetByIdAsync(dto.UserId);

            if (user == null)
                throw new InvalidOperationException($"Usuário com ID {dto.UserId} não encontrado");

            if (!user.IsActive)
                throw new InvalidOperationException("Usuário inativo não pode alterar senha");

            if (user.Password.Value != dto.CurrentPassword)
                throw new UnauthorizedAccessException("Senha atual incorreta");

            var newPassword = new Password(dto.NewPassword);
            user.ChangePassword(newPassword);

            await _userRepository.SaveAsync(user);
            user.ClearDomainEvents();

            return new UserResponseDTO
            {
                Id = user.Id,
                Email = user.Email.Value,
                Name = user.Name,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };
        }
    }

    public class DeactivateUserUseCase
    {
        private readonly IUserRepository _userRepository;

        public DeactivateUserUseCase(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<UserResponseDTO> ExecuteAsync(DeactivateUserDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var user = await _userRepository.GetByIdAsync(dto.UserId);

            if (user == null)
                throw new InvalidOperationException($"Usuário com ID {dto.UserId} não encontrado");

            if (!user.IsActive)
                throw new InvalidOperationException("Usuário já está inativo");

            user.Deactivate();

            await _userRepository.SaveAsync(user);
            user.ClearDomainEvents();

            return new UserResponseDTO
            {
                Id = user.Id,
                Email = user.Email.Value,
                Name = user.Name,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };
        }
    }

    public class ReactivateUserUseCase
    {
        private readonly IUserRepository _userRepository;

        public ReactivateUserUseCase(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<UserResponseDTO> ExecuteAsync(ReactivateUserDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var user = await _userRepository.GetByIdAsync(dto.UserId);

            if (user == null)
                throw new InvalidOperationException($"Usuário com ID {dto.UserId} não encontrado");

            if (user.IsActive)
                throw new InvalidOperationException("Usuário já está ativo");

            user.Reactivate();

            await _userRepository.SaveAsync(user);
            user.ClearDomainEvents();

            return new UserResponseDTO
            {
                Id = user.Id,
                Email = user.Email.Value,
                Name = user.Name,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };
        }
    }

    public class CreateAdminUserUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserUniquenessService _uniquenessService;

        public CreateAdminUserUseCase(IUserRepository userRepository, IUserUniquenessService uniquenessService)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _uniquenessService = uniquenessService ?? throw new ArgumentNullException(nameof(uniquenessService));
        }

        public async Task<UserResponseDTO> ExecuteAsync(CreateAdminUserDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var creatorAdmin = await _userRepository.GetByIdAsync(dto.CreatedByAdminId);

            if (creatorAdmin == null)
                throw new UnauthorizedAccessException("Administrador criador não encontrado");

            if (!creatorAdmin.IsAdmin())
                throw new UnauthorizedAccessException("Apenas administradores podem criar outros administradores");

            if (!creatorAdmin.IsActive)
                throw new UnauthorizedAccessException("Administrador inativo não pode criar outros usuários");

            var email = new Email(dto.Email);
            var password = new Password(dto.Password);

            if (await _uniquenessService.IsEmailTakenAsync(email))
                throw new InvalidOperationException($"Email {email.Value} já está em uso");

            var newAdmin = User.CreateAdmin(email, password, dto.Name, dto.CreatedByAdminId);

            await _userRepository.SaveAsync(newAdmin);
            newAdmin.ClearDomainEvents();

            return new UserResponseDTO
            {
                Id = newAdmin.Id,
                Email = newAdmin.Email.Value,
                Name = newAdmin.Name,
                Role = newAdmin.Role.ToString(),
                CreatedAt = newAdmin.CreatedAt,
                IsActive = newAdmin.IsActive
            };
        }
    }

    public class PromoteUserToAdminUseCase
    {
        private readonly IUserRepository _userRepository;

        public PromoteUserToAdminUseCase(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<UserResponseDTO> ExecuteAsync(PromoteUserToAdminDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var admin = await _userRepository.GetByIdAsync(dto.AdminId);
            if (admin == null)
                throw new UnauthorizedAccessException("Administrador não encontrado");

            if (!admin.IsAdmin())
                throw new UnauthorizedAccessException("Apenas administradores podem promover outros usuários");

            if (!admin.IsActive)
                throw new UnauthorizedAccessException("Administrador inativo não pode realizar esta operação");

            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null)
                throw new InvalidOperationException($"Usuário com ID {dto.UserId} não encontrado");

            if (!user.IsActive)
                throw new InvalidOperationException("Usuário inativo não pode ser promovido");

            user.PromoteToAdmin();

            await _userRepository.SaveAsync(user);
            user.ClearDomainEvents();

            return new UserResponseDTO
            {
                Id = user.Id,
                Email = user.Email.Value,
                Name = user.Name,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };
        }
    }

    public class DemoteAdminToUserUseCase
    {
        private readonly IUserRepository _userRepository;

        public DemoteAdminToUserUseCase(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<UserResponseDTO> ExecuteAsync(DemoteAdminToUserDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var requestingAdmin = await _userRepository.GetByIdAsync(dto.RequestingAdminId);
            if (requestingAdmin == null)
                throw new UnauthorizedAccessException("Administrador solicitante não encontrado");

            if (!requestingAdmin.IsAdmin())
                throw new UnauthorizedAccessException("Apenas administradores podem despromover outros usuários");

            if (!requestingAdmin.IsActive)
                throw new UnauthorizedAccessException("Administrador inativo não pode realizar esta operação");

            var adminToDemote = await _userRepository.GetByIdAsync(dto.AdminId);
            if (adminToDemote == null)
                throw new InvalidOperationException($"Usuário com ID {dto.AdminId} não encontrado");

            if (!adminToDemote.IsActive)
                throw new InvalidOperationException("Usuário inativo não pode ser despromovido");

            if (!adminToDemote.IsAdmin())
                throw new InvalidOperationException("Usuário não é administrador");

            if (dto.AdminId == dto.RequestingAdminId)
                throw new InvalidOperationException("Administrador não pode despromover a si mesmo");

            adminToDemote.DemoteToUser();

            await _userRepository.SaveAsync(adminToDemote);
            adminToDemote.ClearDomainEvents();

            return new UserResponseDTO
            {
                Id = adminToDemote.Id,
                Email = adminToDemote.Email.Value,
                Name = adminToDemote.Name,
                Role = adminToDemote.Role.ToString(),
                CreatedAt = adminToDemote.CreatedAt,
                IsActive = adminToDemote.IsActive
            };
        }
    }
}