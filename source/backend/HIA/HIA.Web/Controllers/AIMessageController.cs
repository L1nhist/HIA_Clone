using HIA.Core.Models.Token;
using HIA.Data.Reports;
using HIA.Repositories.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HIA.Web.Controllers
{
    public class AIMessageController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configs;
        private readonly IBaseRepository<UserLogs> _userLogsRepository;
        private string maCSKCB = "";
        private string userID = "";
        private string useremail = "";

        public static string SanitizeInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // 1. Cắt khoảng trắng đầu/cuối và chuẩn hóa
            input = input.Trim();

            // 2. Giới hạn độ dài (ví dụ: 500 ký tự)
            if (input.Length > 500)
                input = input.Substring(0, 500);

            // 3. Loại bỏ mã HTML hoặc script
            input = Regex.Replace(input, "<.*?>", string.Empty); // Xóa thẻ HTML
            input = Regex.Replace(input, @"<script[^>]*>[\s\S]*?</script>", string.Empty, RegexOptions.IgnoreCase);

            // 4. Loại bỏ ký tự điều khiển, emoji lỗi, ký tự Unicode nguy hiểm
            input = Regex.Replace(input, @"[\u0000-\u001F\u007F-\u009F]", string.Empty); // control chars
            input = Regex.Replace(input, @"[\uD800-\uDFFF]", string.Empty); // surrogate pairs

            // 5. Chặn từ khóa nguy hiểm (ví dụ: prompt injection)
            string[] blockedKeywords = { "ignore", "bypass", "override", "system prompt", "jailbreak", "rm -rf", "curl", "shutdown" };
            foreach (var keyword in blockedKeywords)
            {
                input = Regex.Replace(input, keyword, "[blocked]", RegexOptions.IgnoreCase);
            }

            return input;
        }

        public AIMessageController(IHttpContextAccessor httpContextAccessor, IConfiguration configs, IBaseRepository<UserLogs> userLogsRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _configs = configs;
            _userLogsRepository = userLogsRepository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return PartialView();
        }

        public class MessageDto
        {
            public string message { get; set; } = string.Empty;
        }

        [HttpPost]
        [Route("send")]
        public async Task<IActionResult> Send([FromBody] MessageDto dto)
        {
            maCSKCB = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimKey.MA_CSKCB)?.Value ?? "";
            userID = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Sid)?.Value ?? "";
            useremail = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            // Nếu được kiểm tra thông tin đăng nhập ở đây!!!!!
            if (maCSKCB == "" || userID == "" || useremail == "")
            {
                return BadRequest(new { error = "invalid_request", detail = "User not authenticated" });
            }

            try
            {
                if (dto.message.ToLower().Contains("object"))
                {
                    return BadRequest(new { error = "invalid_request", detail = "Nội dung gửi lên không hợp lệ, máy đã nhiễm virus, đề nghị cài đặt lại để sử dụng chương trình" });
                }
                var isVirus = await _userLogsRepository.AnyAsync(x => x.Email == useremail && x.Action == "Sử dụng API ChatGPT" && (x.UrlAccess ?? "").ToLower().Contains("object"));
                if (isVirus)
                {
                    return BadRequest(new { error = "invalid_request", detail = "User này không được sử dụng chức năng này, máy của bạn đã nhiễm virus, đề nghị cài lại" });
                }

                var isRuntomuch = await _userLogsRepository.AnyAsync(x => x.UserID == userID && x.Action == "Sử dụng API ChatGPT" && x.LogTime >= DateTime.Now.AddSeconds(-5));
                if (isRuntomuch)
                {
                    return BadRequest(new { error = "invalid_request", detail = "Bạn gửi quá nhiều yêu cầu, vui lòng thử lại sau 5s" });
                }
                var sumisRuntomuch = _userLogsRepository.GetCount(x => x.Action == "Sử dụng API ChatGPT" && x.LogTime >= DateTime.Now.AddDays(-1)).Select(x => x.UserID).ToList();
                if (sumisRuntomuch.Count() >= 2000)
                {
                    return BadRequest(new { error = "invalid_request", detail = "Bạn gửi quá nhiều yêu cầu, vui lòng thử lại sau một khoảng thời gian" });
                }
            }
            catch
            {
                return BadRequest(new { error = "invalid_request", detail = "Lỗi hệ thống, vui lòng thử lại sau" });
            }

            //lưu log
            string clearedMessage = SanitizeInput(dto.message.ToString());
            await _userLogsRepository.InsertAsync(new UserLogs()
            {
                UserID = userID,
                Email = useremail,
                Action = "Sử dụng API ChatGPT",
                MA_CSKCB = maCSKCB,
                UrlAccess = clearedMessage,
                LogTime = DateTime.Now,
            });
            await _userLogsRepository.SaveChangesAsync();

            // dto đã được đọc ở khối try phía trên (dto.message), nên tới đây dto luôn khác null.
            if (dto != null)
            {
                // Read endpoint from configuration (e.g. "http://172.16.101.188:5136/api/gpt")
                var endpoint = _configs.GetSection("AppSettings:AiPath").Value;

                string? gptapikey = _configs.GetSection("AppSettings:AiKey").Value;

                using var http = new HttpClient();
                var payload = new
                {
                    message = dto.message,
                    gptApiKey = gptapikey
                };
                // Giữ System.Text.Json mặc định (không dùng JsonDefaults) để tên thuộc tính gửi đi không đổi.
                var json = JsonSerializer.Serialize(payload);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                var resp = await http.PostAsync(endpoint, content);
                var respBody = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    return StatusCode((int)resp.StatusCode, new { error = "remote_gpt_error", detail = respBody });
                }

                // Try to parse expected shape { response, raw, duration }
                try
                {
                    using var doc = JsonDocument.Parse(respBody);
                    var root = doc.RootElement;
                    var responseText = root.TryGetProperty("response", out var r) ? r.GetString() : null;
                    var raw = root.TryGetProperty("raw", out var rr) ? rr.GetRawText() : null;
                    var duration = root.TryGetProperty("duration", out var d) ? (long?)d.GetInt64() : null;
                    return Ok(new { response = responseText, raw = raw, duration = duration });
                }
                catch (JsonException)
                {
                    // If parsing fails, return raw body
                    return Ok(new { response = (string?)null, raw = respBody, duration = (long?)null });
                }
            }
            else
            {
                return BadRequest(new { error = "invalid_request", detail = "Message cannot be empty" });
            }
        }
    }
}
