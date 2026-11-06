using System.Threading.Tasks;

namespace HIA.Repositories.Base
{
    public interface IUnitOfWork
    {
        Task CommitAsync();
    }
}
