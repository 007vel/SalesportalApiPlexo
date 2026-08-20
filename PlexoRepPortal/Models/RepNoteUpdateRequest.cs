namespace PlexoRepPortal.Models
{
    // RepId/Kind aren't included — PUT api/repnotes/{oId} identifies the row by OId and only the
    // text is mutable; changing which rep/kind a note belongs to would collide with the
    // RepId+Kind unique index, so that's not something this endpoint allows.
    public class RepNoteUpdateRequest
    {
        public string? Text { get; set; }
    }
}
