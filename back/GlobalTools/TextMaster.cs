using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Infrastructure
{
    public class TextMaster
    {
        public static string RemoveWhiteSpacesExceptNewLines(string? text, bool throwIfBecomeEmpty = false)
        {
            if (string.IsNullOrEmpty(text) && throwIfBecomeEmpty)
                throw new ArgumentNullException();

            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var result = Regex.Replace(text, @"(\n?)[^\S\n]+(\n?)", m =>
                !string.IsNullOrEmpty(m.Groups[1].Value) || !string.IsNullOrEmpty(m.Groups[2].Value) // If any \n matched
                    ? $"{m.Groups[1].Value}{m.Groups[2].Value}" // Concat Group 1 and 2 values
                    : " ");  // Else, replace the 1+ whitespaces matched with a space

            var finalResult = Regex.Replace(result, @"\n{3,}", "\n\n").Trim(); // Replace 3+ \ns with two \ns

            if (throwIfBecomeEmpty && string.IsNullOrEmpty(result))
            {
                throw new ValidationException("String empty");
            }

            return finalResult;
        }

        public static string RemoveWhiteSpaces(string? text, bool throwIfBecomeEmpty = false)
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