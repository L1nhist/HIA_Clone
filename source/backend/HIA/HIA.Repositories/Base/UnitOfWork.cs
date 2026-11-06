using HIA.Core.Base;

namespace HIA.Repositories.Base;

public class UnitOfWork : IUnitOfWork
{
    private readonly DbFactory _dbFactory;

    public UnitOfWork(DbFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public Task CommitAsync()
    {
        return _dbFactory.DbContext.SaveChangesAsync();
    }
}
