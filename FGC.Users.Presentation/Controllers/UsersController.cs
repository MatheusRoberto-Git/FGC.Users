using FGC.Users.Application.DTOs;
using FGC.Users.Application.UseCases;
using FGC.Users.Presentation.Models.Requests;
using FGC.Users.Presentation.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FGC.Users.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly RegisterUserUseCase _registerUserUseCase;
        private readonly GetUserProfileUseCase _getUserProfileUseCase;
        private readonly ChangePasswordUseCase _changePasswordUseCase;

        public UsersController(RegisterUserUseCase registerUserUseCase, GetUserProfileUseCase getUserProfileUseCase, ChangePasswordUseCase changePasswordUseCase)
        {
            _registerUserUseCase = registerUserUseCase;
            _getUserProfileUseCase = getUserProfileUseCase;
            _changePasswordUseCase = changePasswordUseCase;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<UserResponse>>> Register([FromBody] RegisterUserRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(ApiResponse<object>.ErrorResult("Dados obrigatórios"));

                if (string.IsNullOrWhiteSpace(request.Email))
                    return BadRequest(ApiResponse<object>.ErrorResult("Email é obrigatório"));

                if (string.IsNullOrWhiteSpace(request.Password))
                    return BadRequest(ApiResponse<object>.ErrorResult("Senha é obrigatória"));

                if (string.IsNullOrWhiteSpace(request.Name))
                    return BadRequest(ApiResponse<object>.ErrorResult("Nome é obrigatório"));

                var dto = new RegisterUserDTO
                {
                    Email = request.Email,
                    Password = request.Password,
                    Name = request.Name
                };

                var result = await _registerUserUseCase.ExecuteAsync(dto);

                var response = new UserResponse
                {
                    Id = result.Id,
                    Email = result.Email,
                    Name = result.Name,
                    Role = result.Role,
                    CreatedAt = result.CreatedAt,
                    IsActive = result.IsActive
                };

                return CreatedAtAction(
                    nameof(GetProfile),
                    new { id = result.Id },
                    ApiResponse<UserResponse>.SuccessResult(response, "Usuário cadastrado com sucesso")
                );
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Erro interno do servidor - {ex.Message}"));
            }
        }

        [HttpGet("profile/{id}")]
        [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<UserResponse>>> GetProfile(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResult("ID inválido"));

                var result = await _getUserProfileUseCase.ExecuteAsync(new GetUserProfileDTO { UserId = id });

                var response = new UserResponse
                {
                    Id = result.Id,
                    Email = result.Email,
                    Name = result.Name,
                    Role = result.Role,
                    CreatedAt = result.CreatedAt,
                    IsActive = result.IsActive
                };

                return Ok(ApiResponse<UserResponse>.SuccessResult(response));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Erro interno do servidor - {ex.Message}"));
            }
        }

        [HttpPut("changePassword/{id}")]
        [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<UserResponse>>> ChangePassword(Guid id, [FromBody] ChangePasswordRequest request)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResult("ID inválido"));

                if (request == null)
                    return BadRequest(ApiResponse<object>.ErrorResult("Dados obrigatórios"));

                if (string.IsNullOrWhiteSpace(request.CurrentPassword))
                    return BadRequest(ApiResponse<object>.ErrorResult("Senha atual é obrigatória"));

                if (string.IsNullOrWhiteSpace(request.NewPassword))
                    return BadRequest(ApiResponse<object>.ErrorResult("Nova senha é obrigatória"));

                var dto = new ChangePasswordDTO
                {
                    UserId = id,
                    CurrentPassword = request.CurrentPassword,
                    NewPassword = request.NewPassword
                };

                var result = await _changePasswordUseCase.ExecuteAsync(dto);

                var response = new UserResponse
                {
                    Id = result.Id,
                    Email = result.Email,
                    Name = result.Name,
                    Role = result.Role,
                    CreatedAt = result.CreatedAt,
                    IsActive = result.IsActive
                };

                return Ok(ApiResponse<UserResponse>.SuccessResult(response, "Senha alterada com sucesso"));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
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
    }
}