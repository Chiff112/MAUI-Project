using System.Security.Cryptography;
using System.Text;

namespace PasswordGenerator.Services;

// Настройки генерации пароля
public class PasswordOptions
{
    public int Length { get; set; } = 16;
    public bool IncludeUppercase { get; set; } = true;
    public bool IncludeLowercase { get; set; } = true;
    public bool IncludeDigits { get; set; } = true;
    public bool IncludeSymbols { get; set; } = true;
    public bool ExcludeAmbiguous { get; set; } = false;
}

// Уровни надёжности пароля
public enum PasswordStrength
{
    VeryWeak,
    Weak,
    Medium,
    Strong,
    VeryStrong
}

// Генератор паролей
public static class PasswordService
{
    private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
    private const string Digits = "0123456789";
    private const string Symbols = "!@#$%^&*()-_=+[]{}<>?";
    private const string Ambiguous = "Il1O0o";

    // Собирает пароль из выбранных типов символов
    public static string Generate(PasswordOptions options)
    {
        // Собираем общий набор символов
        var pool = new StringBuilder();
        if (options.IncludeUppercase) pool.Append(Uppercase);
        if (options.IncludeLowercase) pool.Append(Lowercase);
        if (options.IncludeDigits) pool.Append(Digits);
        if (options.IncludeSymbols) pool.Append(Symbols);

        string chars = pool.ToString();

        // Убираем похожие символы, если включена опция
        if (options.ExcludeAmbiguous)
        {
            var filtered = new StringBuilder();
            foreach (char c in chars)
                if (!Ambiguous.Contains(c)) filtered.Append(c);
            chars = filtered.ToString();
        }

        if (chars.Length == 0 || options.Length < 1)
            return string.Empty;

        // Случайно выбираем символы из набора
        var result = new char[options.Length];
        for (int i = 0; i < options.Length; i++)
            result[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];

        return new string(result);
    }

    // Считает надёжность по длине и набору символов
    public static PasswordStrength CalculateStrength(string password)
    {
        if (string.IsNullOrEmpty(password)) return PasswordStrength.VeryWeak;

        int score = 0;
        if (password.Length >= 8) score++;
        if (password.Length >= 12) score++;
        if (password.Length >= 16) score++;

        bool hasUpper = false, hasLower = false, hasDigit = false, hasSymbol = false;
        foreach (char c in password)
        {
            if (char.IsUpper(c)) hasUpper = true;
            else if (char.IsLower(c)) hasLower = true;
            else if (char.IsDigit(c)) hasDigit = true;
            else hasSymbol = true;
        }

        if (hasUpper && hasLower) score++;
        if (hasDigit) score++;
        if (hasSymbol) score++;

        return score switch
        {
            <= 1 => PasswordStrength.VeryWeak,
            2 => PasswordStrength.Weak,
            3 or 4 => PasswordStrength.Medium,
            5 => PasswordStrength.Strong,
            _ => PasswordStrength.VeryStrong
        };
    }
}
