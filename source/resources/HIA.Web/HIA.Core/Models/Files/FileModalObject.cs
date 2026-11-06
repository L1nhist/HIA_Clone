using System.Collections.Generic;

namespace HIA.Core.Models.Files
{
    public class XML1InfoModalObject
    {
        public List<XML1ErrorInfoModel> ErrorList { get; set; }
        public XML1InfoModel Xml1Data { get; set; }
    }
}
