using Domain.Language;
using Domain.Language.ValueObjects;
using DomainServices.DB.Repositories;

namespace DB.Repository.Dictionary;

public class LanguagesRepository : BaseRepository<Language>
{
    public LanguagesRepository(ApplicationContext db) : base(db)
    {
    }
}
