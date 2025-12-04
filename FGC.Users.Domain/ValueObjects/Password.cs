using FGC.Users.Domain.Common.ValueObjects;

namespace FGC.Users.Domain.ValueObjects
{
    public class Password : ValueObject
    {
        public string Value { get; private set; }

        public Password(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Senha não pode ser vazia");

            ValidatePasswordStrength(password);
            Value = password;
        }

        private static void ValidatePasswordStrength(string password)
        {
            if (password.Length < 8)
                throw new ArgumentException("Senha deve ter pelo menos 8 caracteres");

            if (!password.Any(char.IsDigit))
                throw new ArgumentException("Senha deve conter pelo menos um número");

            if (!password.Any(char.IsUpper))
                throw new ArgumentException("Senha deve conter pelo menos uma letra maiúscula");

            if (!password.Any(char.IsLower))
                throw new ArgumentException("Senha deve conter pelo menos uma letra minúscula");

            if (!password.Any(ch => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(ch)))
                throw new ArgumentException("Senha deve conter pelo menos um caractere especial");

            if (IsCommonPassword(password))
                throw new ArgumentException("Senha muito comum. Escolha uma senha mais segura");
        }

        private static bool IsCommonPassword(string password)
        {
            var commonPasswords = new[]
            {
                "123456", "password", "123456789", "12345678",
                "12345", "1234567", "admin", "123123",
                "qwerty", "abc123", "senha123", "fiap123",
                "password123", "admin123", "123qwe"
            };

            return commonPasswords.Contains(password.ToLower());
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator string(Password password)
        {
            return password.Value;
        }

        public override string ToString()
        {
            return "****** (senha protegida)";
        }
    }
}
