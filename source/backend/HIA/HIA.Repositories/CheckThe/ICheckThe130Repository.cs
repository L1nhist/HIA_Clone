using HIA.Data.Records;
using HIA.Repositories.Base;

namespace HIA.Repositories.CheckThe;

public interface ICheckThe130Repository : IBaseRepository<CheckThe4210>
{
    Task<IEnumerable<string?>> GetLastMaCSKCB(int max = 5);
}
