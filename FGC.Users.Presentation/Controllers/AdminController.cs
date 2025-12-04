using FGC.Users.Application.DTOs;
using FGC.Users.Application.UseCases;
using FGC.Users.Presentation.Models.Requests;
using FGC.Users.Presentation.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FGC.Users.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly PromoteUserToAdminUseCase _promoteUserUseCase;
        private readonly DemoteAdminToUserUseCase _demoteAdminUseCase;
        private readonly CreateAdminUserUseCase _createAdminUseCase;
        private readonly DeactivateUserUseCase _deactivateUserUseCase;
        private readonly ReactivateUserUseCase _reactivateUserUseCase;

        public AdminController(PromoteUserToAdminUseCase promoteUserUseCase, DemoteAdminToUserUseCase demoteAdminUseCase, CreateAdminUserUseCase createAdminUseCase, DeactivateUserUseCase deactivateUserUseCase, ReactivateUserUseCase reactivateUserUseCase)
        {
            _promoteUserUseCase = promoteUserUseCase;
            _demoteAdminUseCase = demoteAdminUseCase;
            _createAdminUseCase = createAdminUseCase;
            _deactivateUserUseCase = deactivateUserUseCase;
            _reactivateUserUseCase = reactivateUserUseCase;
        }

        [HttpPost("create")]
        [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status201Created)]
        public async Task<ActionResult<ApiResponse<UserResponse>>> CreateAdmin([FromBody] CreateAdminUserRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(ApiResponse<object>.ErrorResult("Dados obrigatórios"));

                var currentAdminId = GetCurrentUserId();
                if (currentAdminId == null)
                    return Unauthorized(ApiResponse<object>.ErrorResult("Token inválido"));

                var dto = new CreateAdminUserDTO
                {
                    Email = request.Email,
                    Password = request.Password,
                    Name = request.Name,
                    CreatedByAdminId = currentAdminId.Value
                };

                var result = await _createAdminUseCase.ExecuteAsync(dto);

                var response = new UserResponse
                {
                    Id = result.Id,
                    Email = result.Email,
                    Name = result.Name,
                    Role = result.Role,
                    CreatedAt = result.CreatedAt,
                    IsActive = result.IsActive
                };

                return StatusCode(201, ApiResponse<UserResponse>.SuccessResult(response, $"Administrador '{result.Name}' criado com sucesso"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<object>.ErrorResult(ex.Message));
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

        [HttpPut("promote")]
        [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<UserResponse>>> PromoteUser([FromBody] PromoteUserRequest request)
        {
            try
            {
                if (request == null || request.UserId == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResult("ID do usuário é obrigatório"));

                var currentAdminId = GetCurrentUserId();
                if (currentAdminId == null)
                    return Unauthorized(ApiResponse<object>.ErrorResult("Token inválido"));

                var dto = new PromoteUserToAdminDTO
                {
                    UserId = request.UserId,
                    AdminId = currentAdminId.Value
                };

                var result = await _promoteUserUseCase.ExecuteAsync(dto);

                var response = new UserResponse
                {
                    Id = result.Id,
                    Email = result.Email,
                    Name = result.Name,
                    Role = result.Role,
                    CreatedAt = result.CreatedAt,
                    IsActive = result.IsActive
                };

                return Ok(ApiResponse<UserResponse>.SuccessResult(response, $"Usuário '{result.Name}' promovido a administrador"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message.Contains("não encontrado")
                    ? NotFound(ApiResponse<object>.ErrorResult(ex.Message))
                    : Conflict(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Erro interno do servidor - {ex.Message}"));
            }
        }

        [HttpPut("demote/{adminId}")]
        [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<UserResponse>>> DemoteAdminToUser(Guid adminId)
        {
            try
            {
                if (adminId == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResult("ID do administrador é obrigatório"));

                var currentAdminId = GetCurrentUserId();
                if (currentAdminId == null)
                    return Unauthorized(ApiResponse<object>.ErrorResult("Token inválido"));

                var dto = new DemoteAdminToUserDTO
                {
                    AdminId = adminId,
                    RequestingAdminId = currentAdminId.Value
                };

                var result = await _demoteAdminUseCase.ExecuteAsync(dto);

                var response = new UserResponse
                {
                    Id = result.Id,
                    Email = result.Email,
                    Name = result.Name,
                    Role = result.Role,
                    CreatedAt = result.CreatedAt,
                    IsActive = result.IsActive
                };

                return Ok(ApiResponse<UserResponse>.SuccessResult(response, $"Administrador '{result.Name}' despromovido"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message.Contains("não encontrado")
                    ? NotFound(ApiResponse<object>.ErrorResult(ex.Message))
                    : Conflict(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Erro interno do servidor - {ex.Message}"));
            }
        }

        [HttpPut("deactivate/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<UserResponse>>> DeactivateUser(Guid userId)
        {
            try
            {
                if (userId == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResult("ID do usuário é obrigatório"));

                var result = await _deactivateUserUseCase.ExecuteAsync(new DeactivateUserDTO { UserId = userId });

                var response = new UserResponse
                {
                    Id = result.Id,
                    Email = result.Email,
                    Name = result.Name,
                    Role = result.Role,
                    CreatedAt = result.CreatedAt,
                    IsActive = result.IsActive
                };

                return Ok(ApiResponse<UserResponse>.SuccessResult(response, $"Usuário '{result.Name}' desativado"));
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message.Contains("não encontrado")
                    ? NotFound(ApiResponse<object>.ErrorResult(ex.Message))
                    : Conflict(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Erro interno do servidor - {ex.Message}"));
            }
        }

        [HttpPut("reactivate/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<UserResponse>>> ReactivateUser(Guid userId)
        {
            try
            {
                if (userId == Guid.Empty)
                    return BadRequest(ApiResponse<object>.ErrorResult("ID do usuário é obrigatório"));

                var result = await _reactivateUserUseCase.ExecuteAsync(new ReactivateUserDTO { UserId = userId });

                var response = new UserResponse
                {
                    Id = result.Id,
                    Email = result.Email,
                    Name = result.Name,
                    Role = result.Role,
                    CreatedAt = result.CreatedAt,
                    IsActive = result.IsActive
                };

                return Ok(ApiResponse<UserResponse>.SuccessResult(response, $"Usuário '{result.Name}' reativado"));
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message.Contains("não encontrado")
                    ? NotFound(ApiResponse<object>.ErrorResult(ex.Message))
                    : Conflict(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Erro interno do servidor - {ex.Message}"));
            }
        }

        [HttpGet("adminLogged")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public ActionResult<ApiResponse<object>> GetCurrentAdmin()
        {
            var currentUser = new
            {
                UserId = GetCurrentUserId(),
                Email = User.FindFirst(ClaimTypes.Email)?.Value,
                Name = User.FindFirst(ClaimTypes.Name)?.Value,
                Role = User.FindFirst(ClaimTypes.Role)?.Value
            };

            return Ok(ApiResponse<object>.SuccessResult(currentUser, "Informações do administrador logado"));
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}