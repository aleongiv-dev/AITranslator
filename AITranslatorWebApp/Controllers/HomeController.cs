using AITranslatorWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; 

namespace AITranslatorWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly EmailSenderService _emailSender;
        private readonly AITranslatorService _aiTranslator;
        private readonly ILogger<HomeController> _logger; 
        public HomeController(EmailSenderService emailSender, AITranslatorService aiTranslator, ILogger<HomeController> logger)
        {
            _emailSender = emailSender;
            _aiTranslator = aiTranslator;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendSample(string email, string sampleError, string customError, string inputType)
        {

            string finalError = (inputType == "custom") ? customError : sampleError;

            _logger.LogInformation("Request initiated. Mode: {Mode}, Email: {Email}", inputType, email);

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(finalError))
            {
                TempData["Error"] = "Please provide both an email and an error message.";
                return RedirectToAction("Index");
            }

            try
            {
                var aiExplanation = await _aiTranslator.ExplainErrorAsync(finalError);

                var subject = "AI Error Translation Result";
                var htmlBody = $@"
            <p>Hello,</p>
            <p>Here is the AI error translation you requested:</p>
            <table border='1' cellpadding='5' style='border-collapse:collapse; width:100%; font-size: 0.9rem;'>
                <tr style='background-color:#3498db; color:white;'>
                    <th>Source</th><th>Original Error</th><th>AI Error Translation</th>
                </tr>
                <tr>
                    <td>{(inputType == "custom" ? "Custom Input" : "Sample List")}</td>
                    <td>{finalError}</td>
                    <td style='background-color:#f0f7fb;'><strong>EXPLANATION:</strong> {aiExplanation}</td>
                </tr>
            </table>";

                await _emailSender.SendHtmlEmailAsync(email, subject, htmlBody);
                TempData["Success"] = "AI translation sent to your email!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process request");
                TempData["Error"] = "AI Service is currently busy. Please try again.";
            }

            return RedirectToAction("Index");
        }
    }
}