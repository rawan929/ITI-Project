using System;
using System.Collections.Generic;
using System.Linq;

namespace ITI.BLL.Services
{
    /// <summary>
    /// One source of truth for who can donate to whom.
    ///
    /// IMPORTANT: these are the standard whole-blood rules, but they are encoded here
    /// for the project only. Before this system is used for anything real, the table
    /// must be confirmed against your blood bank's own medical policy rather than
    /// trusted because it lives in the code.
    /// </summary>
    public static class BloodCompatibility
    {
        /// <summary>Donor blood type -> the recipient types that donor can give to.</summary>
        private static readonly Dictionary<string, HashSet<string>> DonorToRecipients = new()
        {
            ["O-"] = new HashSet<string> { "O-", "O+", "A-", "A+", "B-", "B+", "AB-", "AB+" },
            ["O+"] = new HashSet<string> { "O+", "A+", "B+", "AB+" },
            ["A-"] = new HashSet<string> { "A-", "A+", "AB-", "AB+" },
            ["A+"] = new HashSet<string> { "A+", "AB+" },
            ["B-"] = new HashSet<string> { "B-", "B+", "AB-", "AB+" },
            ["B+"] = new HashSet<string> { "B+", "AB+" },
            ["AB-"] = new HashSet<string> { "AB-", "AB+" },
            ["AB+"] = new HashSet<string> { "AB+" },
        };

        public static bool IsCompatible(string donorBloodType, string requestedBloodType)
        {
            return !string.IsNullOrWhiteSpace(donorBloodType)
                   && DonorToRecipients.TryGetValue(donorBloodType, out var recipients)
                   && recipients.Contains(requestedBloodType);
        }

        /// <summary>
        /// The donor blood types that can supply the requested type — this is the
        /// filter the matching service starts from.
        /// </summary>
        public static List<string> GetCompatibleDonorTypes(string requestedBloodType)
        {
            if (string.IsNullOrWhiteSpace(requestedBloodType))
            {
                return new List<string>();
            }

            return DonorToRecipients
                .Where(pair => pair.Value.Contains(requestedBloodType))
                .Select(pair => pair.Key)
                .ToList();
        }
    }
}
