using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using BusinessObjects;

namespace Services
{
    public static class StudentPreferenceParser
    {
        public static StudentPreferenceDto Parse(string? prompt)
        {
            prompt ??= string.Empty;
            var text = prompt.Trim().ToLowerInvariant();

            var result = new StudentPreferenceDto
            {
                RawPrompt = prompt
            };

            // 1. Parse khoảng giá trước: "từ 6 triệu đến 8 triệu"
            var rangeMatch = Regex.Match(
                text,
                @"(?:từ|tu|khoảng|khoang)?\s*(\d+)\s*triệu?\s*(?:đến|den|tới|toi|-)\s*(\d+)\s*triệu");

            if (rangeMatch.Success
                && decimal.TryParse(rangeMatch.Groups[1].Value, out var fee1)
                && decimal.TryParse(rangeMatch.Groups[2].Value, out var fee2))
            {
                result.MinFee = Math.Min(fee1, fee2) * 1_000_000;
                result.MaxFee = Math.Max(fee1, fee2) * 1_000_000;
            }
            else
            {
                // 2. Parse giá tối đa
                var maxFeeMatch = Regex.Match(
                    text,
                    @"(?:dưới|duoi|<=?|không quá|khong qua)\s*(\d+)\s*triệu");

                if (maxFeeMatch.Success
                    && decimal.TryParse(maxFeeMatch.Groups[1].Value, out var maxMillion))
                {
                    result.MaxFee = maxMillion * 1_000_000;
                }

                // 3. Parse giá tối thiểu
                var minFeeMatch = Regex.Match(
                    text,
                    @"(?:trên|tren|hơn|hon|lớn hơn|lon hon|>=?|ít nhất|it nhat)\s*(\d+)\s*triệu");

                if (minFeeMatch.Success
                    && decimal.TryParse(minFeeMatch.Groups[1].Value, out var minMillion))
                {
                    result.MinFee = minMillion * 1_000_000;
                }
            }

            // 4. Parse buổi học
            if (text.Contains("sáng") || text.Contains("sang"))
                result.PreferMorning = true;

            if (text.Contains("chiều") || text.Contains("chieu"))
                result.PreferAfternoon = true;

            if (text.Contains("tối") || text.Contains("toi"))
                result.PreferEvening = true;

            // 5. Parse level
            if (text.Contains("mất gốc") || text.Contains("mat goc")
                || text.Contains("cơ bản") || text.Contains("co ban")
                || text.Contains("beginner"))
            {
                result.LevelHint = "Beginner";
            }
            else if (text.Contains("elementary"))
            {
                result.LevelHint = "Elementary";
            }
            else if (text.Contains("intermediate"))
            {
                result.LevelHint = "Intermediate";
            }
            else if (text.Contains("nâng cao") || text.Contains("nang cao")
                     || text.Contains("advanced"))
            {
                result.LevelHint = "Advanced";
            }

            return result;
        }

        public static bool MatchesPreferredSession(string slotText, StudentPreferenceDto pref)
        {
            if (!pref.PreferMorning && !pref.PreferAfternoon && !pref.PreferEvening)
                return true;

            slotText = slotText.ToLowerInvariant();

            var isMorning = slotText.Contains("slot 1") || slotText.Contains("slot 2");
            var isAfternoon = slotText.Contains("slot 3") || slotText.Contains("slot 4");
            var isEvening = slotText.Contains("slot 5");

            return (pref.PreferMorning && isMorning)
                   || (pref.PreferAfternoon && isAfternoon)
                   || (pref.PreferEvening && isEvening);
        }

        public static bool MatchesLevel(string courseName, StudentPreferenceDto pref)
        {
            if (string.IsNullOrWhiteSpace(pref.LevelHint))
                return true;

            var name = courseName.ToLowerInvariant();

            return pref.LevelHint.ToLowerInvariant() switch
            {
                "beginner" => name.Contains("beginner") || name.Contains("elementary"),
                "elementary" => name.Contains("elementary"),
                "intermediate" => name.Contains("intermediate"),
                "advanced" => name.Contains("advanced") || name.Contains("upper-intermediate"),
                _ => true
            };
        }
    }
}