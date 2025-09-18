using SearchBugs.Domain.Bugs;

namespace SearchBugs.Application.BugTracking.Attachments;

public record AttachmentDto(
    Guid Id,
    string FileName,
    string ContentType,
    byte[] Content,
    Guid BugId,
    DateTime CreatedOnUtc,
    DateTime? ModifiedOnUtc)
{
    public static AttachmentDto FromAttachment(Attachment attachment) => new(
        attachment.Id.Value,
        attachment.FileName,
        attachment.ContentType,
        attachment.Content,
        attachment.BugId.Value,
        attachment.CreatedOnUtc,
        attachment.ModifiedOnUtc);
}
