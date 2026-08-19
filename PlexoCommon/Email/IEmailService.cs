namespace PlexoCommon.Email
{
    public interface IEmailService
    {
        Task SendAsync(string to, string? from, string subject, string body, CancellationToken cancellationToken = default);
    }
}
