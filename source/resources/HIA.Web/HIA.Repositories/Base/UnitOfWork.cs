using HIA.Core.Base;
using System.Threading.Tasks;

namespace HIA.Repositories.Base
{
    public class UnitOfWork : IUnitOfWork
    {
        private DbFactory _dbFactory;
        public UnitOfWork(DbFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }
        public Task CommitAsync()
        {
            //Linhist changed 14/04/26
            //return _dbFactory.DbContext.BulkSaveChangesAsync();
            return _dbFactory.DbContext.SaveChangesAsync();
        }
    }
}
