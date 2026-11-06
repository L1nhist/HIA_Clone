using HIA.Core.Base;
using HIA.DatabaseModel.Entites;
using HIA.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HIA.Repositories.Files
{
    public interface IXml01Repository : IBaseRepository<XML01>
    {
        Task<IEnumerable<XML01>> GetAllByMaLK(string maCSKCB, string MA_LK);

        Task<IEnumerable<XML01>> GetAllByListID(string maCSKCB, params long[] ids);

        Task<IEnumerable<T>> GetInnersByListID<T>(string maCSKCB, params long[] ids)
            where T : class, IXMLEntity;

        Task<IEnumerable<XMLError>> GetErrorsByListID(string maCSKCB, params long[] ids);

        Task<IEnumerable<XML01>> GetAllIncludedStateByListID(string maCSKCB, params long[] ids);

        Task<Dictionary<string, int>> CountTotalUploads(DateTime createDate);


        Task<IEnumerable<string>> GetAllListMaCSKCB();

        Task<IEnumerable<XML01>> GetAllListXmlReference(string currentHospitalId);

        Task<IEnumerable<XML01>> GetAllListXmlReferenceLe(int idsLe, string maCSKCB);

        Task<IEnumerable<XML01>> GetAllListXmlReferenceLeString(List<string> idsLe);

        Task<IEnumerable<XML01>> GetAllListXmlReferenceKhoi(string currentHospitalId, Expression<Func<XML01, bool>> filter = null);

        //Linhist added 15/11/22
        Task<IEnumerable<XML01>> GetAllListXmlReference_WithXml4Only(string currentHospitalId, Expression<Func<XML01, bool>> filter = null);

        //Linhist added 09/06/22
        Task<IEnumerable<XML01>> GetListXmlReport3(string currentHospitalId, Expression<Func<XML01, bool>> filter = null, bool includedErrors = false);

        Task<IEnumerable<XML01>> GetAllListXmlReferenceSameDate(string currentHospitalId, DateTime minDate, DateTime MaxDate);

        Task<IEnumerable<XML01>> GetAllListXmlReferenceHaveXML23();

        Task<IEnumerable<XML01>> GetAllListXmlReferenceLeStringFile(List<string> idsLe);

        Task<IEnumerable<XML01>> GetXML1HaveXML4ErrorsOnly(List<int?> xml1Id);

        //Linhist added 09/10/22
        Task<IEnumerable<XML01>> GetXML1AndErrorsOnly(string currentHospitalId, Expression<Func<XML01, bool>> filter = null);
    }
}
