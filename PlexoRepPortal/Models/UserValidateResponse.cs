namespace PlexoRepPortal.Models
{
    public class UserValidateResponse
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = null!;
    }
}
