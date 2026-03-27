using BusinessObjects;
using System.Net.Http;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Text.Json;

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
                Timeout = TimeSpan.FromSeconds(180)
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
                       content = "Bạn là trợ lý tư vấn lớp IELTS. Chỉ được chọn candidate từ danh sách đã cho. Không được tạo candidate mới. Chỉ trả về đúng 1 JSON object hợp lệ. " +
                       "Không markdown. Không backtick. Không ví dụ. Không giải thích. Không thêm bất kỳ text nào ngoài JSON."
                    },
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },
                temperature = 0.2,
                max_tokens = 250
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
                    $"- candidateId={c.CandidateId}; course={c.CourseName}; class={c.ClassCode}; fee={c.Fee}; day={c.DayOfWeek}; slot={c.Slot}; seatsLeft={c.SeatsLeft}");
            }

            sb.AppendLine();
            sb.AppendLine("Hãy chọn tối đa 3 candidate phù hợp nhất.");
            sb.AppendLine("Chỉ được dùng candidateId có trong danh sách.");
            sb.AppendLine("Summary chỉ 1 câu ngắn.");
            sb.AppendLine("Reason của mỗi item chỉ 1 câu ngắn.");
            sb.AppendLine("KHÔNG được trả ví dụ.");
            sb.AppendLine("KHÔNG được dùng markdown.");
            sb.AppendLine("KHÔNG được dùng backtick.");
            sb.AppendLine("KHÔNG được thêm bất kỳ text nào ngoài JSON.");
            sb.AppendLine("Chỉ trả về đúng JSON này:");
            sb.AppendLine("{");
            sb.AppendLine("  \"summary\": \"...\",");
            sb.AppendLine("  \"items\": [");
            sb.AppendLine("    { \"candidateId\": 1, \"score\": 0.95, \"reason\": \"...\" }");
            sb.AppendLine("  ]");
            sb.AppendLine("}");
            sb.AppendLine("Nếu không có lựa chọn phù hợp thì trả:");
            sb.AppendLine("{");
            sb.AppendLine("  \"summary\": \"Không tìm thấy lớp phù hợp.\",");
            sb.AppendLine("  \"items\": []");
            sb.AppendLine("}");

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