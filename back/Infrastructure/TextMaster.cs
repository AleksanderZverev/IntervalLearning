using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Infrastructure
{
    public class TextMaster
    {
        public static string RemoveWhitespaces(string? text, bool throwIfBecomeEmpty = false)
        {
            if (string.IsNullOrEmpty(text) && throwIfBecomeEmpty)
                throw new ArgumentNullException();

            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var result = Regex.Replace(text, @"\s+", " ").Trim();

            if (throwIfBecomeEmpty && string.IsNullOrEmpty(result))
            {
                throw new ValidationException("String empty");
            }

            return result;
        }
    }
}