using System.Net;
using System.Reflection;
using System.Text;

namespace PlexoCommon.Email.Templates
{
    public static class AdminNotificationEmailTemplate
    {
        // fields: ordered (Label, Value) pairs rendered as the details-card table rows,
        // e.g. [("Rep ID", "1000"), ("Email", "rep@example.com")].
        // message: free text from a <textarea>; HTML-encoded then normalized to <br> line breaks.
        public static string Build(string title, IEnumerable<(string Label, string Value)> fields, string message)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames()
                .First(n => n.EndsWith("AdminNotificationEmail.html", StringComparison.Ordinal));

            using var stream = assembly.GetManifestResourceStream(resourceName)!;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd()
                .Replace("{{Title}}", WebUtility.HtmlEncode(title))
                .Replace("{{FieldsRows}}", BuildFieldsRows(fields))
                .Replace("{{Message}}", FormatMessage(message));
        }

        private static string BuildFieldsRows(IEnumerable<(string Label, string Value)> fields)
        {
            var list = fields.ToList();
            var sb = new StringBuilder();
            for (var i = 0; i < list.Count; i++)
            {
                var (label, value) = list[i];
                sb.Append("<tr>")
                  .Append("<td style=\"color:#7A8591;font-size:13px;padding-bottom:10px;width:100px;\">")
                  .Append(WebUtility.HtmlEncode(label)).Append("</td>")
                  .Append("<td style=\"color:#263544;font-size:14px;font-weight:600;padding-bottom:10px;\">")
                  .Append(WebUtility.HtmlEncode(value)).Append("</td>")
                  .Append("</tr>");

                if (i < list.Count - 1)
                {
                    sb.Append("<tr><td colspan=\"2\" style=\"height:1px;background-color:#E3E7EC;line-height:1px;font-size:1px;\">&nbsp;</td></tr>");
                }
            }
            return sb.ToString();
        }

        private static string FormatMessage(string message) =>
            WebUtility.HtmlEncode(message).Replace("\r\n", "\n").Replace("\n", "<br>");
    }
}
