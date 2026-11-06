using HIA.DatabaseModel.Entites;
using Microsoft.EntityFrameworkCore;
using System;

namespace HIA.Core.Base
{
    public class DbFactory : IDisposable
    {
        private bool _disposed;
        private Func<HIADbContext> _instanceFunc;
        private DbContext _dbContext;
        public DbContext DbContext => _dbContext ?? (_dbContext = _instanceFunc.Invoke());
        public DbFactory(Func<HIADbContext> dbContextFactory)
        {
            _instanceFunc = dbContextFactory;
        }

        public void Dispose()
        {
            if (!_disposed && _dbContext != null)
            {
                _disposed = true;
                _dbContext.Dispose();
            }
        }
    }
}
