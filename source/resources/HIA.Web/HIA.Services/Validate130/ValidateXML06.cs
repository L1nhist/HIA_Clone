using HIA.Core.Base;
using HIA.DatabaseModel.Entites;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HIA.Services.Validate130
{
    public class ValidateXML06 : BaseValidator<XML06>
    {
        private readonly IEnumerable<DMBacSi> dmBACSI;

        public ValidateXML06
        (
            IEnumerable<DMBacSi> dMBacSis,
            ILogger<dynamic> logger = null
        ) : base(logger)
        {
            dmBACSI = dMBacSis;
        }

        public override async Task<bool> Validate(XML06 xml)
        {
            var Ma_PD1 = "NL.01;NL.02;NL.03;NL.04;NL.05;NL.06;NL.07;NL.08;NL.09;NL.10;NL.11;NL.12;NL.13;NL.14;NL.15;NL.16;NL.17;NL.18;NL.19;NL.20;NL.21;NL.22;NL.23;NL.24;NL.25;NL.26;NL.27;NL.28;NL.29;";
            var Ma_PD2 = "TE.01;TE.02;TE.03;TE.04;TE.05;TE.06;TE.07;TE.08;TE.09;TE.10;TE.11;TE.12;TE.13;TE.14;TE.15;TE.16;TE.17;TE.18;TE.19;TE.20;";
            var Ma_PD3 = "KHAC;";
            var Ma_xtr = "1;2;3;4;5;6;7;8";
            var Ma_TTDK = "1;2;3;4;5;6;7;8;9";

            //Kiem tra neu XML null thi ko chay tiep nua
            if (!await base.Validate(xml)) return false;

            //Uti.HandleCronJobLog(_logger, $"[130] Bat dau giam dinh ho so XML05", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML05)}");

            #region Kiểm tra các lỗi cơ bản
            //	1828	XML6	MA_LK	Mã liên kết không được để trống
            //	1829	XML6	MA_LK	Mã liên kết không được lớn hơn 100 ký tự
            Field(x => x.MA_LK).IsEmpty().ThenError(1828).LengthOver(100).ThenError(1829);

            //	1830	XML6	MA_LYDO_DTRI	Mã lý do điều trị không được để trống và phải là số nguyên có giá trị từ 1 đến 5
            Field(x => x.MA_LYDO_DTRI).IsEmpty().NotBetweenOrEqual(1, 5).ThenError(1830);

            //	1831	XML6	LOAI_DTRI_LAO	Loại điều trị lao không được để trống và phải là số nguyên có giá trị từ 0 đến 3
            Field(x => x.LOAI_DTRI_LAO).IsEmpty().NotBetweenOrEqual(0, 3).ThenError(1831);

            //	1832	XML6	MA_LOAI_BN	Mã loại bệnh nhân không được để trống và phải là số nguyên có giá trị từ 1 đến 5
            Field(x => x.MA_LOAI_BN).IsEmpty().NotBetweenOrEqual(1, 5).ThenError(1832);

            //	1833	XML6	MA_TINH_TRANG_DK	Mã tình trạng đến khám không được để trống
            //	1834	XML6	MA_TINH_TRANG_DK	Mã tình trạng đến khám không được lớn hơn 18 ký tự và gồm các số nguyên từ 1 đến 9 cách nhau bằng dấu ';', không tồn tại khoảng trắng ở giữa
            Field(x => x.MA_TINH_TRANG_DK).IsEmpty().ThenError(1833).LengthOver(18).EachAny(";", t => t.Length != 1 || EvalHelper.Cast<byte>(t) == null).ThenError(1834);

            //	1835	XML6	MA_XU_TRI	Mã xử trí không được để trống và phải là số nguyên có giá trị từ 1 đến 8
            //2125	4	XML6	MA_XU_TRI	[130] Kiểm tra lại thông tin mã xử trí, nội dung không nằm trong mã từ 1 đến 8
            //*** đã đổi thành kiểu chuỗi
            Field(x => x.MA_XU_TRI).IsEmpty().ThenError(1835);
            if(!Uti.HCodeMach(Ma_xtr,File.MA_XU_TRI.ToString()))
            {
                RaiseError(true, 2115);
            }    

            //	1836	XML6	MA_PHAC_DO_DIEU_TRI	Mã phác đồ điều trị không được để trống
            //	1837	XML6	MA_PHAC_DO_DIEU_TRI	Mã phác đồ điều trị không được lớn hơn 200 ký tự
            
            Field(x => x.MA_PHAC_DO_DIEU_TRI).IsEmpty().ThenError(1836).LengthOver(200).ThenError(1837);

            
            //2131	4	XML6	MA_TINH_TRANG_DK	[130] Mã tình trặng đăng ký phải có giá trị từ 1 đến 9
            if(!Field(x=>x.MA_TINH_TRANG_DK).IsEmpty())
            {
                if(!Uti.HCodeMach(Ma_TTDK,File.MA_TINH_TRANG_DK))
                {
                    RaiseError(true, 2131);
                }    
            }
            //2144    4   XML6 GIAI_DOAN_LAM_SANG[130] Giai đoạn lầm sàng chỉ nhập các mã có từ 1 đến 4
            //2145    4   XML6 NHOM_DOI_TUONG[130] Nhóm đối tượng chỉ nhập các mã từ 0 đến 11
            if (!Field(x => x.GIAI_DOAN_LAM_SANG).IsEmpty())
            {
                Field(x => x.GIAI_DOAN_LAM_SANG).NotBetweenOrEqual(1, 4).ThenError(2144);
            }
            if (!Field(x => x.NHOM_DOI_TUONG).IsEmpty())
            {
                Field(x => x.NHOM_DOI_TUONG).NotBetweenOrEqual(0, 11).ThenError(2145);
            }
            #endregion

            #region Kiểm tra các lỗi có liên quan đến các trường khác
            //2108	4	XML6	PHACDO_DTRI_LAO	[130] [130] Phác đồ điều trị Lao bắt buộc phải nhập, có giá trị từ 1 đến 12
            //2109    4   XML6 NGAYBD_DTRI_LAO[130] Ngày bắt đầu điều trị Lao bắt buộc phải nhập
            //2110    4   XML6 NGAYKT_DTRI_LAO[130] Ngày kết thúc điều trị Lao bắt buộc phải nhập
            //2111	4	XML6	NGAYKT_DTRI_LAO	[130] Ngày kết thúc điều trị không được nhỏ hơn ngày bắt đầu điều trị
            //2142	4	XML6	SANG_LOC_LAO	[130] Mã phương pháp sàng lọc Lao bắt buộc phải nhập, và phải có giá trị từ 1 đến 4
            //2143	4	XML6	KQ_DTRI_LAO	[130] Kết quả điều trị Lao bắt buộc phải nhập, và phải có giá trị từ 1 đến 7
            if (File.LOAI_DTRI_LAO.Value != 0)
            {
                Field(x => x.PHACDO_DTRI_LAO).IsEmpty().ThenError(2108).NotBetweenOrEqual(1,12).ThenError(2108);
                //2694	3	XML6	NULL	[3176] Phác đồ điều trị Lao bắt buộc phải nhập, có giá trị từ 1 đến 27
                Field(x => x.PHACDO_DTRI_LAO).IsEmpty().ThenError(2694).NotBetweenOrEqual(1, 27).ThenError(2694);
                Field(x => x.NGAYBD_DTRI_LAO).IsEmpty().ThenError(2109);
                //Field(x => x.NGAYKT_DTRI_LAO).IsEmpty().ThenError(2110);
                //if(!Field(x=>x.NGAYKT_DTRI_LAO).IsEmpty())
                //{
                //    Field(x => x.NGAYKT_DTRI_LAO).IsUnder(File.NGAYBD_DTRI_LAO).ThenError(2111);
                //}
                Field(x => x.NGAYKT_DTRI_LAO).IsEmpty().ThenError(2110).IsUnder(File.NGAYBD_DTRI_LAO).ThenError(2111);
                Field(x => x.SANG_LOC_LAO).IsEmpty().ThenError(2142).NotBetweenOrEqual(1, 4).ThenError(2142);
                Field(x => x.KQ_DTRI_LAO).IsEmpty().ThenError(2143).NotBetweenOrEqual(1, 7).ThenError(2143);
            }

            //2112	4	XML6	LAN_XN_PCR	[130] Lần xét nghiệm PCR bắt buộc phải nhập
            //2113    4   XML6 NGAY_XN_PCR[130] Ngày xét nghiệm PCR bắt buộc phải nhập
            //2114    4   XML6 NGAY_KQ_XN_PCR[130] Ngày kết quả xét nghiệp PCR bắt buộc phải nhập
            //2115    4   XML6 MA_KQ_XN_PCR[130] Mã kết quả xét nghiệm PCR bắt buộc phải nhập
            if (File.MA_TINH_TRANG_DK=="1")
            {
                Field(x => x.LAN_XN_PCR).IsEmpty().ThenError(2112);
                Field(x => x.NGAY_XN_PCR).IsEmpty().ThenError(2113);
                Field(x => x.NGAY_KQ_XN_PCR).IsEmpty().ThenError(2114);
                Field(x => x.MA_KQ_XN_PCR).IsEmpty().ThenError(2115);
            }
            //2116	4	XML6	NGAY_BAT_DAU_XU_TRI	[130] Ngày bắt đầu xử trí bắt buộc phải nhập
            //2117    4   XML6 NGAY_KET_THUC_XU_TRI[130] Ngày kết thúc xử trý bắt buộc phải nhập
            //2118    4   XML6 NGAY_KET_THUC_XU_TRI[130] Ngày kết thúc xử trí không được nhỏ hơn ngày bắt đầu
            //2119    4   XML6 SO_NGAY_CAP_THUOC_ARV[130] Số ngày cấp thuốc ARV bắt buộc phải nhập
            //2120    4   XML6 SO_NGAY_CAP_THUOC_ARV[130] Số ngày cấp thuốc ARV phải lớn hơn 0
            //2146    4   XML6 SO_NGAY_CAP_THUOC_ARV[130] Số ngày cấp thuốc lớn hơn thực tế(ngày kết thúc -ngày bắt đầu)
            //Mã xử trí đã đổi thành chuỗi
            if (Field(x => x.MA_XU_TRI).HasValue("1"))// File.MA_XU_TRI.Value == 1)
            {
                Field(x => x.NGAY_BAT_DAU_XU_TRI).IsEmpty().ThenError(2116);
                //Field(x => x.NGAY_KET_THUC_XU_TRI).IsEmpty().ThenError(2117);
                //Field(x => x.NGAY_KET_THUC_XU_TRI).IsUnder(File.NGAY_BAT_DAU_XU_TRI).ThenError(2118);
                Field(x => x.NGAY_KET_THUC_XU_TRI).IsEmpty().ThenError(2117).IsUnder(File.NGAY_BAT_DAU_XU_TRI).ThenError(2118);
                //Field(x => x.SO_NGAY_CAP_THUOC_ARV).IsEmpty().ThenError(2119);
                //Field(x => x.SO_NGAY_CAP_THUOC_ARV).IsUnder(1).ThenError(2120);
                Field(x => x.SO_NGAY_CAP_THUOC_ARV).IsEmpty().ThenError(2119).IsUnder(1).ThenError(2120);
                if(Field(x=>x.NGAY_BAT_DAU_XU_TRI).NotEmpty().Field(x=>x.NGAY_KET_THUC_XU_TRI).NotEmpty())
                {
                    var ngaycapthuocARV = (File.NGAY_KET_THUC_XU_TRI.Value - File.NGAY_BAT_DAU_XU_TRI.Value).Days;
                    
                    if(ngaycapthuocARV<=File.SO_NGAY_CAP_THUOC_ARV.Value) 
                    {
                        RaiseError(true, 2146);
                    }
                }


            }
            //2147	4	XML6	LY_DO_CHUYEN_PHAC_DO	[130] Lý do chuyển phác đồ phải có giá trị từ 1 đến 6
            //2148    4   XML6 NGAY_CHUYEN_PHAC_DO[130] Ngày chuyển phác đồ bắt buộc phải nhập
            //2149    4   XML6 MA_CSKCB[130] Mã CSKCB không trùng với mã CSKCB hiện tại

            if(!Field(x=>x.LY_DO_CHUYEN_PHAC_DO).NotEmpty())
            {
                Field(x => x.LY_DO_CHUYEN_PHAC_DO).NotBetweenOrEqual(1, 6).ThenError(2147);
                Field(x => x.NGAY_CHUYEN_PHAC_DO).IsEmpty().ThenError(2148);
            }
            Field(x => x.MA_CSKCB).NotValue(File.XML01.MA_CSKCB).ThenError(2149);
            //2121	4	XML6	MA_THE_BHYT	[130] Mã thẻ BHYT không trùng thông tin với Mã thẻ BHYT tại XML1
            //2122    4   XML6 SO_CCCD[130] Số CCCD không trùng với số CCCD tại XML1
            if (!string.IsNullOrEmpty(File.XML01.MA_THE_BHYT))
            {
                if(!Uti.CodeMatch(File.XML01.MA_THE_BHYT,File.MA_THE_BHYT))
                {
                    RaiseError(true, 2121);
                }    
            }
            if (!string.IsNullOrEmpty(File.XML01.SO_CCCD))
            {
                if (!Uti.CodeMatch(File.XML01.SO_CCCD, File.SO_CCCD))
                {
                    RaiseError(true, 2122);
                }
            }

            //2123	4	XML6	MA_PHAC_DO_DIEU_TRI_BD	[130] Mã Phác đồ điều trị BD không đúng Phụ Lục 10 QĐ 5937/QĐ BYT
            //*** Kiểm tra xem có đúng là trẻ em hay không?
           
            if(Field(x => x.MA_PHAC_DO_DIEU_TRI_BD).NotEmpty())
            {
                if (!Uti.HCodeMach(Ma_PD1 + Ma_PD2 + Ma_PD3,File.MA_PHAC_DO_DIEU_TRI_BD))
                {
                    RaiseError(true, 2123);
                }
            }

            //2127	4	XML6	MA_LYDO_XNTL_VR	[130] Mã lý do xét nghiệm đo tải lượng virus phải có giá trị từ 1 đến 3
            //2128	4	XML6	NGAY_XN_TLVR	[130] Ngày lấy mẫu xét nghiệm tải lượng virus bắt buộc phải nhập
            //2129    4   XML6 KQ_XNTL_VR[130] Mã kết quả xét nghiệm tải lượng virus bắt buộc phải nhập và có giá trị từ 1 đến 5
            //2130    4   XML6 NGAY_KQ_XN_TLVR[130] Thời điểm có kết quả xét nghiệm tải lượng virus bắt buộc phải nhập
            if (Field(x => x.MA_LYDO_XNTL_VR).NotEmpty())
            {
                Field(x => x.MA_LYDO_XNTL_VR).NotBetweenOrEqual(1, 3).ThenError(2127);
                Field(x => x.NGAY_XN_TLVR).IsEmpty().ThenError(2128);
                Field(x => x.KQ_XNTL_VR).IsEmpty().ThenError(2129).NotBetweenOrEqual(1,5).ThenError(2129);
                Field(x => x.NGAY_KQ_XN_TLVR).IsEmpty().ThenError(2130);
            }
            //2126	4	XML6	MA_BAC_PHAC_DO_BD	[130] Mã bậc phác đồ điều trị ban đầu phải có giá trị từ 1 đến 3
            if (!Uti.CodeMatch(Ma_PD3, File.MA_BAC_PHAC_DO_BD.ToString()))
            {
                Field(x => x.MA_BAC_PHAC_DO).NotBetweenOrEqual(1, 3).ThenError(2126);
            }

            //2132	4	XML6	LAN_XN_PCR	[130] Lần xét nghiệp PCR chỉ áp dụng cho trẻ dưới 18 tháng bị phơi nhiễm và phải có giá trị từ 1 đến 3
            //2133    4   XML6 NGAY_XN_PCR[130] Ngày xét nghiệm PCR bắt buộc phải nhập
            //2134    4   XML6 NGAY_KQ_XN_PCR[130] Ngày kết quả xét nghiệm PCR bắt buộc phải nhập
            //2135    4   XML6 MA_KQ_XN_PCR[130] Mã kết quả xét nghiệm PCR bắt buộc phải nhập và phải có giá trị từ 0 đến 1
            //2136	4	XML6	NGAY_KQ_XN_PCR	[130] Ngày kết quả xét nghiệm PCR không được nhỏ hơn ngày xét nghiệm
            //Kiểm tra tình trạng đăng ký có phải là trẻ dưới 18 tháng phơi nhiễm không
            if (Uti.CodeMatch(File.MA_TINH_TRANG_DK,"1"))
            {
                if(!Field(x=>x.LAN_XN_PCR).IsEmpty())
                {
                    Field(x => x.LAN_XN_PCR).NotBetweenOrEqual(1, 3).ThenError(2132);
                    Field(x => x.NGAY_XN_PCR).IsEmpty().ThenError(2133);
                    Field(x => x.NGAY_KQ_XN_PCR).IsEmpty().ThenError(2134).IsUnder(File.NGAY_XN_PCR).ThenError(2136);
                    Field(x => x.MA_KQ_XN_PCR).IsEmpty().ThenError(2135).NotBetweenOrEqual(0,1).ThenError(2135);
                    
                }    
            }

            //2124	4	XML6	NGAY_SINH	[130] Ngày sinh không trùng thông tin tại XML1
            //2137    4   XML6 GIOI_TINH[130] Thông tin giới tính không trùng với nội dung tại XML1
            //2138    4   XML6 DIA_CHI[130] Thông tin địa chỉ không trùng với thông tin địa chỉ tại XML1
            //2139    4   XML6 MATINH_CU_TRU[130] Thông tin mã tỉnh cư trú không khớp với thông tin mã tỉnh cư trú tại XML1
            //2140    4   XML6 MAHUYEN_CU_TRU[130] Thông tin mã huyện cư trú không khớp với thông tin mã huyện cư trú tại XML1
            //2141    4   XML6 MAXA_CU_TRU[130] Thông tin mã xã cư trú không khớp với thông tin mã xã cư trú tại XML1
            //if (!string.IsNullOrEmpty(File.XML01.NGAY_SINH.ToString()))
            //{
                Field(x => x.NGAY_SINH).NotValue(File.XML01.NGAY_SINH).IgnoreEmpty().ThenError(2124);
                
            //}
            //if (!string.IsNullOrEmpty(File.XML01.GIOI_TINH.ToString()))
            //{
                Field(x => x.GIOI_TINH).NotValue(File.XML01.GIOI_TINH).IgnoreEmpty().ThenError(2137);

            //}
            //if (!string.IsNullOrEmpty(File.XML01.DIA_CHI.ToString()))
            //{
                Field(x => x.DIA_CHI).NotValue(File.XML01.DIA_CHI).IgnoreEmpty().ThenError(2138);

            //}
            //if (!string.IsNullOrEmpty(File.XML01.MATINH_CU_TRU.ToString()))
            //{
                Field(x => x.MATINH_CU_TRU).NotValue(File.XML01.MATINH_CU_TRU).IgnoreEmpty().ThenError(2139);

            //}
            //if (!string.IsNullOrEmpty(File.XML01.MAHUYEN_CU_TRU.ToString()))
            //{
                Field(x => x.MAHUYEN_CU_TRU).NotValue(File.XML01.MAHUYEN_CU_TRU).IgnoreEmpty().ThenError(2140);

            //}
            //if (!string.IsNullOrEmpty(File.XML01.MAXA_CU_TRU.ToString()))
            //{
                Field(x => x.MAXA_CU_TRU).NotValue(File.XML01.MAXA_CU_TRU).IgnoreEmpty().ThenError(2141);

            //}
            #endregion

            #region Kiểm tra các lỗi có liên quan đến các Danh mục

            #endregion

            return true;
        }
    }
}