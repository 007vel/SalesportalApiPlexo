namespace PlexoCommon.Email
{
    public interface IEmailService
    {
        Task SendAsync(string to, string? userContactemail, string subject, string body, CancellationToken cancellationToken = default);
    }
}
