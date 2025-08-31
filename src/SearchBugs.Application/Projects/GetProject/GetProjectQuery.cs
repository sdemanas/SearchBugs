using Shared.Messaging;
using Shared.Results;

namespace SearchBugs.Application.Projects.GetProject;

public sealed record GetProjectQuery(Guid ProjectId) : IQuery<GetProjectResponse>;
