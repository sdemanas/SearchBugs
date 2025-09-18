using Shared.Messaging;

namespace SearchBugs.Application.Projects.GetProject;

public sealed record GetProjectQuery(Guid ProjectId) : IQuery<GetProjectResponse>;
