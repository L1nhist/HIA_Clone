//using HIA.Core.Models.Pagination;
//using HIA.DatabaseModel.Entites;
//using HIA.Repositories.Base;
//using System;
//using System.Collections.Generic;
//using System.Linq.Expressions;
//using System.Threading.Tasks;

//namespace HIA.Repositories.Files
//{
//    public interface IXml1Repository : IBaseRepository<XML1>
//    {
//        Task<IEnumerable<string>> GetAllListMaCSKCB();
//        Task<IEnumerable<XML1>> GetAllListXmlReference(string currentHospitalId);

//        Task<IEnumerable<XML1>> GetAllListXmlReferenceLe(int idsLe, string maCSKCB);

//        Task<IEnumerable<XML1>> GetAllListXmlReferenceLeString(List<string> idsLe);

//        Task<IEnumerable<XML1>> GetAllListXmlReferenceKhoi(string currentHospitalId, Expression<Func<XML1, bool>> filter = null);

//        //Linhist added 15/11/22
//        Task<IEnumerable<XML1>> GetAllListXmlReference_WithXml4Only(string currentHospitalId, Expression<Func<XML1, bool>> filter = null);

//        //Linhist added 09/06/22
//        Task<IEnumerable<XML1>> GetListXmlReport3(string currentHospitalId, Expression<Func<XML1, bool>> filter = null, bool includedErrors = false);

//        Task<IEnumerable<XML1>> GetAllListXmlReferenceSameDate(string currentHospitalId,DateTime minDate,DateTime MaxDate);

//        Task<IEnumerable<XML1>> GetAllListXmlReferenceHaveXML23();

//        Task<IEnumerable<XML1>> GetAllListXmlReferenceLeStringFile(List<string> idsLe);

//        Task<IEnumerable<XML1>> GetXML1HaveXML4ErrorsOnly(List<int?> xml1Id);

//        //Linhist added 09/10/22
//        Task<IEnumerable<XML1>> GetXML1AndErrorsOnly(string currentHospitalId, Expression<Func<XML1, bool>> filter = null);

//        //Linhist added 24/02/23
//        Task<IEnumerable<XML1>> GetXml_1_2(string currentHospitalId, Expression<Func<XML1, bool>> filter = null);
//        Task<IEnumerable<XML1>> GetXml_1_3(string currentHospitalId, Expression<Func<XML1, bool>> filter = null);
//        Task<IEnumerable<XML1>> GetXml_1_4(string currentHospitalId, Expression<Func<XML1, bool>> filter = null);
//        Task<IEnumerable<XML1>> GetXml_1_5(string currentHospitalId, Expression<Func<XML1, bool>> filter = null);
//        Task<IEnumerable<XML1>> GetXml_1_E(string currentHospitalId, Expression<Func<XML1, bool>> filter = null);

//        //Linhist added 11/05/23
//        Task<List<string>> GetFileNames(Expression<Func<XML1, bool>> filter = null);

//        Task<Dictionary<string, int>> CountTotalUploads(DateTime createDate);
//    }
//}
