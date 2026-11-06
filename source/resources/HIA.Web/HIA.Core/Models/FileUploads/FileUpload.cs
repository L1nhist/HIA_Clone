using HIA.DatabaseModel.Entites;
using Microsoft.AspNetCore.Http;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;


namespace HIA.Core.Models.FileUploads
{

    public class FileUpload
    {
        [Required]
        [Display(Name = "File")]
        public List<IFormFile> FormFiles { get; set; } // convert to list
        public string SuccessMessage { get; set; }
    }
    public class DocumentViewmodel
    {
        public int Id { get; set; }
        [MaxLength(250)]
        public string FileName { get; set; }
        [MaxLength(100)]
        public string FileType { get; set; }
        [MaxLength]
        public byte[] FileData { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }
    //xml 5


    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class DSACH_CHI_TIET_DIEN_BIEN_BENH
    {

        private DSACH_CHI_TIET_DIEN_BIEN_BENHCHI_TIET_DIEN_BIEN_BENH[] cHI_TIET_DIEN_BIEN_BENHField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("CHI_TIET_DIEN_BIEN_BENH")]
        public DSACH_CHI_TIET_DIEN_BIEN_BENHCHI_TIET_DIEN_BIEN_BENH[] CHI_TIET_DIEN_BIEN_BENH
        {
            get
            {
                return this.cHI_TIET_DIEN_BIEN_BENHField;
            }
            set
            {
                this.cHI_TIET_DIEN_BIEN_BENHField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DSACH_CHI_TIET_DIEN_BIEN_BENHCHI_TIET_DIEN_BIEN_BENH
    {

        private string mA_LKField;

        private int sTTField;

        private string dIEN_BIENField;

        private string hOI_CHANField;

        private string pHAU_THUATField;

        private string nGAY_YLField;

        /// <remarks/>
        public string MA_LK
        {
            get
            {
                return this.mA_LKField;
            }
            set
            {
                this.mA_LKField = value;
            }
        }

        /// <remarks/>
        public int STT
        {
            get
            {
                return this.sTTField;
            }
            set
            {
                this.sTTField = value;
            }
        }

        /// <remarks/>
        public string DIEN_BIEN
        {
            get
            {
                return this.dIEN_BIENField;
            }
            set
            {
                this.dIEN_BIENField = value;
            }
        }

        /// <remarks/>
        public string HOI_CHAN
        {
            get
            {
                return this.hOI_CHANField;
            }
            set
            {
                this.hOI_CHANField = value;
            }
        }

        /// <remarks/>
        public string PHAU_THUAT
        {
            get
            {
                return this.pHAU_THUATField;
            }
            set
            {
                this.pHAU_THUATField = value;
            }
        }

        /// <remarks/>
        public string NGAY_YL
        {
            get
            {
                return this.nGAY_YLField;
            }
            set
            {
                this.nGAY_YLField = value;
            }
        }
    }



    //XML 4

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class DSACH_CHI_TIET_CLS
    {

        private DSACH_CHI_TIET_CLSCHI_TIET_CLS[] cHI_TIET_CLSField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("CHI_TIET_CLS")]
        public DSACH_CHI_TIET_CLSCHI_TIET_CLS[] CHI_TIET_CLS
        {
            get
            {
                return this.cHI_TIET_CLSField;
            }
            set
            {
                this.cHI_TIET_CLSField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DSACH_CHI_TIET_CLSCHI_TIET_CLS
    {

        private string mA_LKField;

        private int sTTField;

        private string mA_DICH_VUField;

        private string mA_CHI_SOField;

        private string tEN_CHI_SOField;

        private string gIA_TRIField;

        private string mA_MAYField;

        private string mO_TAField;

        private string kET_LUANField;

        private string nGAY_KQField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string MA_LK
        {
            get
            {
                return this.mA_LKField;
            }
            set
            {
                this.mA_LKField = value;
            }
        }

        /// <remarks/>
        public int STT
        {
            get
            {
                return this.sTTField;
            }
            set
            {
                this.sTTField = value;
            }
        }

        /// <remarks/>
        public string MA_DICH_VU
        {
            get
            {
                return this.mA_DICH_VUField;
            }
            set
            {
                this.mA_DICH_VUField = value;
            }
        }

        /// <remarks/>
        public string MA_CHI_SO
        {
            get
            {
                return this.mA_CHI_SOField;
            }
            set
            {
                this.mA_CHI_SOField = value;
            }
        }

        /// <remarks/>
        public string TEN_CHI_SO
        {
            get
            {
                return this.tEN_CHI_SOField;
            }
            set
            {
                this.tEN_CHI_SOField = value;
            }
        }

        /// <remarks/>
        public string GIA_TRI
        {
            get
            {
                return this.gIA_TRIField;
            }
            set
            {
                this.gIA_TRIField = value;
            }
        }

        /// <remarks/>
        public string MA_MAY
        {
            get
            {
                return this.mA_MAYField;
            }
            set
            {
                this.mA_MAYField = value;
            }
        }

        /// <remarks/>
        public string MO_TA
        {
            get
            {
                return this.mO_TAField;
            }
            set
            {
                this.mO_TAField = value;
            }
        }

        /// <remarks/>
        public string KET_LUAN
        {
            get
            {
                return this.kET_LUANField;
            }
            set
            {
                this.kET_LUANField = value;
            }
        }

        /// <remarks/>
        public string NGAY_KQ
        {
            get
            {
                return this.nGAY_KQField;
            }
            set
            {
                this.nGAY_KQField = value;
            }
        }
    }


    //xml3


    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class GIAMDINHHS
    {

        private GIAMDINHHSTHONGTINDONVI tHONGTINDONVIField;

        private GIAMDINHHSTHONGTINHOSO tHONGTINHOSOField;

        private object cHUKYDONVIField;

        /// <remarks/>
        public GIAMDINHHSTHONGTINDONVI THONGTINDONVI
        {
            get
            {
                return this.tHONGTINDONVIField;
            }
            set
            {
                this.tHONGTINDONVIField = value;
            }
        }

        /// <remarks/>
        public GIAMDINHHSTHONGTINHOSO THONGTINHOSO
        {
            get
            {
                return this.tHONGTINHOSOField;
            }
            set
            {
                this.tHONGTINHOSOField = value;
            }
        }
        public object CHUKYDONVI

        {

            get

            {

                return this.cHUKYDONVIField;

            }

            set

            {

                this.cHUKYDONVIField = value;

            }

        }

    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class GIAMDINHHSTHONGTINDONVI
    {

        private double mACSKCBField;

        /// <remarks/>
        public double MACSKCB
        {
            get
            {
                return this.mACSKCBField;
            }
            set
            {
                this.mACSKCBField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class GIAMDINHHSTHONGTINHOSO
    {

        private string nGAYLAPField;

        private ushort sOLUONGHOSOField;

        private GIAMDINHHSTHONGTINHOSOHOSOFILEHOSO[][] dANHSACHHOSOField;

        /// <remarks/>
        //haimaster convert NgAYLAP to String
        public string NGAYLAP
        {
            get
            {
                return this.nGAYLAPField;
            }
            set
            {
                this.nGAYLAPField = value;
            }
        }

        /// <remarks/>
        public ushort SOLUONGHOSO
        {
            get
            {
                return this.sOLUONGHOSOField;
            }
            set
            {
                this.sOLUONGHOSOField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("HOSO", IsNullable = false)]
        [System.Xml.Serialization.XmlArrayItemAttribute("FILEHOSO", IsNullable = false, NestingLevel = 1)]
        public GIAMDINHHSTHONGTINHOSOHOSOFILEHOSO[][] DANHSACHHOSO
        {
            get
            {
                return this.dANHSACHHOSOField;
            }
            set
            {
                this.dANHSACHHOSOField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class GIAMDINHHSTHONGTINHOSOHOSOFILEHOSO
    {

        private string lOAIHOSOField;

        private string nOIDUNGFILEField;

        /// <remarks/>
        public string LOAIHOSO
        {
            get
            {
                return this.lOAIHOSOField;
            }
            set
            {
                this.lOAIHOSOField = value;
            }
        }

        /// <remarks/>
        public string NOIDUNGFILE
        {
            get
            {
                return this.nOIDUNGFILEField;
            }
            set
            {
                this.nOIDUNGFILEField = value;
            }
        }
    }






    //xml2

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public class DSACH_CHI_TIET_THUOC
    {

        private DSACH_CHI_TIET_THUOCCHI_TIET_THUOC[] cHI_TIET_THUOCField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("CHI_TIET_THUOC")]
        public DSACH_CHI_TIET_THUOCCHI_TIET_THUOC[] CHI_TIET_THUOC
        {
            get
            {
                return this.cHI_TIET_THUOCField;
            }
            set
            {
                this.cHI_TIET_THUOCField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public class DSACH_CHI_TIET_THUOCCHI_TIET_THUOC
    {

        private string mA_LKField;

        private int sTTField;

        private string mA_THUOCField;

        private byte mA_NHOMField;

        private string tEN_THUOCField;

        private string dON_VI_TINHField;

        private string hAM_LUONGField;

        private string dUONG_DUNGField;

        private string lIEU_DUNGField;

        private string sO_DANG_KYField;

        private string tT_THAUField;

        private byte pHAM_VIField;

        private double tYLE_TTField;

        private double sO_LUONGField;

        private double dON_GIAField;

        private double tHANH_TIENField;

        private double mUC_HUONGField;

        //private double t_NGUONKHACField;
        private string t_NGUONKHACField;

        private double t_BNTTField;

        private double t_BHTTField;

        private double t_BNCCTField;

        //private double t_NGOAIDSField;
        private string t_NGOAIDSField;

        private string mA_KHOAField;

        private string mA_BAC_SIField;

        private string mA_BENHField;

        private string nGAY_YLField;

        private byte mA_PTTTField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string MA_LK
        {
            get
            {
                return this.mA_LKField;
            }
            set
            {
                this.mA_LKField = value;
            }
        }

        /// <remarks/>
        public int STT
        {
            get
            {
                return this.sTTField;
            }
            set
            {
                this.sTTField = value;
            }
        }

        /// <remarks/>
        public string MA_THUOC
        {
            get
            {
                return this.mA_THUOCField;
            }
            set
            {
                this.mA_THUOCField = value;
            }
        }

        /// <remarks/>
        public byte MA_NHOM
        {
            get
            {
                return this.mA_NHOMField;
            }
            set
            {
                this.mA_NHOMField = value;
            }
        }

        /// <remarks/>
        public string TEN_THUOC
        {
            get
            {
                return this.tEN_THUOCField;
            }
            set
            {
                this.tEN_THUOCField = value;
            }
        }

        /// <remarks/>
        public string DON_VI_TINH
        {
            get
            {
                return this.dON_VI_TINHField;
            }
            set
            {
                this.dON_VI_TINHField = value;
            }
        }

        /// <remarks/>
        public string HAM_LUONG
        {
            get
            {
                return this.hAM_LUONGField;
            }
            set
            {
                this.hAM_LUONGField = value;
            }
        }

        /// <remarks/>
        public string DUONG_DUNG
        {
            get
            {
                return this.dUONG_DUNGField;
            }
            set
            {
                this.dUONG_DUNGField = value;
            }
        }

        /// <remarks/>
        public string LIEU_DUNG
        {
            get
            {
                return this.lIEU_DUNGField;
            }
            set
            {
                this.lIEU_DUNGField = value;
            }
        }

        /// <remarks/>
        public string SO_DANG_KY
        {
            get
            {
                return this.sO_DANG_KYField;
            }
            set
            {
                this.sO_DANG_KYField = value;
            }
        }

        /// <remarks/>
        public string TT_THAU
        {
            get
            {
                return this.tT_THAUField;
            }
            set
            {
                this.tT_THAUField = value;
            }
        }

        /// <remarks/>
        public byte PHAM_VI
        {
            get
            {
                return this.pHAM_VIField;
            }
            set
            {
                this.pHAM_VIField = value;
            }
        }

        /// <remarks/>
        public double TYLE_TT
        {
            get
            {
                return this.tYLE_TTField;
            }
            set
            {
                this.tYLE_TTField = value;
            }
        }

        /// <remarks/>
        public double SO_LUONG
        {
            get
            {
                return this.sO_LUONGField;
            }
            set
            {
                this.sO_LUONGField = value;
            }
        }

        /// <remarks/>
        public double DON_GIA
        {
            get
            {
                return this.dON_GIAField;
            }
            set
            {
                this.dON_GIAField = value;
            }
        }

        /// <remarks/>
        public double THANH_TIEN
        {
            get
            {
                return this.tHANH_TIENField;
            }
            set
            {
                this.tHANH_TIENField = value;
            }
        }

        /// <remarks/>
        public double MUC_HUONG
        {
            get
            {
                return this.mUC_HUONGField;
            }
            set
            {
                this.mUC_HUONGField = value;
            }
        }

        /// <remarks/>
        //public double T_NGUONKHAC
        public string T_NGUONKHAC
        {
            get
            {
                return this.t_NGUONKHACField;
            }
            set
            {
                this.t_NGUONKHACField = value;
            }
        }

        /// <remarks/>
        public double T_BNTT
        {
            get
            {
                return this.t_BNTTField;
            }
            set
            {
                this.t_BNTTField = value;
            }
        }

        /// <remarks/>
        public double T_BHTT
        {
            get
            {
                return this.t_BHTTField;
            }
            set
            {
                this.t_BHTTField = value;
            }
        }

        /// <remarks/>
        public double T_BNCCT
        {
            get
            {
                return this.t_BNCCTField;
            }
            set
            {
                this.t_BNCCTField = value;
            }
        }

        /// <remarks/>
        //public double T_NGOAIDS
        public string T_NGOAIDS
        {
            get
            {
                return this.t_NGOAIDSField;
            }
            set
            {
                this.t_NGOAIDSField = value;
            }
        }

        /// <remarks/>
        public string MA_KHOA
        {
            get
            {
                return this.mA_KHOAField;
            }
            set
            {
                this.mA_KHOAField = value;
            }
        }

        /// <remarks/>
        public string MA_BAC_SI
        {
            get
            {
                return this.mA_BAC_SIField;
            }
            set
            {
                this.mA_BAC_SIField = value;
            }
        }

        /// <remarks/>
        public string MA_BENH
        {
            get
            {
                return this.mA_BENHField;
            }
            set
            {
                this.mA_BENHField = value;
            }
        }

        /// <remarks/>
        public string NGAY_YL
        {
            get
            {
                return this.nGAY_YLField;
            }
            set
            {
                this.nGAY_YLField = value;
            }
        }

        /// <remarks/>
        public byte MA_PTTT
        {
            get
            {
                return this.mA_PTTTField;
            }
            set
            {
                this.mA_PTTTField = value;
            }
        }
    }

    //xml3
    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public class DSACH_CHI_TIET_DVKT
    {

        private DSACH_CHI_TIET_DVKTCHI_TIET_DVKT[] cHI_TIET_DVKTField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("CHI_TIET_DVKT")]
        public DSACH_CHI_TIET_DVKTCHI_TIET_DVKT[] CHI_TIET_DVKT
        {
            get
            {
                return this.cHI_TIET_DVKTField;
            }
            set
            {
                this.cHI_TIET_DVKTField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public class DSACH_CHI_TIET_DVKTCHI_TIET_DVKT
    {

        private string mA_LKField;

        private int sTTField;

        private string mA_DICH_VUField;

        private string mA_VAT_TUField;

        private byte mA_NHOMField;

        private string gOI_VTYTField;

        private string tEN_VAT_TUField;

        private string tEN_DICH_VUField;

        private string dON_VI_TINHField;

        private byte pHAM_VIField;

        private double sO_LUONGField;

        private double dON_GIAField;

        private string tT_THAUField;

        private double tYLE_TTField;

        private double tHANH_TIENField;

        private string t_TRANTTField;

        private double mUC_HUONGField;

        //private double t_NGUONKHACField;
        private string t_NGUONKHACField;

        private double t_BNTTField;

        private double t_BHTTField;

        private double t_BNCCTField;
        //24.12.2021 haimaster comment Doi double to string
        private string t_NGOAIDSField;

        private string mA_KHOAField;

        private string mA_GIUONGField;

        private string mA_BAC_SIField;

        private string mA_BENHField;

        private string nGAY_YLField;

        private string nGAY_KQField;

        private byte mA_PTTTField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string MA_LK
        {
            get
            {
                return this.mA_LKField;
            }
            set
            {
                this.mA_LKField = value;
            }
        }

        /// <remarks/>
        public int STT
        {
            get
            {
                return this.sTTField;
            }
            set
            {
                this.sTTField = value;
            }
        }

        /// <remarks/>
        public string MA_DICH_VU
        {
            get
            {
                return this.mA_DICH_VUField;
            }
            set
            {
                this.mA_DICH_VUField = value;
            }
        }

        /// <remarks/>
        public string MA_VAT_TU
        {
            get
            {
                return this.mA_VAT_TUField;
            }
            set
            {
                this.mA_VAT_TUField = value;
            }
        }

        /// <remarks/>
        public byte MA_NHOM
        {
            get
            {
                return this.mA_NHOMField;
            }
            set
            {
                this.mA_NHOMField = value;
            }
        }

        /// <remarks/>
        public string GOI_VTYT
        {
            get
            {
                return this.gOI_VTYTField;
            }
            set
            {
                this.gOI_VTYTField = value;
            }
        }

        /// <remarks/>
        public string TEN_VAT_TU
        {
            get
            {
                return this.tEN_VAT_TUField;
            }
            set
            {
                this.tEN_VAT_TUField = value;
            }
        }

        /// <remarks/>
        public string TEN_DICH_VU
        {
            get
            {
                return this.tEN_DICH_VUField;
            }
            set
            {
                this.tEN_DICH_VUField = value;
            }
        }

        /// <remarks/>
        public string DON_VI_TINH
        {
            get
            {
                return this.dON_VI_TINHField;
            }
            set
            {
                this.dON_VI_TINHField = value;
            }
        }

        /// <remarks/>
        public byte PHAM_VI
        {
            get
            {
                return this.pHAM_VIField;
            }
            set
            {
                this.pHAM_VIField = value;
            }
        }

        /// <remarks/>
        public double SO_LUONG
        {
            get
            {
                return this.sO_LUONGField;
            }
            set
            {
                this.sO_LUONGField = value;
            }
        }

        /// <remarks/>
        public double DON_GIA
        {
            get
            {
                return this.dON_GIAField;
            }
            set
            {
                this.dON_GIAField = value;
            }
        }

        /// <remarks/>
        public string TT_THAU
        {
            get
            {
                return this.tT_THAUField;
            }
            set
            {
                this.tT_THAUField = value;
            }
        }

        /// <remarks/>
        public double TYLE_TT
        {
            get
            {
                return this.tYLE_TTField;
            }
            set
            {
                this.tYLE_TTField = value;
            }
        }

        /// <remarks/>
        public double THANH_TIEN
        {
            get
            {
                return this.tHANH_TIENField;
            }
            set
            {
                this.tHANH_TIENField = value;
            }
        }

        /// <remarks/>
        public string T_TRANTT
        {
            get
            {
                return this.t_TRANTTField;
            }
            set
            {
                this.t_TRANTTField = value;
            }
        }

        /// <remarks/>
        public double MUC_HUONG
        {
            get
            {
                return this.mUC_HUONGField;
            }
            set
            {
                this.mUC_HUONGField = value;
            }
        }

        /// <remarks/>
        //public double T_NGUONKHAC
        public string T_NGUONKHAC
        {
            get
            {
                return this.t_NGUONKHACField;
            }
            set
            {
                this.t_NGUONKHACField = value;
            }
        }

        /// <remarks/>
        public double T_BNTT
        {
            get
            {
                return this.t_BNTTField;
            }
            set
            {
                this.t_BNTTField = value;
            }
        }

        /// <remarks/>
        public double T_BHTT
        {
            get
            {
                return this.t_BHTTField;
            }
            set
            {
                this.t_BHTTField = value;
            }
        }

        /// <remarks/>
        public double T_BNCCT
        {
            get
            {
                return this.t_BNCCTField;
            }
            set
            {
                this.t_BNCCTField = value;
            }
        }

        /// <remarks/>
        public string T_NGOAIDS
        {
            get
            {
                return this.t_NGOAIDSField;
            }
            set
            {
                this.t_NGOAIDSField = value;
            }
        }

        /// <remarks/>
        public string MA_KHOA
        {
            get
            {
                return this.mA_KHOAField;
            }
            set
            {
                this.mA_KHOAField = value;
            }
        }

        /// <remarks/>
        public string MA_GIUONG
        {
            get
            {
                return this.mA_GIUONGField;
            }
            set
            {
                this.mA_GIUONGField = value;
            }
        }

        /// <remarks/>
        public string MA_BAC_SI
        {
            get
            {
                return this.mA_BAC_SIField;
            }
            set
            {
                this.mA_BAC_SIField = value;
            }
        }

        /// <remarks/>
        public string MA_BENH
        {
            get
            {
                return this.mA_BENHField;
            }
            set
            {
                this.mA_BENHField = value;
            }
        }

        /// <remarks/>
        public string NGAY_YL
        {
            get
            {
                return this.nGAY_YLField;
            }
            set
            {
                this.nGAY_YLField = value;
            }
        }

        /// <remarks/>
        public string NGAY_KQ
        {
            get
            {
                return this.nGAY_KQField;
            }
            set
            {
                this.nGAY_KQField = value;
            }
        }

        /// <remarks/>
        public byte MA_PTTT
        {
            get
            {
                return this.mA_PTTTField;
            }
            set
            {
                this.mA_PTTTField = value;
            }
        }
    }



    //xml 1

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]

    public partial class TONG_HOP
    {

        private string mA_LKField;

        private int sTTField;

        private string mA_BNField;

        private string hO_TENField;

        private string nGAY_SINHField;

        private byte gIOI_TINHField;

        private string dIA_CHIField;

        private string mA_THEField;

        private string mA_DKBDField;

        private string gT_THE_TUField;

        private string gT_THE_DENField;

        private string mIEN_CUNG_CTField;

        private string tEN_BENHField;

        private string mA_BENHField;

        private string mA_BENHKHACField;

        private string mA_LYDO_VVIENField;

        private string mA_NOI_CHUYENField;

        private string mA_TAI_NANField;

        private string nGAY_VAOField;

        private string nGAY_RAField;

        private int sO_NGAY_DTRIField;

        private byte kET_QUA_DTRIField;

        private byte tINH_TRANG_RVField;

        private string nGAY_TTOANField;

        //private double t_THUOCField;
        private string t_THUOCField;

        //private double t_VTYTField;
        private string t_VTYTField;

        //private double t_TONGCHIField;
        private string t_TONGCHIField;

        //private double t_BNTTField;
        private string t_BNTTField;

        //private double t_BNCCTField;
        private string t_BNCCTField;

        //private double t_BHTTField;
        private string t_BHTTField;

        //private double? t_NGUONKHACField;

        private string t_NGUONKHACField;

        private string t_NGOAIDSField;

        private int nAM_QTField;

        private byte tHANG_QTField;

        private byte mA_LOAI_KCBField;

        private string mA_KHOAField;

        private string mA_CSKCBField;

        private string mA_KHUVUCField;

        private string mA_PTTT_QTField;

        private string cAN_NANGField;

        //<TONG_HOP>
        //  <MA_LK>24.009218/NT</MA_LK>
        //  <STT>1250</STT>
        //  <MA_BN>22038713</MA_BN>
        //  <HO_TEN>Đinh Thị Tân</HO_TEN>
        //  <NGAY_SINH>19731006</NGAY_SINH>
        //  <GIOI_TINH>2</GIOI_TINH>
        //  <DIA_CHI>Ấp Ruộng Tre, Xã Bảo Quang, Thành phố Long Khánh, Tỉnh Đồng Nai</DIA_CHI>
        //  <MA_THE>DN4754420131467</MA_THE>
        //  <MA_DKBD>75009</MA_DKBD>
        //  <GT_THE_TU>20240101</GT_THE_TU>
        //  <GT_THE_DEN>20241231</GT_THE_DEN>
        //  <MIEN_CUNG_CT></MIEN_CUNG_CT>
        //  <TEN_BENH>Các viêm khớp khác (M13)</TEN_BENH>
        //  <MA_BENH>M13</MA_BENH>
        //  <MA_BENHKHAC></MA_BENHKHAC>
        //  <MA_LYDO_VVIEN>1</MA_LYDO_VVIEN>
        //  <MA_NOI_CHUYEN></MA_NOI_CHUYEN>
        //  <MA_TAI_NAN></MA_TAI_NAN>
        //  <NGAY_VAO>202404030650</NGAY_VAO>
        //  <NGAY_RA>202404050800</NGAY_RA>
        //  <SO_NGAY_DTRI>2</SO_NGAY_DTRI>
        //  <KET_QUA_DTRI>2</KET_QUA_DTRI>
        //  <TINH_TRANG_RV>1</TINH_TRANG_RV>
        //  <NGAY_TTOAN>202404050812</NGAY_TTOAN>
        //  <T_THUOC>0.00</T_THUOC>
        //  <T_VTYT>0.00</T_VTYT>
        //  <T_TONGCHI>343050.00</T_TONGCHI>
        //  <T_BNTT>0.00</T_BNTT>
        //  <T_BNCCT>68610.00</T_BNCCT>
        //  <T_BHTT>274440.00</T_BHTT>
        //  <T_NGUONKHAC></T_NGUONKHAC>
        //  <T_NGOAIDS></T_NGOAIDS>
        //  <NAM_QT>2024</NAM_QT>
        //  <THANG_QT>4</THANG_QT>
        //  <MA_LOAI_KCB>2</MA_LOAI_KCB>
        //  <MA_KHOA>K31</MA_KHOA>
        //  <MA_CSKCB>75009</MA_CSKCB>
        //  <MA_KHUVUC></MA_KHUVUC>
        //  <MA_PTTT_QT></MA_PTTT_QT>
        //  <CAN_NANG></CAN_NANG>
        //</TONG_HOP>
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string MA_LK
        {
            get
            {
                return this.mA_LKField;
            }
            set
            {
                this.mA_LKField = value;
            }
        }

        /// <remarks/>
        public int STT
        {
            get
            {
                return this.sTTField;
            }
            set
            {
                this.sTTField = value;
            }
        }

        /// <remarks/>
        public string MA_BN
        {
            get
            {
                return this.mA_BNField;
            }
            set
            {
                this.mA_BNField = value;
            }
        }

        /// <remarks/>
        public string HO_TEN
        {
            get
            {
                return this.hO_TENField;
            }
            set
            {
                this.hO_TENField = value;
            }
        }

        /// <remarks/>
        public string NGAY_SINH
        {
            get
            {
                return this.nGAY_SINHField;
            }
            set
            {
                this.nGAY_SINHField = value;
            }
        }

        /// <remarks/>
        public byte GIOI_TINH
        {
            get
            {
                return this.gIOI_TINHField;
            }
            set
            {
                this.gIOI_TINHField = value;
            }
        }

        /// <remarks/>
        public string DIA_CHI
        {
            get
            {
                return this.dIA_CHIField;
            }
            set
            {
                this.dIA_CHIField = value;
            }
        }

        /// <remarks/>
        public string MA_THE
        {
            get
            {
                return this.mA_THEField;
            }
            set
            {
                this.mA_THEField = value;
            }
        }

        /// <remarks/>
        public string MA_DKBD
        {
            get
            {
                return this.mA_DKBDField;
            }
            set
            {
                this.mA_DKBDField = value;
            }
        }

        /// <remarks/>
        public string GT_THE_TU
        {
            get
            {
                return this.gT_THE_TUField;
            }
            set
            {
                this.gT_THE_TUField = value;
            }
        }

        /// <remarks/>
        public string GT_THE_DEN
        {
            get
            {
                return this.gT_THE_DENField;
            }
            set
            {
                this.gT_THE_DENField = value;
            }
        }

        /// <remarks/>
        public string MIEN_CUNG_CT
        {
            get
            {
                return this.mIEN_CUNG_CTField;
            }
            set
            {
                this.mIEN_CUNG_CTField = value;
            }
        }

        /// <remarks/>
        public string TEN_BENH
        {
            get
            {
                return this.tEN_BENHField;
            }
            set
            {
                this.tEN_BENHField = value;
            }
        }

        /// <remarks/>
        public string MA_BENH
        {
            get
            {
                return this.mA_BENHField;
            }
            set
            {
                this.mA_BENHField = value;
            }
        }

        /// <remarks/>
        public string MA_BENHKHAC
        {
            get
            {
                return this.mA_BENHKHACField;
            }
            set
            {
                this.mA_BENHKHACField = value;
            }
        }

        /// <remarks/>
        public string MA_LYDO_VVIEN
        {
            get
            {
                return this.mA_LYDO_VVIENField;
            }
            set
            {
                this.mA_LYDO_VVIENField = value;
            }
        }

        /// <remarks/>
        public string MA_NOI_CHUYEN
        {
            get
            {
                return this.mA_NOI_CHUYENField;
            }
            set
            {
                this.mA_NOI_CHUYENField = value;
            }
        }

        /// <remarks/>
        public string MA_TAI_NAN
        {
            get
            {
                return this.mA_TAI_NANField;
            }
            set
            {
                this.mA_TAI_NANField = value;
            }
        }

        /// <remarks/>
        public string NGAY_VAO
        {
            get
            {
                return this.nGAY_VAOField;
            }
            set
            {
                this.nGAY_VAOField = value;
            }
        }

        /// <remarks/>
        public string NGAY_RA
        {
            get
            {
                return this.nGAY_RAField;
            }
            set
            {
                this.nGAY_RAField = value;
            }
        }

        /// <remarks/>
        public int SO_NGAY_DTRI
        {
            get
            {
                return this.sO_NGAY_DTRIField;
            }
            set
            {
                this.sO_NGAY_DTRIField = value;
            }
        }

        /// <remarks/>
        public byte KET_QUA_DTRI
        {
            get
            {
                return this.kET_QUA_DTRIField;
            }
            set
            {
                this.kET_QUA_DTRIField = value;
            }
        }

        /// <remarks/>
        public byte TINH_TRANG_RV
        {
            get
            {
                return this.tINH_TRANG_RVField;
            }
            set
            {
                this.tINH_TRANG_RVField = value;
            }
        }

        /// <remarks/>
        public string NGAY_TTOAN
        {
            get
            {
                return this.nGAY_TTOANField;
            }
            set
            {
                this.nGAY_TTOANField = value;
            }
        }

        /// <remarks/>
        //public double T_THUOC
        public string T_THUOC
        {
            get
            {
                return this.t_THUOCField;
            }
            set
            {
                this.t_THUOCField = value;
            }
        }

        /// <remarks/>
        //public double T_VTYT
        public string T_VTYT
        {
            get
            {
                return this.t_VTYTField;
            }
            set
            {
                this.t_VTYTField = value;
            }
        }

        /// <remarks/>
        //public double T_TONGCHI
        public string T_TONGCHI
        {
            get
            {
                return this.t_TONGCHIField;
            }
            set
            {
                this.t_TONGCHIField = value;
            }
        }

        /// <remarks/>
        //public double T_BNTT
        public string T_BNTT
        {
            get
            {
                return this.t_BNTTField;
            }
            set
            {
                this.t_BNTTField = value;
            }
        }

        /// <remarks/>
        //public double T_BNCCT
        public string T_BNCCT
        {
            get
            {
                return this.t_BNCCTField;
            }
            set
            {
                this.t_BNCCTField = value;
            }
        }

        /// <remarks/>
        //public double T_BHTT
        public string T_BHTT
        {
            get
            {
                return this.t_BHTTField;
            }
            set
            {
                this.t_BHTTField = value;
            }
        }

        /// <remarks/>
        //public double? T_NGUONKHAC
        public string T_NGUONKHAC
        {
            get
            {
                return this.t_NGUONKHACField;
            }
            set
            {
                this.t_NGUONKHACField = value;
            }
        }

        /// <remarks/>
        public string T_NGOAIDS
        {
            get
            {
                return this.t_NGOAIDSField;
            }
            set
            {
                this.t_NGOAIDSField = value;
            }
        }

        /// <remarks/>
        public int NAM_QT
        {
            get
            {
                return this.nAM_QTField;
            }
            set
            {
                this.nAM_QTField = value;
            }
        }

        /// <remarks/>
        public byte THANG_QT
        {
            get
            {
                return this.tHANG_QTField;
            }
            set
            {
                this.tHANG_QTField = value;
            }
        }

        /// <remarks/>
        public byte MA_LOAI_KCB
        {
            get
            {
                return this.mA_LOAI_KCBField;
            }
            set
            {
                this.mA_LOAI_KCBField = value;
            }
        }

        /// <remarks/>
        public string MA_KHOA
        {
            get
            {
                return this.mA_KHOAField;
            }
            set
            {
                this.mA_KHOAField = value;
            }
        }

        /// <remarks/>
        public string MA_CSKCB
        {
            get
            {
                return this.mA_CSKCBField;
            }
            set
            {
                this.mA_CSKCBField = value;
            }
        }

        /// <remarks/>
        public string MA_KHUVUC
        {
            get
            {
                return this.mA_KHUVUCField;
            }
            set
            {
                this.mA_KHUVUCField = value;
            }
        }

        /// <remarks/>
        public string MA_PTTT_QT
        {
            get
            {
                return this.mA_PTTT_QTField;
            }
            set
            {
                this.mA_PTTT_QTField = value;
            }
        }

        /// <remarks/>
        public string CAN_NANG
        {
            get
            {
                return this.cAN_NANGField;
            }
            set
            {
                this.cAN_NANGField = value;
            }
        }
    }



}
