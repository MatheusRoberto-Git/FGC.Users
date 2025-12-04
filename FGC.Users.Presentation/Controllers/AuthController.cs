using FGC.Users.Application.DTOs;
using FGC.Users.Application.UseCases;
using FGC.Users.Domain.Interfaces;
using FGC.Users.Presentation.Models.Requests;
using FGC.Users.Presentation.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FGC.Users.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthenticateUserUseCase _authenticateUserUseCase;
        private readonly IJwtService _jwtService;
        private readonly IUserRepository _userRepository;

        public AuthController(AuthenticateUserUseCase authenticateUserUseCase, IJwtService jwtService, IUserRepository userRepository)
        {
            _authenticateUserUseCase = authenticateUserUseCase;
            _jwtService = jwtService;
            _userRepository = userRepository;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(ApiResponse<object>.ErrorResult("Dados de login são obrigatórios"));

                if (string.IsNullOrWhiteSpace(request.Email))
                    return BadRequest(ApiResponse<object>.ErrorResult("Email é obrigatório"));

                if (string.IsNullOrWhiteSpace(request.Password))
                    return BadRequest(ApiResponse<object>.ErrorResult("Senha é obrigatória"));

                var dto = new LoginDTO
                {
                    Email = request.Email,
                    Password = request.Password
                };

                var authenticatedUser = await _authenticateUserUseCase.ExecuteAsync(dto);
                var user = await _userRepository.GetByIdAsync(authenticatedUser.Id);

                if (user == null)
                    throw new InvalidOperationException("Usuário não encontrado após autenticação");

                var token = _jwtService.GenerateToken(user);
                var expiresAt = DateTime.UtcNow.AddMinutes(120);

                var response = new AuthResponse
                {
                    User = new UserResponse
                    {
                        Id = authenticatedUser.Id,
                        Email = authenticatedUser.Email,
                        Name = authenticatedUser.Name,
                        Role = authenticatedUser.Role,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = authenticatedUser.IsActive
                    },
                    Token = token,
                    ExpiresAt = expiresAt,
                    LastLoginAt = authenticatedUser.LastLoginAt ?? DateTime.UtcNow
                };

                return Ok(ApiResponse<AuthResponse>.SuccessResult(response, "Login realizado com sucesso"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Erro interno do servidor - {ex.Message}"));
            }
        }

        [HttpPost("logout")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public ActionResult<ApiResponse<object>> Logout()
        {
            return Ok(ApiResponse<object>.SuccessResult(null, "Logout realizado com sucesso"));
        }

        [HttpPost("validateToken")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<object>> ValidateToken([FromBody] string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return BadRequest(ApiResponse<object>.ErrorResult("Token é obrigatório"));

                var principal = _jwtService.ValidateToken(token);

                if (principal == null)
                    return Unauthorized(ApiResponse<object>.ErrorResult("Token inválido ou expirado"));

                var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var email = principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                var role = principal.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                return Ok(ApiResponse<object>.SuccessResult(new { UserId = userId, Email = email, Role = role }, "Token válido"));
            }
            catch (Exception ex)
            {
                return Unauthorized(ApiResponse<object>.ErrorResult($"Token inválido - {ex.Message}"));
            }
        }
    }
}