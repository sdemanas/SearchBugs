using SearchBugs.Domain.Bugs;
using SearchBugs.Persistence.Repositories;

namespace SearchBugs.Persistence.Repositories;

internal sealed class CustomFieldRepository : Repository<CustomField, CustomFieldId>, ICustomFieldRepository
{
    public CustomFieldRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}
