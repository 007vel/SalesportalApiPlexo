using System.Net;
using System.Reflection;

namespace PlexoCommon.Email.Templates
{
    public static class RepWelcomeEmailTemplate
    {
        public static (string Subject, string Html) Build(string fullName, string email, string repId, string? loginLink)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames()
                .First(n => n.EndsWith("RepWelcomeEmail.html", StringComparison.Ordinal));

            using var stream = assembly.GetManifestResourceStream(resourceName)!;
            using var reader = new StreamReader(stream);
            var html = reader.ReadToEnd()
                .Replace("{{FullName}}", WebUtility.HtmlEncode(fullName))
                .Replace("{{Email}}", WebUtility.HtmlEncode(email))
                .Replace("{{RepId}}", WebUtility.HtmlEncode(repId))
                .Replace("{{LoginLink}}", loginLink ?? "#");

            return ("Welcome to the Plexo Sales Portal", html);
        }
    }
}
