using Microsoft.AspNetCore.Mvc;
using System.Text;
using AITranslatorWebApp.Services;

namespace AITranslatorWebApp.Controllers
{
    [ApiController]
    [Route("api/webhooks/cloudmailin")]
    public class WebhookController : ControllerBase
    {
        private readonly AITranslatorService _translator;
        private readonly EmailParserService _parser;
        private readonly EmailSenderService _emailSender; // Added EmailSenderService
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(
            AITranslatorService translator,
            EmailParserService parser,
            EmailSenderService emailSender, // Inject the service here
            ILogger<WebhookController> logger)
        {
            _translator = translator;
            _parser = parser;
            _emailSender = emailSender;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] IncomingEmailDto email)
        {
            try
            {
                if (email == null)
                {
                    _logger.LogWarning("Incoming email JSON is null.");
                    return BadRequest("No email data received.");
                }

                _logger.LogInformation("Email from: {0}", email.Headers?.From);

                // Parse the table from the incoming email
                _parser.ParseTable(email);

                if (email.TableRows == null || !email.TableRows.Any())
                {
                    _logger.LogWarning("No table rows found.");
                    return Ok("No table found.");
                }

                // Process each row using the AI service
                var tasks = email.TableRows.Select(async row =>
                {
                    row.AiExplanation = await _translator.ExplainErrorAsync(row.ErrorMessage);
                });
                await Task.WhenAll(tasks);

                // Generate the response HTML containing the original data and the AI explanation
                string resultHtml = GenerateHtmlResponse(email.TableRows);

                //SEND OUTGOING EMAIL (GMAIL SMTP)
                if (!string.IsNullOrEmpty(email.Headers?.From))
                {
                    // Construct a subject line and send the email
                    string subject = "AI Analysis: " + (email.Headers.Subject ?? "Error Report");
                    string messageBody = $@"
                        <div style='font-family: sans-serif;'>
                            <p>Hello,</p>
                            <p>Our AI has analyzed the errors in your recent email. Here are the findings:</p>
                            {resultHtml}
                            <p style='margin-top: 20px; color: #777;'><i>This is an automated AI response.</i></p>
                        </div>";

                    await _emailSender.SendHtmlEmailAsync(email.Headers.From, subject, messageBody);
                    _logger.LogInformation("Reply sent successfully to {0}", email.Headers.From);
                }

                // Return the HTML as the response to the webhook
                return Content(resultHtml, "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook processing failed.");
                return StatusCode(500, "Error processing request.");
            }
        }

        private string GenerateHtmlResponse(List<EmailTableRow> rows)
        {
            var sb = new StringBuilder();
            sb.Append("<table border='1' cellpadding='10' style='border-collapse:collapse; font-family:sans-serif; width: 100%;'>");

            // Header with the new 4th column
            sb.Append("<tr style='background-color:#3498db; color:white;'>");
            sb.Append("<th>Partner</th><th>Type</th><th>Original Error</th><th>AI Error Translation</th>");
            sb.Append("</tr>");

            foreach (var row in rows)
            {
                sb.Append("<tr>");
                sb.Append($"<td>{row.TradingPartnerName}</td>");
                sb.Append($"<td>{row.DocumentType}</td>");
                sb.Append($"<td>{row.ErrorMessage}</td>");
                sb.Append($"<td style='background-color:#f0f7fb;'><b>{row.AiExplanation}</b></td>");
                sb.Append("</tr>");
            }
            sb.Append("</table>");
            return sb.ToString();
        }
    }
}