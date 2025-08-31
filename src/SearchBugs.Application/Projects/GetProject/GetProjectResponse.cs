namespace SearchBugs.Application.Projects.GetProject;

public sealed record GetProjectResponse(
    Guid Id,
    string Name,
    string Description,
    DateTime CreatedOnUtc,
    DateTime? UpdatedOnUtc);
