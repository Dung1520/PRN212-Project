using System.Net.Http;
using System.Text;
using System.Text.Json;
using BusinessObjects;

namespace Services
{
    public class LmStudioRecommendationProvider : IRecommendationProvider
    {
        private readonly HttpClient _httpClient;
        private readonly string _modelName;

        public LmStudioRecommendationProvider(string modelName, HttpClient? httpClient = null)
        {
            _modelName = modelName;
            _httpClient = httpClient ?? new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(60)
            };
        }

        public async Task<AiRecommendationResponseDto> RecommendAsync(
            StudentPreferenceDto preference,
            List<AiRecommendationCandidateDto> candidates)
        {
            var prompt = BuildPrompt(preference, candidates);

            var requestBody = new
            {
                model = _modelName,
                messages = new object[]
                {
                    new
                    {
                        role = "system",
                        content = "Bạn là trợ lý tư vấn lớp IELTS. Chỉ được chọn từ danh sách candidate được cung cấp. Không được bịa class mới. BẮT BUỘC chỉ trả về JSON hợp lệ, không thêm giải thích ngoài JSON."
                    },
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },
                temperature = 0.2
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                "http://127.0.0.1:1234/v1/chat/completions",
                content);

            response.EnsureSuccessStatusCode();

            var responseText = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseText);

            var rawContent = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(rawContent))
                throw new Exception("LM Studio trả về nội dung rỗng.");

            rawContent = ExtractJson(rawContent);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonSerializer.Deserialize<AiRecommendationResponseDto>(rawContent, options);

            if (result == null)
                throw new Exception("Không parse được JSON từ LM Studio.");

            result.Items ??= new List<AiRecommendationItemDto>();
            return result;
        }

        private static string BuildPrompt(StudentPreferenceDto pref, List<AiRecommendationCandidateDto> candidates)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Yêu cầu của sinh viên:");
            sb.AppendLine(pref.RawPrompt);
            sb.AppendLine();
            sb.AppendLine("Danh sách candidate hợp lệ:");

            foreach (var c in candidates)
            {
                sb.AppendLine(
                    $"- candidateId={c.CandidateId}; course={c.CourseCode}-{c.CourseName}; class={c.ClassCode}; " +
                    $"fee={c.Fee}; weeks={c.DurationWeeks}; day={c.DayOfWeek}; slot={c.Slot}; " +
                    $"start={c.StartDate:dd/MM/yyyy}; seatsLeft={c.SeatsLeft}");
            }

            sb.AppendLine();
            sb.AppendLine("Trả về JSON đúng format:");
            sb.AppendLine("{");
            sb.AppendLine("  \"summary\": \"...\",");
            sb.AppendLine("  \"items\": [");
            sb.AppendLine("    { \"candidateId\": 1, \"score\": 0.95, \"reason\": \"...\" }");
            sb.AppendLine("  ]");
            sb.AppendLine("}");
            sb.AppendLine("Chỉ chọn tối đa 3 item. Không được tạo candidateId ngoài danh sách.");

            return sb.ToString();
        }

        private static string ExtractJson(string raw)
        {
            var start = raw.IndexOf('{');
            var end = raw.LastIndexOf('}');

            if (start >= 0 && end > start)
                return raw.Substring(start, end - start + 1);

            return raw;
        }
    }
}