using DEMOShoesSizeTag.Interfaces;
using System.Text.Json;

namespace DEMOShoesSizeTag.Services
{
    public class GeminiVisionService : IGeminiVisionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiVisionService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["Gemini:ApiKey"] ?? throw new ArgumentNullException("Thiếu cấu hình Gemini API Key");
        }

        public async Task<string> ExtractShoeSizeAsync(IFormFile image)
        {
            // Sử dụng model gemini-1.5-flash chuyên dụng cho tác vụ nhanh và đọc ảnh
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

            // 1. Chuyển đổi ảnh sang Base64
            using var memoryStream = new MemoryStream();
            await image.CopyToAsync(memoryStream);
            var base64Image = Convert.ToBase64String(memoryStream.ToArray());
            var mimeType = image.ContentType; // VD: "image/jpeg"

            // 2. Tạo Payload (Bao gồm Prompt Text + Image Data + Cấu hình JSON)
            var requestBody = new
            {
                contents = new[]
                {
                new
                {
                    parts = new object[]
                    {
                        new { text = "@\"Analyze this shoe tag image. Extract the shoe sizes for US, UK, FR, JP, and CHN. \r\nIMPORTANT RULES: \r\n1. If a size is represented as a fraction or merged string (e.g., '6 1/2', '61/2', or '6½'), ALWAYS convert it to a decimal format (e.g., '6.5'). \r\n2. Return the result STRICTLY as a JSON object with keys: US, UK, FR, JP, CHN.\";" },
                        new { inline_data = new { mime_type = mimeType, data = base64Image } }
                    }
                }
            },
                generationConfig = new
                {
                    responseMimeType = "application/json" // Bắt buộc trả về JSON chuẩn
                }
            };

            // 3. Gửi Request
            var response = await _httpClient.PostAsJsonAsync(url, requestBody);

            if (!response.IsSuccessStatusCode)
            {
                var errorDetail = await response.Content.ReadAsStringAsync();
                throw new Exception($"Gemini API Error: {errorDetail}");
            }

            // 4. Bóc tách kết quả JSON
            var responseStream = await response.Content.ReadAsStreamAsync();
            using var jsonDocument = await JsonDocument.ParseAsync(responseStream);

            var root = jsonDocument.RootElement;

            try
            {
                // Trích xuất text từ cấu trúc response của Google
                var textResult = root.GetProperty("candidates")[0]
                                     .GetProperty("content")
                                     .GetProperty("parts")[0]
                                     .GetProperty("text").GetString();

                return textResult ?? "{}";
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi đọc dữ liệu trả về từ Gemini.", ex);
            }
        }
    }
}
