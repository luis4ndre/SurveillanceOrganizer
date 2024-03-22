using System.Text.RegularExpressions;

namespace SurveillanceOrganizer.CameraTypes
{
    public class Xiaomi360 : ICamera
    {
        public DateTime? GetDateFromFileName(string? lastFolder)
        {
            var match = GetMatched(lastFolder);
            if (match != null)
                return Convert.ToDateTime($"{match.Substring(0, 4)}/{match.Substring(4, 2)}/{match.Substring(6, 2)} {match.Substring(match.Length - 2)}:00");

            return null;
        }

        private string? GetMatched(string lastFolder)
        {
            // Create a pattern for a word that starts with letter "M"  
            var pattern = @"(\d{10})";
            // Create a Regex  
            var rg = new Regex(pattern);

            // Get all matches  
            var matched = rg.Matches(lastFolder);

            if (matched.Count == 1)
                return matched[0].Groups[1].Value;

            return null;
        }
    }
}
