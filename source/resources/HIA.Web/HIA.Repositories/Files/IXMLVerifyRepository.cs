//using HIA.DatabaseModel.Entites;
//using HIA.Repositories.Base;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace HIA.Repositories.Files
//{
//    public interface IXMLVerifyRepository : IBaseRepository<XMLVerify>
//    {
//        Task<IEnumerable<long>> GetListIDToVerify(string maCSKCB, int max = 300);
//        Task<IEnumerable<XMLVerify>> GetListToVerify(string maCSKCB, int max = 300);
//        Task<IEnumerable<string>> GetLastMaCSKCBtoVerify(int max = 10);
//    }
//}