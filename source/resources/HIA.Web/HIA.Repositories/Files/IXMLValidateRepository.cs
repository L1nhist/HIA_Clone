using HIA.DatabaseModel.Entites;
using HIA.Repositories.Base;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HIA.Repositories.Files
{
    public interface IXMLValidateRepository : IBaseRepository<XMLValidate>
    {
        Task<IEnumerable<string>> GetLatestMaCSKCB(int max = 5);

        Task<IEnumerable<string>> GetLatestMaCSKCBDesc(int max = 30);

        Task<IEnumerable<long>> GetListIDToValidate(string maCSKCB, int max = 300);

        Task<IEnumerable<string>> GetTopCSKCB(int max = 5, bool ascending = true);

        Task<IEnumerable<string>> GetLessCSKCB(string skips);
    }
}