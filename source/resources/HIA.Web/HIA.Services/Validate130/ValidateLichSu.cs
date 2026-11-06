using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HIA.Core.Base;
using HIA.DatabaseModel.Entites;
using HIA.Repositories.Files;

namespace HIA.Services.Validate130
{
    public class ValidateLichSu : BaseValidator<XML01>
    {
        private readonly IXml01Repository _hosoRepository;
        private readonly Dictionary<string, string> _dmDacBiet;

        public string TenBenhChinh { get; private set; }

        public ValidateLichSu
        (
            IXml01Repository hosoRepository,
            Dictionary<string, string> dMDacBiets,
            Dictionary<int, string> errors,
            ILogger<dynamic> logger = null
        ) : base(errors, logger)
        {
            _hosoRepository = hosoRepository;
            _dmDacBiet = dMDacBiets;
        }

        public override async Task<bool> Validate(XML01 xml)
        {
            // Kiem tra neu XML null thi ko chay tiep nua
            if (!await base.Validate(xml)) return false;

            //2365	0	XML1	MA_BENH_CHINH	[130] Bệnh mãn tính dài ngày, mỗi lần khám bệnh của Bệnh nhân không nhỏ hơn 30 ngày
            //if (_dmDacBiet.ContainsKey("BenhManTinhDaiNgay"))
            //{
            //    var mabenhs = _dmDacBiet["BenhManTinhDaiNgay"];
            //    var hoso30Ngays = await _hosoRepository.GetListAsync(x => x.XML01ID != File.XML01ID && x.IsDelete != true && x.MA_CSKCB == File.MA_CSKCB && x.MA_BN == File.MA_BN && x.MA_LK != File.MA_LK && x.NGAY_TTOAN != null && x.NGAY_TTOAN.Value.AddDays(30) <= File.NGAY_TTOAN, query => query.OrderBy(x => x.NGAY_TTOAN));
            //    var hscheck = hoso30Ngays.Where(x => Uti.CodeMatch(x.MA_BENH_CHINH, mabenhs) || (x.MA_BENH_KT != null && Uti.CodeMatch(x.MA_BENH_KT, mabenhs))).FirstOrDefault();
            //    if (hscheck != null)
            //    {
            //        foreach (var mabenhInfor in Info.MA_BENH_ICD10.Split(";"))
            //        {
            //            RaiseError(Uti.CodeContain(mabenhInfor, _dmDacBiet["BenhManTinhDaiNgay"]),
            //                2365, $"Bệnh nhân có mã bệnh mãn tính:{mabenhInfor}|, đã đi khám vào ngày:{hscheck.NGAY_VAO:dd/MM/yyyy HH:mm:ss}, khoảng cách: {(File.NGAY_TTOAN - hscheck.NGAY_TTOAN).Value.TotalDays:F0}, chưa đủ 30 ngày. Kiểm tra lại");
            //        }
            //    }
            //}

            return true;
        }
    }
}
