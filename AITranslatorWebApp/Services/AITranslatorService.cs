using System.Net.Http.Json;
using System.Text.Json;

public class AITranslatorService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AITranslatorService> _logger;

    private readonly string _gradioEndpoint =
        "https://aleongiv-error-translator-space.hf.space/gradio_api/predict";

    public AITranslatorService(HttpClient httpClient, ILogger<AITranslatorService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> ExplainErrorAsync(string errorText)
    {
        if (string.IsNullOrWhiteSpace(errorText))
            return "No error text provided.";

        try
        {
           
            string callEndpoint = "https://aleongiv-error-translator-space.hf.space/gradio_api/call/explain_error";
            var payload = new { data = new[] { errorText } };

            var response = await _httpClient.PostAsJsonAsync(callEndpoint, payload);
            response.EnsureSuccessStatusCode();

            using var callDoc = await response.Content.ReadFromJsonAsync<JsonDocument>();
            string eventId = callDoc.RootElement.GetProperty("event_id").GetString();

        
            string statusEndpoint = $"https://aleongiv-error-translator-space.hf.space/gradio_api/call/explain_error/{eventId}";

            // connection open
            using var request = new HttpRequestMessage(HttpMethod.Get, statusEndpoint);
            using var statusResponse = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            using var stream = await statusResponse.Content.ReadAsStreamAsync();
            using var reader = new System.IO.StreamReader(stream);

            // Read the stream line by line until 'complete'
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (line.StartsWith("data:"))
                {
                    var json = line.Substring(5).Trim();
                    try
                    {
                        using var doc = JsonDocument.Parse(json);
                        // Gradio "call" API sends the result in an array under the 'data' or root property
                        if (doc.RootElement.ValueKind == JsonValueKind.Array)
                        {
                            var result = doc.RootElement[0].GetString();
                            if (!string.IsNullOrEmpty(result))
                            {
                                _logger.LogInformation("AI Result received, closing stream.");
                                return result; 
                            }
                        }
                    }
                    catch {  }
                }
            }

            return "AI Error: Stream closed without data.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Detailed AI Service Failure");
            return "AI Translation Error.";
        }
    }
    private string ExtractGradioData(string sseResponse)
    {

        var lines = sseResponse.Split('\n');
        foreach (var line in lines)
        {
            if (line.StartsWith("data:"))
            {
                var json = line.Substring(5).Trim();
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
                {
                    return doc.RootElement[0].GetString() ?? "No explanation found.";
                }
            }
        }
        return "Explanation pending or not found.";
    }
}
