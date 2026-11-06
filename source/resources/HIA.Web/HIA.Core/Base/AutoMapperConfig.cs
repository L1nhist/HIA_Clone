using AutoMapper;
using HIA.Core.Models.Categories;
using HIA.Core.Models.Files;
using HIA.Core.Models.Hospital;
using HIA.Core.Models.Pagination;
using HIA.Core.Models.User;
using HIA.Core.Models.HDSD;
using HIA.DatabaseModel.Entites;
using System.Linq;
using HIA.Core.Models.FileUploads130;

namespace HIA.Core.Base
{
    public class AutoMapperConfig
    {
        public static IMapper Initialize()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<PaginationRequestModel, FilterDownloadModel>()
                    .ReverseMap();

                cfg.CreateMap<FilesPaginationRequest, FilterDownloadModel>()
                    .ReverseMap();

                cfg.CreateMap<User, ModifyUserModel>()
                    .ReverseMap();

                cfg.CreateMap<User, NewUserModel>()
                    .ReverseMap();

                cfg.CreateMap<ModifyUserModel, NewUserModel>()
                    .ReverseMap();

                cfg.CreateMap<DichVuMaMay, DichVuMaMayInfoModel>()
                    .ForMember(ent => ent.DvmmID, opt => opt.MapFrom(e => e.DVMMID))
                    .ForMember(ent => ent.Ma_CSKCB, opt => opt.MapFrom(e => e.MA_CSKCB))
                    .ForMember(ent => ent.Ma_DICH_VU, opt => opt.MapFrom(e => e.MA_DICH_VU))
                    .ForMember(ent => ent.Ma_MAY, opt => opt.MapFrom(e => e.MA_MAY))
                    .ReverseMap();

                cfg.CreateMap<DichVuMaMay, DichVuMaMayModifyModel>()
                    .ReverseMap();

                cfg.CreateMap<Hospital, ModifyHospitalModel>()
                  .ForMember(ent => ent.Address, opt => opt.MapFrom(e => e.Address))
                  .ForMember(ent => ent.Loai_Tuyen, opt => opt.MapFrom(e => e.Loai_Tuyen))
                  .ForMember(ent => ent.MA_CSKCB, opt => opt.MapFrom(e => e.MA_CSKCB))
                  .ForMember(ent => ent.OfficeTime, opt => opt.MapFrom(e => e.OfficeTime))
                  .ReverseMap();

                cfg.CreateMap<XML01, XML1InfoModel>()
                    .ForMember(ent => ent.XML1ID, opt => opt.MapFrom(e => e.XML01ID))
                    .ForMember(ent => ent.Stt, opt => opt.MapFrom(e => e.STT))
                    .ForMember(ent => ent.Can_NANG, opt => opt.MapFrom(e => e.CAN_NANG))
                    .ForMember(ent => ent.Dia_CHI, opt => opt.MapFrom(e => e.DIA_CHI))
                    .ForMember(ent => ent.Gioi_TINH, opt => opt.MapFrom(e => e.GIOI_TINH))
                    .ForMember(ent => ent.Ket_QUA_DTRI, opt => opt.MapFrom(e => e.KET_QUA_DTRI))
                    .ForMember(ent => ent.Nam_QT, opt => opt.MapFrom(e => e.NAM_QT))
                    .ForMember(ent => ent.Thang_QT, opt => opt.MapFrom(e => e.THANG_QT))
                    .ReverseMap();

                cfg.CreateMap<XML02, XML2InfoModel>()
                    .ForMember(ent => ent.Stt, opt => opt.MapFrom(e => e.STT))
                    .ForMember(ent => ent.Ten_THUOC, opt => opt.MapFrom(e => e.TEN_THUOC))
                    .ForMember(ent => ent.Don_VI_TINH, opt => opt.MapFrom(e => e.DON_VI_TINH))
                    .ForMember(ent => ent.Ham_LUONG, opt => opt.MapFrom(e => e.HAM_LUONG))
                    .ForMember(ent => ent.Duong_DUNG, opt => opt.MapFrom(e => e.DUONG_DUNG))
                    .ForMember(ent => ent.Lieu_DUNG, opt => opt.MapFrom(e => e.LIEU_DUNG))
                    .ForMember(ent => ent.Pham_VI, opt => opt.MapFrom(e => e.PHAM_VI))
                    .ForMember(ent => ent.Don_GIA, opt => opt.MapFrom(e => e.DON_GIA))
                    .ForMember(ent => ent.Muc_HUONG, opt => opt.MapFrom(e => e.MUC_HUONG))
                    .ReverseMap();

                cfg.CreateMap<XML03, XML3InfoModel>()
                    .ForMember(ent => ent.Stt, opt => opt.MapFrom(e => e.STT))
                    .ForMember(ent => ent.Goi_VTYT, opt => opt.MapFrom(e => e.GOI_VTYT))
                    .ForMember(ent => ent.Ten_VAT_TU, opt => opt.MapFrom(e => e.TEN_VAT_TU))
                    .ForMember(ent => ent.Ten_DICH_VU, opt => opt.MapFrom(e => e.TEN_DICH_VU))
                    .ForMember(ent => ent.Don_VI_TINH, opt => opt.MapFrom(e => e.DON_VI_TINH))
                    .ForMember(ent => ent.Pham_VI, opt => opt.MapFrom(e => e.PHAM_VI))
                    .ForMember(ent => ent.Muc_HUONG, opt => opt.MapFrom(e => e.MUC_HUONG))
                    .ReverseMap();

                cfg.CreateMap<DMKhoa, DMKhoaInfoModel>()
                    .ReverseMap();
                cfg.CreateMap<DMKhoa, DMKhoaModifyModel>()
                    .ReverseMap();

                cfg.CreateMap<DMThuoc, DMThuocInfoModel>()
                    .ForMember(ent => ent.THID, opt => opt.MapFrom(e => e.THID))
                    .ForMember(ent => ent.Ma_THUOC, opt => opt.MapFrom(e => e.MA_THUOC))
                    .ForMember(ent => ent.Ten_THUOC, opt => opt.MapFrom(e => e.TEN_THUOC))
                    .ForMember(ent => ent.Ten_HOAT_CHAT, opt => opt.MapFrom(e => e.TEN_HOAT_CHAT))
                    .ForMember(ent => ent.Loai_THUOC, opt => opt.MapFrom(e => e.LOAI_THUOC))
                    .ForMember(ent => ent.Ma_DUONG_DUNG, opt => opt.MapFrom(e => e.MA_DUONG_DUNG))
                    .ForMember(ent => ent.Duong_DUNG, opt => opt.MapFrom(e => e.DUONG_DUNG))
                    .ForMember(ent => ent.Ham_LUONG, opt => opt.MapFrom(e => e.HAM_LUONG))
                    .ForMember(ent => ent.Dang_BAO_CHE, opt => opt.MapFrom(e => e.DANG_BAO_CHE))
                    .ForMember(ent => ent.Pp_CHE_BIEN, opt => opt.MapFrom(e => e.PP_CHEBIEN))
                   .ReverseMap();
                cfg.CreateMap<DMThuoc, DMThuocModifyModel>()
                .ForMember(ent => ent.THID, opt => opt.MapFrom(e => e.THID))
                .ForMember(ent => ent.PP_CHE_BIEN, opt => opt.MapFrom(e => e.PP_CHEBIEN))
                    .ReverseMap();


                cfg.CreateMap<DMVTYT, DMVTYTInfoModel>()
                    .ReverseMap();
                cfg.CreateMap<DMVTYT, DMVTYTModifyModel>()
                    .ReverseMap();

                cfg.CreateMap<DMDichVu, DMDichVuInfoModel>()
                    .ReverseMap();
                cfg.CreateMap<DMDichVu, DMDichVuModifyModel>()
                    .ReverseMap();

                cfg.CreateMap<DMDVThuCong, DMDVThuCongInfoModel>()
                    .ReverseMap();
                cfg.CreateMap<DMDVThuCong, DMDVThuCongModifyModel>()
                    .ReverseMap();

                cfg.CreateMap<DMKhoaBV, DMKhoaBVInfoBasicModel>()
                    .ReverseMap();
                cfg.CreateMap<DMKhoaBV, DMKhoaBVModifyBasicModel>()
                     .ForMember(ent => ent.KBVID, opt => opt.MapFrom(e => e.KBVID))
                    .ReverseMap();

                cfg.CreateMap<DMThietBi, DMThietBiInfoModel>()
                    .ReverseMap();
                cfg.CreateMap<DMThietBi, DMThietBiModifyModel>()
                    .ReverseMap();

                cfg.CreateMap<DMBacSi, DMBacSiInfoModel>()
                         .ForMember(ent => ent.Ma_CSKCB, opt => opt.MapFrom(e => e.MA_CSKCB))
                         .ForMember(ent => ent.Ma_BHXH, opt => opt.MapFrom(e => e.MA_BHXH))
                         .ForMember(ent => ent.Ho_TEN, opt => opt.MapFrom(e => e.HO_TEN))
                         .ForMember(ent => ent.Gioi_TINH, opt => opt.MapFrom(e => e.GIOI_TINH))
                         .ForMember(ent => ent.Ma_KHOA, opt => opt.MapFrom(e => e.MA_KHOA))
                         .ForMember(ent => ent.Ten_KHOA, opt => opt.MapFrom(e => e.TEN_KHOA))
                         .ForMember(ent => ent.Chuc_DANH, opt => opt.MapFrom(e => e.CHUC_DANH))
                         .ForMember(ent => ent.Vi_TRI, opt => opt.MapFrom(e => e.VI_TRI))
                         .ForMember(ent => ent.Ma_CCHN, opt => opt.MapFrom(e => e.MA_CCHN))
                         .ForMember(ent => ent.Ngaycap_CCHN, opt => opt.MapFrom(e => e.NGAYCAP_CCHN))
                         .ForMember(ent => ent.Noicap_CCHN, opt => opt.MapFrom(e => e.NOICAP_CCHN))
                         .ForMember(ent => ent.Phamvi_CM, opt => opt.MapFrom(e => e.PHAMVI_CM))
                         .ForMember(ent => ent.Dvkt_KHAC, opt => opt.MapFrom(e => e.DVKT_KHAC))
                         .ForMember(ent => ent.Vb_PHANCONG, opt => opt.MapFrom(e => e.VB_PHANCONG))
                         .ForMember(ent => ent.Tg_DANGKY, opt => opt.MapFrom(e => e.TG_DANGKY))
                         .ForMember(ent => ent.Tg_NGAY, opt => opt.MapFrom(e => e.TG_NGAY))
                         .ForMember(ent => ent.Tg_TUAN, opt => opt.MapFrom(e => e.TG_TUAN))
                         .ForMember(ent => ent.Cskcb_KHAC, opt => opt.MapFrom(e => e.CSKCB_KHAC))
                         .ForMember(ent => ent.Cskcb_CGKT, opt => opt.MapFrom(e => e.CSKCB_CGKT))
                         .ForMember(ent => ent.Qd_CGKT, opt => opt.MapFrom(e => e.QD_CGKT))
                         .ForMember(ent => ent.Tu_NGAY, opt => opt.MapFrom(e => e.TU_NGAY))
                         .ForMember(ent => ent.Den_NGAY, opt => opt.MapFrom(e => e.DEN_NGAY))
                        .ReverseMap();
                cfg.CreateMap<DMBacSi, DMBacSiModifyModel>()
                         .ForMember(ent => ent.Ma_CSKCB, opt => opt.MapFrom(e => e.MA_CSKCB))
                         .ForMember(ent => ent.Ma_BHXH, opt => opt.MapFrom(e => e.MA_BHXH))
                         .ForMember(ent => ent.Ho_TEN, opt => opt.MapFrom(e => e.HO_TEN))
                         .ForMember(ent => ent.Gioi_TINH, opt => opt.MapFrom(e => e.GIOI_TINH))
                         .ForMember(ent => ent.Ma_KHOA, opt => opt.MapFrom(e => e.MA_KHOA))
                         .ForMember(ent => ent.Ten_KHOA, opt => opt.MapFrom(e => e.TEN_KHOA))
                         .ForMember(ent => ent.Chuc_DANH, opt => opt.MapFrom(e => e.CHUC_DANH))
                         .ForMember(ent => ent.Vi_TRI, opt => opt.MapFrom(e => e.VI_TRI))
                         .ForMember(ent => ent.Ma_CCHN, opt => opt.MapFrom(e => e.MA_CCHN))
                         .ForMember(ent => ent.Ngaycap_CCHN, opt => opt.MapFrom(e => e.NGAYCAP_CCHN))
                         .ForMember(ent => ent.Noicap_CCHN, opt => opt.MapFrom(e => e.NOICAP_CCHN))
                         .ForMember(ent => ent.Phamvi_CM, opt => opt.MapFrom(e => e.PHAMVI_CM))
                         .ForMember(ent => ent.Dvkt_KHAC, opt => opt.MapFrom(e => e.DVKT_KHAC))
                         .ForMember(ent => ent.Vb_PHANCONG, opt => opt.MapFrom(e => e.VB_PHANCONG))
                         .ForMember(ent => ent.Tg_DANGKY, opt => opt.MapFrom(e => e.TG_DANGKY))
                         .ForMember(ent => ent.Tg_NGAY, opt => opt.MapFrom(e => e.TG_NGAY))
                         .ForMember(ent => ent.Tg_TUAN, opt => opt.MapFrom(e => e.TG_TUAN))
                         .ForMember(ent => ent.Cskcb_KHAC, opt => opt.MapFrom(e => e.CSKCB_KHAC))
                         .ForMember(ent => ent.Cskcb_CGKT, opt => opt.MapFrom(e => e.CSKCB_CGKT))
                         .ForMember(ent => ent.Qd_CGKT, opt => opt.MapFrom(e => e.QD_CGKT))
                         .ForMember(ent => ent.Tu_NGAY, opt => opt.MapFrom(e => e.TU_NGAY))
                         .ForMember(ent => ent.Den_NGAY, opt => opt.MapFrom(e => e.DEN_NGAY))
                    .ReverseMap();

                //Linhist added 17/06/22
                cfg.CreateMap<HDSDThuoc, HDSDThuocInfoModel>()
                    .ForMember(ent => ent.Id, opt => opt.MapFrom(e => e.HDSDThuocID))
                    .ForMember(ent => ent.Ma_THUOC, opt => opt.MapFrom(e => e.MA_THUOC))
                    .ForMember(ent => ent.Ten_THUOC, opt => opt.MapFrom(e => e.TEN_THUOC))
                    .ForMember(ent => ent.Duong_DUNG, opt => opt.MapFrom(e => e.DUONG_DUNG))
                    .ForMember(ent => ent.Chi_DINH, opt => opt.MapFrom(e => e.CHI_DINH))
                    .ForMember(ent => ent.Do_TUOI, opt => opt.MapFrom(e => e.TUOI))
                    .ForMember(ent => ent.Co_THAI, opt => opt.MapFrom(e => e.CO_THAI))
                    .ForMember(ent => ent.Cho_CON_BU, opt => opt.MapFrom(e => e.CHO_CON_BU))
                    .ForMember(ent => ent.Ma_BENH, opt => opt.MapFrom(e => e.MA_BENH))
                    .ForMember(ent => ent.Ten_BENH, opt => opt.MapFrom(e => e.TEN_BENH))
                    .ForMember(ent => ent.Thuoc_KHAC, opt => opt.MapFrom(e => e.THUOC_KHAC))
                    .ForMember(ent => ent.Dich_VU, opt => opt.MapFrom(e => e.DICH_VU))
                    .ForMember(ent => ent.Tien_SU_BENH, opt => opt.MapFrom(e => e.TIEN_SU_BENH))
                    .ForMember(ent => ent.Tyle_TT, opt => opt.MapFrom(e => e.TYLE_TT))
                    .ForMember(ent => ent.Tre_SO_SINH, opt => opt.MapFrom(e => e.TRE_SO_SINH))
                    .ForMember(ent => ent.Thoi_GIAN_TOI_THIEU, opt => opt.MapFrom(e => e.THOI_GIAN_TOI_THIEU))
                    .ForMember(ent => ent.Hieu_LUC, opt => opt.MapFrom(e => e.HIEU_LUC))
                    .ForMember(ent => ent.Ma_CSKCB, opt => opt.MapFrom(e => e.MA_CSKCB))
                    .ReverseMap();

                cfg.CreateMap<HDSDThuoc, ModifyHDSDThuocModel>()
                    .ForMember(ent => ent.Id, opt => opt.MapFrom(e => e.HDSDThuocID))
                    .ForMember(ent => ent.Ma_THUOC, opt => opt.MapFrom(e => e.MA_THUOC))
                    .ForMember(ent => ent.Ten_THUOC, opt => opt.MapFrom(e => e.TEN_THUOC))
                    .ForMember(ent => ent.Duong_DUNG, opt => opt.MapFrom(e => e.DUONG_DUNG))
                    .ForMember(ent => ent.Chi_DINH, opt => opt.MapFrom(e => e.CHI_DINH == true))
                    .ForMember(ent => ent.Do_TUOI, opt => opt.MapFrom(e => e.TUOI))
                    .ForMember(ent => ent.Co_THAI, opt => opt.MapFrom(e => e.CO_THAI == true))
                    .ForMember(ent => ent.Cho_CON_BU, opt => opt.MapFrom(e => e.CHO_CON_BU == true))
                    .ForMember(ent => ent.Ma_BENH, opt => opt.MapFrom(e => e.MA_BENH))
                    .ForMember(ent => ent.Ten_BENH, opt => opt.MapFrom(e => e.TEN_BENH))
                    .ForMember(ent => ent.Thuoc_KHAC, opt => opt.MapFrom(e => e.THUOC_KHAC))
                    .ForMember(ent => ent.Dich_VU, opt => opt.MapFrom(e => e.DICH_VU))
                    .ForMember(ent => ent.Tien_SU_BENH, opt => opt.MapFrom(e => e.TIEN_SU_BENH))
                    .ForMember(ent => ent.Tyle_TT, opt => opt.MapFrom(e => e.TYLE_TT))
                    .ForMember(ent => ent.Tre_SO_SINH, opt => opt.MapFrom(e => e.TRE_SO_SINH))
                    .ForMember(ent => ent.Thoi_GIAN_TOI_THIEU, opt => opt.MapFrom(e => e.THOI_GIAN_TOI_THIEU))
                    .ForMember(ent => ent.Ma_LOI, opt => opt.MapFrom(e => e.MA_LOI))
                    .ForMember(ent => ent.Ma_CSKCB, opt => opt.MapFrom(e => e.MA_CSKCB))
                    .ReverseMap();

                cfg.CreateMap<HDSDDichVu, HDSDDichVuInfoModel>()
                    .ForMember(ent => ent.Id, opt => opt.MapFrom(e => e.HDSDDVID))
                    .ForMember(ent => ent.Ma_DICH_VU, opt => opt.MapFrom(e => e.MA_DICH_VU))
                    .ForMember(ent => ent.Ten_DICH_VU, opt => opt.MapFrom(e => e.TEN_DICH_VU))
                    .ForMember(ent => ent.Chi_DINH, opt => opt.MapFrom(e => e.CHI_DINH == true))
                    .ForMember(ent => ent.Do_TUOI, opt => opt.MapFrom(e => e.TUOI))
                    .ForMember(ent => ent.Co_THAI, opt => opt.MapFrom(e => e.CO_THAI == true))
                    .ForMember(ent => ent.Cho_CON_BU, opt => opt.MapFrom(e => e.CHO_CON_BU == true))
                    .ForMember(ent => ent.Ma_BENH, opt => opt.MapFrom(e => e.MA_BENH))
                    .ForMember(ent => ent.Thuoc_KHAC, opt => opt.MapFrom(e => e.THUOC_KHAC))
                    .ForMember(ent => ent.Ma_NHOM, opt => opt.MapFrom(e => e.MA_NHOM))
                    .ForMember(ent => ent.Dich_VU_KHAC, opt => opt.MapFrom(e => e.DICH_VU_KHAC))
                    .ForMember(ent => ent.Tien_SU_BENH, opt => opt.MapFrom(e => e.TIEN_SU_BENH))
                    .ForMember(ent => ent.Tyle_TT, opt => opt.MapFrom(e => e.TYLE_TT))
                    .ForMember(ent => ent.Thoi_GIAN_TOI_THIEU, opt => opt.MapFrom(e => e.THOI_GIAN_TOI_THIEU))
                    .ForMember(ent => ent.Thoi_GIAN_THUC_HIEN, opt => opt.MapFrom(e => e.THOI_GIAN_THUC_HIEN))
                    .ForMember(ent => ent.Hieu_LUC, opt => opt.MapFrom(e => e.HIEU_LUC))
                    .ForMember(ent => ent.Ma_LOAI_KCB, opt => opt.MapFrom(e => e.MA_LOAI_KCB))
                    .ForMember(ent => ent.Ma_CSKCB, opt => opt.MapFrom(e => e.MA_CSKCB))
                    .ForMember(ent => ent.Ma_DICH_VU_KO_CHECK, opt => opt.MapFrom(e => e.MA_DICH_VU_KO_CHECK))
                    .ReverseMap();

                cfg.CreateMap<HDSDDichVu, ModifyHDSDDichVuModel>()
                    .ForMember(ent => ent.Id, opt => opt.MapFrom(e => e.HDSDDVID))
                    .ForMember(ent => ent.Ma_DICH_VU, opt => opt.MapFrom(e => e.MA_DICH_VU))
                    .ForMember(ent => ent.Ten_DICH_VU, opt => opt.MapFrom(e => e.TEN_DICH_VU))
                    .ForMember(ent => ent.Chi_DINH, opt => opt.MapFrom(e => e.CHI_DINH == true))
                    .ForMember(ent => ent.Do_TUOI, opt => opt.MapFrom(e => e.TUOI))
                    .ForMember(ent => ent.Co_THAI, opt => opt.MapFrom(e => e.CO_THAI == true))
                    .ForMember(ent => ent.Cho_CON_BU, opt => opt.MapFrom(e => e.CHO_CON_BU == true))
                    .ForMember(ent => ent.Ma_BENH, opt => opt.MapFrom(e => e.MA_BENH))
                    .ForMember(ent => ent.Thuoc_KHAC, opt => opt.MapFrom(e => e.THUOC_KHAC))
                    .ForMember(ent => ent.Ma_NHOM, opt => opt.MapFrom(e => e.MA_NHOM))
                    .ForMember(ent => ent.Dich_VU_KHAC, opt => opt.MapFrom(e => e.DICH_VU_KHAC))
                    .ForMember(ent => ent.Tien_SU_BENH, opt => opt.MapFrom(e => e.TIEN_SU_BENH))
                    .ForMember(ent => ent.Tyle_TT, opt => opt.MapFrom(e => e.TYLE_TT))
                    .ForMember(ent => ent.Thoi_GIAN_TOI_THIEU, opt => opt.MapFrom(e => e.THOI_GIAN_TOI_THIEU))
                    .ForMember(ent => ent.Thoi_GIAN_THUC_HIEN, opt => opt.MapFrom(e => e.THOI_GIAN_THUC_HIEN))
                    .ForMember(ent => ent.Ma_LOI, opt => opt.MapFrom(e => e.MA_LOI))
                    .ForMember(ent => ent.Ma_LOAI_KCB, opt => opt.MapFrom(e => e.MA_LOAI_KCB))
                    .ForMember(ent => ent.Ma_CSKCB, opt => opt.MapFrom(e => e.MA_CSKCB))
                    .ForMember(ent => ent.Ma_DICH_VU_KO_CHECK, opt => opt.MapFrom(e => e.MA_DICH_VU_KO_CHECK))
                    .ReverseMap();
            });
            return config.CreateMapper();
        }
    }
}
