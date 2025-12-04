namespace FGC.Users.Presentation.Models.Responses
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public DateTime Timestamp { get; set; }

        public ApiResponse()
        {
            Message = string.Empty;
            Timestamp = DateTime.UtcNow;
        }

        public static ApiResponse<T> SuccessResult(T data, string message = "Operação realizada com sucesso")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse<T> ErrorResult(string message, T data = default)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    public class UserResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class AuthResponse
    {
        public UserResponse User { get; set; } = new();
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime LastLoginAt { get; set; }
        public string TokenType { get; set; } = "Bearer";
    }
}
