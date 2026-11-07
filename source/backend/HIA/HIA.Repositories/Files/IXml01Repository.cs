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

    Task<IEnumerable<XML01>> GetAllListXmlReference(string currentHospitalId);
}
