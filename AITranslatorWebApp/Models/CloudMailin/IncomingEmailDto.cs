using System.Text.Json.Serialization;
using System.Collections.Generic;

public class IncomingEmailDto
{
    [JsonPropertyName("headers")]
    public EmailHeaders Headers { get; set; }

    [JsonPropertyName("plain")]
    public string Plain { get; set; }

    [JsonPropertyName("html")]
    public string Html { get; set; }


    public List<EmailTableRow> TableRows { get; set; } = new();
}

public class EmailHeaders
{
    [JsonPropertyName("from")]
    public string From { get; set; }

    [JsonPropertyName("to")]
    public string To { get; set; }

    [JsonPropertyName("subject")]
    public string Subject { get; set; }
}

public class EmailTableRow
{
    public string TradingPartnerName { get; set; }
    public string DocumentType { get; set; }
    public string ErrorMessage { get; set; }
    public string AiExplanation { get; set; }
}
