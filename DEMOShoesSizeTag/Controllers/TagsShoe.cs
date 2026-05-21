using DEMOShoesSizeTag.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.RegularExpressions;
using Tesseract;

namespace DEMOShoesSizeTag.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShoeSizeController : ControllerBase
    {
        private readonly IGeminiVisionService _geminiService;

        public ShoeSizeController(IGeminiVisionService geminiService)
        {
            _geminiService = geminiService;
        }

        [HttpPost("extract")]
        public async Task<IActionResult> ExtractSize(IFormFile image)
        {
            // Validate cơ bản
            if (image == null || image.Length == 0)
                return BadRequest(new { success = false, message = "Vui lòng tải lên ảnh mác giày." });

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(image.ContentType))
                return BadRequest(new { success = false, message = "Chỉ hỗ trợ định dạng JPG, PNG, WEBP." });

            try
            {
                // Gọi AI đọc ảnh
                var jsonString = await _geminiService.ExtractShoeSizeAsync(image);

                // Parse chuỗi JSON do AI trả về thành Object để API ASP.NET Core tự động format đẹp
                var resultObject = JsonSerializer.Deserialize<object>(jsonString);

                return Ok(new
                {
                    success = true,
                    data = resultObject
                });
            }
            catch (Exception ex)
            {
                // Trong thực tế, bạn nên dùng ILogger để ghi log lỗi ở đây
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi xử lý ảnh.", error = ex.Message });
            }
        }
    }
}