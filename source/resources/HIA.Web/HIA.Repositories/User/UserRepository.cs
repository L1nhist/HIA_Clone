using HIA.Core.Base;
using HIA.Repositories.Base;
using Microsoft.AspNetCore.Http;

namespace HIA.Repositories.User
{
    public class UserRepository : BaseRepository<DatabaseModel.Entites.User>, IUserRepository
    {
        public UserRepository(DbFactory dbFactory, IHttpContextAccessor httpContext) : base(dbFactory, httpContext)
        {
        }
    }
}
