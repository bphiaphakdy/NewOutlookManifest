
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace FluentWeatherAddinWeb.Controllers
{
    public class EmailData
    {
        public string SenderEmail { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public DateTime ReceivedTime { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class WeatherController : ControllerBase
    {
        private static readonly HttpClient _http = new HttpClient();

        // Simple proxy to Open-Meteo for Zürich (7-day daily min/max)
        // Note: For production, add proper error handling, caching, and input validation.
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var url = "https://api.open-meteo.com/v1/forecast?latitude=47.37&longitude=8.55&daily=temperature_2m_max,temperature_2m_min&timezone=Europe%2FZurich";
            try
            {
                var resp = await _http.GetAsync(url);
                var content = await resp.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            catch
            {
                return StatusCode(502, new { error = "Upstream weather service unavailable." });
            }
        }

        // Endpoint zum Verarbeiten von E-Mail-Daten
        [HttpPost("process-email")]
        public async Task<IActionResult> ProcessEmail([FromBody] EmailData emailData)
        {
            try
            {
                // Hier können Sie die E-Mail-Daten verarbeiten
                Console.WriteLine($"Neue E-Mail erhalten:");
                Console.WriteLine($"Absender: {emailData.SenderEmail}");
                Console.WriteLine($"Betreff: {emailData.Subject}");
                Console.WriteLine($"Body (ersten 100 Zeichen): {emailData.Body.Substring(0, Math.Min(100, emailData.Body.Length))}...");
                Console.WriteLine($"Empfangen: {emailData.ReceivedTime}");
                
                // Beispiel: E-Mail-Analyse oder Weiterverarbeitung
                var analysis = AnalyzeEmail(emailData);
                
                return Ok(new { 
                    message = "E-Mail erfolgreich verarbeitet",
                    sender = emailData.SenderEmail,
                    analysis = analysis,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Fehler beim Verarbeiten der E-Mail: {ex.Message}" });
            }
        }

        private object AnalyzeEmail(EmailData emailData)
        {
            return new
            {
                BodyLength = emailData.Body.Length,
                WordCount = emailData.Body.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
                HasAttachments = false, // Könnte erweitert werden
                SenderDomain = emailData.SenderEmail.Split('@').LastOrDefault(),
                IsImportant = emailData.Subject.Contains("URGENT", StringComparison.OrdinalIgnoreCase) ||
                            emailData.Subject.Contains("WICHTIG", StringComparison.OrdinalIgnoreCase)
            };
        }
    }
}
