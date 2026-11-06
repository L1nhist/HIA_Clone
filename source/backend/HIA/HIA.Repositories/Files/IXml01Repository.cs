using System.Linq.Expressions;
using HIA.Data.Base;
using HIA.Data.Records;
using HIA.Repositories.Base;

namespace HIA.Repositories.Files;

public interface IXml01Repository : IBaseRepository<XML01>
{
    Task<IEnumerable<XML01>> GetAllByMaLK(string maCSKCB, string MA_LK);

    Task<IEnumerable<XML01>> GetAllByListID(string maCSKCB, params long[] ids);

    Task<IEnumerable<T>> GetInnersByListID<T>(string maCSKCB, params long[] ids)
        where T : class, IRecord;

    Task<IEnumerable<XMLError>> GetErrorsByListID(string maCSKCB, params long[] ids);

    Task<IEnumerable<XML01>> GetAllIncludedStateByListID(string maCSKCB, params long[] ids);

    Task<Dictionary<string, int>> CountTotalUploads(DateTime createDate);

    Task<IEnumerable<string>?> GetAllListMaCSKCB();

    Task<IEnumerable<XML01>> GetAllListXmlReference(string currentHospitalId);

    Task<IEnumerable<XML01>?> GetAllListXmlReferenceLe(int idsLe, string maCSKCB);

    Task<IEnumerable<XML01>?> GetAllListXmlReferenceLeString(List<string> idsLe);

    Task<IEnumerable<XML01>?> GetAllListXmlReferenceKhoi(string currentHospitalId, Expression<Func<XML01, bool>>? filter = null);

    Task<IEnumerable<XML01>?> GetAllListXmlReference_WithXml4Only(string currentHospitalId, Expression<Func<XML01, bool>>? filter = null);

    Task<IEnumerable<XML01>?> GetListXmlReport3(string currentHospitalId, Expression<Func<XML01, bool>>? filter = null, bool includedErrors = false);

    Task<IEnumerable<XML01>?> GetAllListXmlReferenceSameDate(string currentHospitalId, DateTime minDate, DateTime MaxDate);

    Task<IEnumerable<XML01>?> GetAllListXmlReferenceLeStringFile(List<string> idsLe);

    Task<IEnumerable<XML01>?> GetXML1AndErrorsOnly(string currentHospitalId, Expression<Func<XML01, bool>>? filter = null);
}
