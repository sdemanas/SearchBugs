using SearchBugs.Domain.Bugs;

namespace SearchBugs.Persistence.Repositories;

internal sealed class CustomFieldRepository : Repository<CustomField, CustomFieldId>, ICustomFieldRepository
{
    public CustomFieldRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}
