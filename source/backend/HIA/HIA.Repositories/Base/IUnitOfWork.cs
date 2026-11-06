namespace HIA.Repositories.Base;

public interface IUnitOfWork
{
    Task CommitAsync();
}
