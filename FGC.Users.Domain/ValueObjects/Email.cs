using FGC.Users.Domain.Common.ValueObjects;
using System.Text.RegularExpressions;

namespace FGC.Users.Domain.ValueObjects
{
    public class Email : ValueObject
    {
        public string Value { get; private set; }

        public Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email não pode ser vazio ou nulo");

            if (value.Length > 254)
                throw new ArgumentException("Email muito longo. Máximo de 254 caracteres");

            var normalizedEmail = value.Trim().ToLowerInvariant();

            if (!IsValidFormat(normalizedEmail))
                throw new ArgumentException("Formato de email inválido");

            Value = normalizedEmail;
        }

        private static bool IsValidFormat(string email)
        {
            const string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator string(Email email)
        {
            return email.Value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
