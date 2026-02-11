using HtmlAgilityPack;
using System.Net;

namespace AITranslatorWebApp.Services
{
    public class EmailParserService
    {
        public void ParseTable(IncomingEmailDto email)
        {
            if (email == null || string.IsNullOrWhiteSpace(email.Html))
                return;

            var doc = new HtmlDocument();
            doc.LoadHtml(email.Html);


            var tables = doc.DocumentNode.SelectNodes("//table");
            if (tables == null) return;

            email.TableRows.Clear();

            foreach (var table in tables)
            {
                var rows = table.SelectNodes(".//tr");
                if (rows == null) continue;

                foreach (var row in rows)
                {

                    var cells = row.SelectNodes("./td");

                    if (cells != null && cells.Count >= 3)
                    {

                        string pName = CleanHtml(cells[0].InnerText);


                        if (string.IsNullOrEmpty(pName) ||
                            pName.ToLower().Contains("trading_partner") ||
                            pName.ToLower() == "parameters") continue;

                        var tableRow = new EmailTableRow
                        {
                            TradingPartnerName = pName,
                            DocumentType = CleanHtml(cells[1].InnerText),
                            ErrorMessage = CleanHtml(cells[2].InnerText)
                        };

                        email.TableRows.Add(tableRow);
                    }
                }
            }
        }


        private string CleanHtml(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            return HtmlEntity.DeEntitize(input)
                             .Replace("\r", "")
                             .Replace("\n", "")
                             .Trim();
        }
    }
}