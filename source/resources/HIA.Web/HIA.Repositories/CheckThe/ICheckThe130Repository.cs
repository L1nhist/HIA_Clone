using HIA.Core.Models.Pagination;
using HIA.DatabaseModel.Entites;
using HIA.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HIA.Repositories.CheckThe
{
    public interface ICheckThe130Repository : IBaseRepository<CheckThe4210>
    {
        Task<IEnumerable<string>> GetLastMaCSKCB(int max = 5);
    }
}
