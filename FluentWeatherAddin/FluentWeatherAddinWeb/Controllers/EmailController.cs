using Microsoft.AspNetCore.Mvc;

namespace FluentWeatherAddinWeb.Controllers
{
    public class EmlProcessRequest
    {
        public string EmlContent { get; set; } = string.Empty;
        public EmlMetadata Metadata { get; set; } = new();
    }

    public class EmlMetadata
    {
        public string From { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public DateTime DateTimeCreated { get; set; }
        public int BodyLength { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        [HttpPost("process-eml")]
        public async Task<IActionResult> ProcessEml([FromBody] EmlProcessRequest request)
        {
            try
            {
                // EML-Inhalt in der Console ausgeben
                Console.WriteLine("=== AXA E-MAIL PROZESSOR ===");
                Console.WriteLine($"Zeitstempel: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"Absender: {request.Metadata.From}");
                Console.WriteLine($"Betreff: {request.Metadata.Subject}");
                Console.WriteLine($"Empfangen: {request.Metadata.DateTimeCreated:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"Body-Länge: {request.Metadata.BodyLength} Zeichen");
                Console.WriteLine();
                Console.WriteLine("=== EML-INHALT ===");
                Console.WriteLine(request.EmlContent);
                Console.WriteLine("=== ENDE EML ===");
                Console.WriteLine();

                // Einfache Analyse
                var analysis = AnalyzeEml(request);

                return Ok(new
                {
                    message = "EML erfolgreich verarbeitet",
                    processed = true,
                    analysis = analysis,
                    timestamp = DateTime.Now,
                    emlSize = request.EmlContent.Length
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FEHLER beim Verarbeiten der EML: {ex.Message}");
                return BadRequest(new { error = $"Fehler beim Verarbeiten der EML: {ex.Message}" });
            }
        }

        private object AnalyzeEml(EmlProcessRequest request)
        {
            var senderDomain = request.Metadata.From.Contains('@') 
                ? request.Metadata.From.Split('@').Last() 
                : "Unbekannt";

            var isImportant = request.Metadata.Subject.Contains("URGENT", StringComparison.OrdinalIgnoreCase) ||
                            request.Metadata.Subject.Contains("WICHTIG", StringComparison.OrdinalIgnoreCase) ||
                            request.Metadata.Subject.Contains("EILEN", StringComparison.OrdinalIgnoreCase);

            return new
            {
                SenderDomain = senderDomain,
                IsImportant = isImportant,
                EmlSize = request.EmlContent.Length,
                ProcessedAt = DateTime.Now,
                HasAttachments = request.EmlContent.Contains("Content-Disposition: attachment"),
                MessageId = ExtractMessageId(request.EmlContent)
            };
        }

        private string ExtractMessageId(string emlContent)
        {
            var lines = emlContent.Split('\n');
            var messageIdLine = lines.FirstOrDefault(line => line.StartsWith("Message-ID:", StringComparison.OrdinalIgnoreCase));
            return messageIdLine?.Replace("Message-ID:", "").Trim() ?? "Nicht gefunden";
        }
    }
}