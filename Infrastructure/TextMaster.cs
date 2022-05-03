using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Infrastructure
{
    public class TextMaster
    {
        public static string RemoveWhitespaces(string fullName, bool throwIfBecomeEmpty = false)
        {
            var result = Regex.Replace(fullName, @"\s+", " ").Trim();

            if (throwIfBecomeEmpty && string.IsNullOrEmpty(result))
            {
                throw new ValidationException("String empty");
            }

            return result;
        }
    }
}