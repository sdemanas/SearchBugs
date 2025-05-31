using SearchBugs.Domain.Bugs;

namespace SearchBugs.Application.BugTracking.CustomFields;

public record CustomFieldDto(
    Guid Id,
    string Name,
    string FieldType,
    string Value,
    Guid ProjectId)
{
    public static CustomFieldDto FromCustomField(CustomField customField, string value) => new(
        customField.Id.Value,
        customField.Name,
        customField.FieldType,
        value,
        customField.ProjectId.Value);
} 