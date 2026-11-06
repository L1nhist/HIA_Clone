//using HIA.Core.Models.Pagination;
//using HIA.DatabaseModel.Entites;
//using HIA.Repositories.Base;
//using System;
//using System.Collections.Generic;
//using System.Linq.Expressions;
//using System.Threading.Tasks;

//namespace HIA.Repositories.Files
//{
//    public interface IXml1Repository_Temp : IBaseRepository<XML1_Temp>
//    {
//        Task<IEnumerable<string>> GetAllListMaCSKCB();
//        Task<IEnumerable<XML1_Temp>> GetAllListXmlReference(string currentHospitalId);

//        Task<IEnumerable<XML1_Temp>> GetAllListXmlReferenceLe(int idsLe, string maCSKCB);

//        Task<IEnumerable<XML1_Temp>> GetAllListXmlReferenceLeString(List<string> idsLe);

//        Task<IEnumerable<XML1_Temp>> GetAllListXmlReferenceKhoi(string currentHospitalId, Expression<Func<XML1_Temp, bool>> filter = null);

//        //Linhist added 15/11/22
//        Task<IEnumerable<XML1_Temp>> GetAllListXmlReference_WithXml4Only(string currentHospitalId, Expression<Func<XML1_Temp, bool>> filter = null);

//        //Linhist added 09/06/22
//        Task<IEnumerable<XML1_Temp>> GetListXmlReport3(string currentHospitalId, Expression<Func<XML1_Temp, bool>> filter = null, bool includedErrors = false);

//        Task<IEnumerable<XML1_Temp>> GetAllListXmlReferenceSameDate(string currentHospitalId,DateTime minDate,DateTime MaxDate);

//        Task<IEnumerable<XML1_Temp>> GetAllListXmlReferenceHaveXML23();

//        Task<IEnumerable<XML1_Temp>> GetAllListXmlReferenceLeStringFile(List<string> idsLe);

//        Task<IEnumerable<XML1_Temp>> GetXML1HaveXML4ErrorsOnly(List<int?> xml1Id);

//        //Linhist added 09/10/22
//        Task<IEnumerable<XML1_Temp>> GetXML1AndErrorsOnly(string currentHospitalId, Expression<Func<XML1_Temp, bool>> filter = null);
//    }
//}
