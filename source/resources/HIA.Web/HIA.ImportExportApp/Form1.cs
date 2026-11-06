using HIA.Core.Models.FileUploads;
using HIA.DatabaseModel.Entites;
using HIA.ImportExportApp.Common;
using HIA.Services.Auth;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HIA.ImportExportApp
{
    public partial class HIA_App : Form
    {
        private Timer timer1;
        private readonly IAuthService _authService;
        public HIA_App(IAuthService authService)
        {
            timer1 = new Timer();
            InitializeComponent();
            _authService = authService;
            InitTimer();
        }

        
        public void InitTimer()
        {
            timer1 = new Timer();
            timer1.Tick += new EventHandler(UploadFile);
            timer1.Interval = 5000; // in miliseconds
            timer1.Start();
        }

        private void UploadFile(object? sender, EventArgs e)
        {
            if (Storages.CurrentUser == null || string.IsNullOrWhiteSpace(Storages.UploadPath)) return;
            if (!Directory.Exists(Storages.UploadPath)) return;
            DirectoryInfo di = new DirectoryInfo(@$"{Storages.UploadPath}");
            var xmlFiles = di.GetFiles("*.xml");
            if (xmlFiles.Length <= 0) return;
            if (Storages.FilesNameHandleProccessing != null && Storages.FilesNameHandleProccessing.Length != 0) return;
            Storages.FilesNameHandleProccessing = xmlFiles;

            foreach (var file in xmlFiles)
            {
                string result = string.Empty;
                using (BinaryReader b = new BinaryReader(File.Open(file.FullName, FileMode.Open)))
                {
                    byte[] binData = b.ReadBytes((int)file.Length);
                    if (GetEncodingHT(binData) == Encoding.UTF8)
                    {
                        result = System.Text.Encoding.Default.GetString(binData);
                    }
                    else if (GetEncodingHT(binData) == Encoding.Unicode)
                    {
                        result = System.Text.Encoding.Unicode.GetString(binData);
                    }
                    else
                    {
                        result = System.Text.Encoding.ASCII.GetString(binData);
                    }
                }
                //try
                //{
                //    var catalog1 = result.ParseXML<GIAMDINHHS>();

                //    foreach (var hosoNho in catalog1.THONGTINHOSO.DANHSACHHOSO)
                //    {



                //        List<GIAMDINHHSTHONGTINHOSOHOSOFILEHOSO> allHoso = hosoNho.ToList();
                //        XML1 newXML1 = new XML1();
                //        foreach (var hosocon in allHoso)
                //        {
                //            string caseSwitch = hosocon.LOAIHOSO;

                //            switch (caseSwitch)
                //            {
                //                case "XML1":
                //                    try
                //                    {
                //                        TONG_HOP xml1 = Encoding.Default.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE)).ParseXML<TONG_HOP>();
                //                        newXML1.HO_TEN = xml1.HO_TEN;
                //                        newXML1.MA_THE = xml1.MA_THE;

                //                        if (xml1.CAN_NANG.ToString().Trim().Length > 0) { newXML1.CAN_NANG = double.Parse(xml1.CAN_NANG); } else { newXML1.CAN_NANG = 0; };
                //                        newXML1.DIA_CHI = xml1.DIA_CHI;
                //                        newXML1.GIOI_TINH = xml1.GIOI_TINH;
                //                        newXML1.GT_THE_DEN = xml1.GT_THE_DEN;
                //                        newXML1.GT_THE_TU = xml1.GT_THE_TU;
                //                        newXML1.KET_QUA_DTRI = xml1.KET_QUA_DTRI;
                //                        newXML1.MA_BENH = xml1.MA_BENH;
                //                        newXML1.MA_BENHKHAC = xml1.MA_BENHKHAC;
                //                        newXML1.MA_BN = xml1.MA_BN;
                //                        newXML1.MA_CSKCB = xml1.MA_CSKCB;
                //                        newXML1.MA_DKBD = xml1.MA_DKBD;
                //                        newXML1.MA_KHOA = xml1.MA_KHOA;
                //                        newXML1.MA_KHUVUC = xml1.MA_KHUVUC;
                //                        newXML1.MA_LK = xml1.MA_LK;
                //                        newXML1.MA_LOAI_KCB = xml1.MA_LOAI_KCB;
                //                        if (xml1.MA_LYDO_VVIEN.ToString().Trim().Length > 0) { newXML1.MA_LYDO_VVIEN = (byte)int.Parse(xml1.MA_LYDO_VVIEN); } else { newXML1.MA_LYDO_VVIEN = 0; }
                //                        newXML1.MA_NOI_CHUYEN = xml1.MA_NOI_CHUYEN;
                //                        newXML1.MA_PTTT_QT = xml1.MA_PTTT_QT;
                //                        if (xml1.MA_TAI_NAN.Length > 0) newXML1.MA_TAI_NAN = (byte)int.Parse(xml1.MA_TAI_NAN);
                //                        newXML1.MIEN_CUNG_CT = xml1.MIEN_CUNG_CT;
                //                        newXML1.NAM_QT = xml1.NAM_QT;
                //                        newXML1.NGAY_RA = DateTime.ParseExact(xml1.NGAY_RA, "yyyyMMddHHmm", null);
                //                        if (xml1.NGAY_SINH.ToString().Length != 4)
                //                        {
                //                            newXML1.NGAY_SINH = DateTime.ParseExact(xml1.NGAY_SINH, "yyyyMMdd", null);
                //                        }
                //                        else { newXML1.NGAY_SINH = DateTime.ParseExact(xml1.NGAY_SINH.ToString() + "0101", "yyyyMMdd", null); }
                //                        newXML1.NGAY_TTOAN = DateTime.ParseExact(xml1.NGAY_TTOAN, "yyyyMMddHHmm", null);
                //                        newXML1.NGAY_VAO = DateTime.ParseExact(xml1.NGAY_VAO, "yyyyMMddHHmm", null);
                //                        newXML1.SO_NGAY_DTRI = xml1.SO_NGAY_DTRI;
                //                        newXML1.STT = xml1.STT;
                //                        newXML1.TEN_BENH = xml1.TEN_BENH;
                //                        newXML1.THANG_QT = xml1.THANG_QT;
                //                        newXML1.TINH_TRANG_RV = xml1.TINH_TRANG_RV;
                //                        newXML1.T_BHTT = xml1.T_BHTT;
                //                        newXML1.T_BNCCT = xml1.T_BNCCT;
                //                        newXML1.T_BNTT = xml1.T_BNTT;
                //                        newXML1.T_VTYT = xml1.T_VTYT;
                //                        newXML1.T_TONGCHI = xml1.T_TONGCHI;
                //                        newXML1.T_THUOC = xml1.T_THUOC;
                //                        newXML1.T_NGUONKHAC = xml1.T_NGUONKHAC;
                //                        if (xml1.T_NGOAIDS.Length > 0) { newXML1.T_NGOAIDS = double.Parse(xml1.T_NGOAIDS); } else { newXML1.T_NGOAIDS = 0; }
                //                        newXML1.FileName = file.Name;
                //                        newXML1.MIEN_CUNG_CT = xml1.MIEN_CUNG_CT;
                //                        newXML1.T_NGUONKHAC = xml1.T_NGUONKHAC;
                //                        newXML1.MA_PTTT_QT = xml1.MA_PTTT_QT;
                //                        newXML1.NGAYLAP = DateTime.Now;
                //                        //success++;
                //                    }
                //                    catch (Exception e)
                //                    {
                //                        //failed++;
                //                        //string errorAt = GetXMLErrorAtRow(Encoding.Default.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE)), e);
                //                        //failedMessage += "XML 1: " + Path.GetFileName(aformFile.FileName) + " Lỗi: " + errorAt + "<br/> ";
                //                    }
                //                    break;
                //                case "XML2":
                //                    try
                //                    {
                //                        DSACH_CHI_TIET_THUOC xml2 = Encoding.Default.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE)).ParseXML<DSACH_CHI_TIET_THUOC>();
                //                        List<XML2> listXML2 = new List<XML2>();
                //                        foreach (DSACH_CHI_TIET_THUOCCHI_TIET_THUOC chitiet in xml2.CHI_TIET_THUOC)
                //                        {
                //                            XML2 xml2temp = new XML2();
                //                            if (newXML1 != null)
                //                            {
                //                                xml2temp.DON_GIA = chitiet.DON_GIA;
                //                                xml2temp.DON_VI_TINH = chitiet.DON_VI_TINH;
                //                                xml2temp.DUONG_DUNG = chitiet.DUONG_DUNG;
                //                                xml2temp.SO_LUONG = chitiet.SO_LUONG;
                //                                xml2temp.MUC_HUONG = (byte)chitiet.MUC_HUONG;
                //                                xml2temp.MA_THUOC = chitiet.MA_THUOC;
                //                                xml2temp.THANH_TIEN = chitiet.THANH_TIEN;
                //                                xml2temp.STT = chitiet.STT;
                //                                xml2temp.PHAM_VI = chitiet.PHAM_VI;
                //                                xml2temp.TEN_THUOC = chitiet.TEN_THUOC;
                //                                xml2temp.TT_THAU = chitiet.TT_THAU;
                //                                xml2temp.TYLE_TT = (byte)chitiet.TYLE_TT;
                //                                xml2temp.T_NGOAIDS = chitiet.T_NGOAIDS;
                //                                xml2temp.T_BHTT = chitiet.T_BHTT;
                //                                xml2temp.T_BNCCT = chitiet.T_BNCCT;
                //                                xml2temp.T_NGUONKHAC = chitiet.T_NGUONKHAC;
                //                                xml2temp.MA_NHOM = chitiet.MA_NHOM;
                //                                xml2temp.T_BHTT = chitiet.T_BHTT;
                //                                if (chitiet.NGAY_YL.Length == 12) xml2temp.NGAY_YL = DateTime.ParseExact(chitiet.NGAY_YL, "yyyyMMddHHmm", null);
                //                                xml2temp.MA_PTTT = chitiet.MA_PTTT;
                //                                xml2temp.LIEU_DUNG = chitiet.LIEU_DUNG;
                //                                xml2temp.SO_DANG_KY = chitiet.SO_DANG_KY;
                //                                xml2temp.MA_KHOA = chitiet.MA_KHOA;
                //                                xml2temp.MA_BAC_SI = chitiet.MA_BAC_SI;
                //                                xml2temp.MA_BENH = chitiet.MA_BENH;
                //                                xml2temp.MA_LK = chitiet.MA_LK;
                //                                xml2temp.HAM_LUONG = chitiet.HAM_LUONG;

                //                            }
                //                            listXML2.Add(xml2temp);
                //                        }
                //                        if (newXML1 != null)
                //                        {
                //                            newXML1.XML2s = listXML2;
                //                        }
                //                    }
                //                    catch (Exception e)
                //                    {
                //                        //failed++;
                //                        //string errorAt = GetXMLErrorAtRow(Encoding.Default.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE)), e);
                //                        //failedMessage += "XML2: " + Path.GetFileName(aformFile.FileName) + " Lỗi: " + errorAt + "<br/> ";// +" "+ e.InnerException.ToString() + "<br/> ";

                //                    }
                //                    break;
                //                case "XML3":
                //                    try
                //                    {
                //                        DSACH_CHI_TIET_DVKT xml3 = Encoding.Default.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE)).ParseXML<DSACH_CHI_TIET_DVKT>();
                //                        List<XML3> listXML3 = new List<XML3>();
                //                        foreach (DSACH_CHI_TIET_DVKTCHI_TIET_DVKT chitiet in xml3.CHI_TIET_DVKT)
                //                        {
                //                            XML3 xml3temp = new XML3();
                //                            if (newXML1 != null)
                //                            {
                //                                xml3temp.DON_GIA = chitiet.DON_GIA;
                //                                xml3temp.DON_VI_TINH = chitiet.DON_VI_TINH;
                //                                xml3temp.GOI_VTYT = chitiet.GOI_VTYT;
                //                                xml3temp.MA_BAC_SI = chitiet.MA_BAC_SI;
                //                                xml3temp.MA_BENH = chitiet.MA_BENH;
                //                                xml3temp.MA_DICH_VU = chitiet.MA_DICH_VU;
                //                                xml3temp.MA_GIUONG = chitiet.MA_GIUONG;
                //                                xml3temp.MA_KHOA = chitiet.MA_KHOA;
                //                                xml3temp.MA_LK = chitiet.MA_LK;
                //                                xml3temp.MA_NHOM = chitiet.MA_NHOM;
                //                                xml3temp.MA_PTTT = chitiet.MA_PTTT;
                //                                xml3temp.MA_VAT_TU = chitiet.MA_VAT_TU;
                //                                xml3temp.MUC_HUONG = (byte)chitiet.MUC_HUONG;
                //                                if (chitiet.NGAY_KQ.Length == 12) xml3temp.NGAY_KQ = DateTime.ParseExact(chitiet.NGAY_KQ, "yyyyMMddHHmm", null);
                //                                if (chitiet.NGAY_YL.Length == 12) xml3temp.NGAY_YL = DateTime.ParseExact(chitiet.NGAY_YL, "yyyyMMddHHmm", null);
                //                                xml3temp.SO_LUONG = chitiet.SO_LUONG;
                //                                xml3temp.STT = chitiet.STT;
                //                                xml3temp.TEN_DICH_VU = chitiet.TEN_DICH_VU;
                //                                xml3temp.TEN_VAT_TU = chitiet.TEN_VAT_TU;
                //                                xml3temp.THANH_TIEN = chitiet.THANH_TIEN;
                //                                xml3temp.TT_THAU = chitiet.TT_THAU;
                //                                xml3temp.TYLE_TT = (byte)chitiet.TYLE_TT;
                //                                xml3temp.T_BHTT = chitiet.T_BHTT;
                //                                xml3temp.T_BNCCT = chitiet.T_BNCCT;
                //                                xml3temp.T_NGOAIDS = chitiet.T_NGOAIDS;
                //                                xml3temp.T_NGUONKHAC = chitiet.T_NGUONKHAC;
                //                                if (chitiet.T_TRANTT.ToString().Trim().Length > 0) { xml3temp.T_TRANTT = Convert.ToDouble(chitiet.T_TRANTT); } else { xml3temp.T_TRANTT = 0; };


                //                            }
                //                            listXML3.Add(xml3temp);
                //                        }
                //                        if (newXML1 != null)
                //                        {
                //                            newXML1.XML3s = listXML3;
                //                        }
                //                    }
                //                    catch (Exception e)
                //                    {
                //                        //failed++;
                //                        //string errorAt = GetXMLErrorAtRow(Encoding.Default.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE)), e);
                //                        //failedMessage += "XML 3: " + Path.GetFileName(aformFile.FileName) + " Lỗi: " + errorAt + "<br/> ";// +" "+ e.InnerException.ToString() + "<br/> ";

                //                    }
                //                    break;
                //                case "XML4":
                //                    try
                //                    {
                //                        DSACH_CHI_TIET_CLS xml4 = Encoding.Default.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE)).ParseXML<DSACH_CHI_TIET_CLS>();
                //                        List<XML4> listXML4 = new List<XML4>();
                //                        foreach (DSACH_CHI_TIET_CLSCHI_TIET_CLS chitiet in xml4.CHI_TIET_CLS)
                //                        {
                //                            XML4 xml4temp = new XML4();
                //                            if (newXML1 != null)
                //                            {
                //                                xml4temp.KET_LUAN = chitiet.KET_LUAN;
                //                                xml4temp.MA_CHI_SO = chitiet.MA_CHI_SO;
                //                                xml4temp.MA_DICH_VU = chitiet.MA_DICH_VU;
                //                                xml4temp.MA_LK = chitiet.MA_LK;
                //                                xml4temp.MA_MAY = chitiet.MA_MAY;
                //                                xml4temp.MO_TA = chitiet.MO_TA;
                //                                if (chitiet.NGAY_KQ?.Length == 12) xml4temp.NGAY_KQ = DateTime.ParseExact(chitiet.NGAY_KQ, "yyyyMMddHHmm", null);
                //                                xml4temp.STT = chitiet.STT;
                //                                xml4temp.TEN_CHI_SO = chitiet.TEN_CHI_SO;
                //                            }
                //                            listXML4.Add(xml4temp);
                //                        }
                //                        if (newXML1 != null)
                //                        {
                //                            newXML1.XML4s = listXML4;
                //                        }
                //                    }
                //                    catch (Exception e)
                //                    {
                //                        //failed++;
                //                        //string errorAt = GetXMLErrorAtRow(Encoding.Default.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE)), e);
                //                        //failedMessage += "XML 4: " + Path.GetFileName(aformFile.FileName) + " Lỗi: " + errorAt + "<br/> ";// +" "+ e.InnerException.ToString() + "<br/> ";

                //                    }
                //                    break;
                //                case "XML5":
                //                    try
                //                    {

                //                        DSACH_CHI_TIET_DIEN_BIEN_BENH xml5 = Encoding.Default.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE)).ParseXML<DSACH_CHI_TIET_DIEN_BIEN_BENH>();
                //                        List<XML5> listXML5 = new List<XML5>();
                //                        foreach (DSACH_CHI_TIET_DIEN_BIEN_BENHCHI_TIET_DIEN_BIEN_BENH chitiet in xml5.CHI_TIET_DIEN_BIEN_BENH)
                //                        {
                //                            XML5 xml5temp = new XML5();
                //                            if (newXML1 != null)
                //                            {
                //                                xml5temp.MA_LK = chitiet.MA_LK;
                //                                if (chitiet.NGAY_YL?.Length == 12) xml5temp.NGAY_YL = DateTime.ParseExact(chitiet.NGAY_YL, "yyyyMMddHHmm", null);
                //                                xml5temp.PHAU_THUAT = chitiet.PHAU_THUAT;
                //                                xml5temp.STT = chitiet.STT;
                //                                xml5temp.HOI_CHAN = chitiet.HOI_CHAN;
                //                                xml5temp.DIEN_BIEN = chitiet.DIEN_BIEN;




                //                            }
                //                            listXML5.Add(xml5temp);
                //                        }
                //                        if (newXML1 != null)
                //                        {
                //                            newXML1.XML5s = listXML5;
                //                        }
                //                    }
                //                    catch (Exception e)
                //                    {
                //                        //failed++;
                //                        //string errorAt = GetXMLErrorAtRow(Encoding.Default.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE)), e);
                //                        //failedMessage += "XML 5: " + Path.GetFileName(aformFile.FileName) + " Lỗi: " + errorAt + "<br/> ";// +" "+ e.InnerException.ToString() + "<br/> ";

                //                    }
                //                    break;
                //            }


                //        }

                //        if (newXML1 != null && newXML1.HO_TEN != null)
                //        {
                //            finalXML1List.Add(newXML1);
                //            //await _hosoService.CreateNewHoSo(newXML1);
                //            successMessage += Path.GetFileName(aformFile.FileName) + "<br/>";
                //        }
                //    }
                //}
                //catch
                //{
                //    //failed++;
                //    //string errorAt = aformFile.FileName;
                //    //failedMessage += "Lỗi đọc file tổng:" + Path.GetFileName(aformFile.FileName) + " Lỗi: " + errorAt + "<br/> ";// +" "+ e.InnerException.ToString() + "<br/> ";
                //}
            }
        }
        private static Encoding GetEncodingHT(byte[] bom)
        {
            // Analyze the BOM

            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe && bom[2] == 0 && bom[3] == 0) return Encoding.UTF32; //UTF-32LE
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return new UTF32Encoding(true, true);  //UTF-32BE

            // We actually have no idea what the encoding is if we reach this point, so
            // you may wish to return null instead of defaulting to ASCII
            return Encoding.ASCII;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            var currentUser = Storages.CurrentUser;
            if (currentUser == null)
            {
                var loginForm = new LoginForm(_authService);
                loginForm.ShowDialog();
            } 
            else
            {
                var userInfo = new UserInfo();
                userInfo.PatchValueToForm();
                userInfo.ShowDialog();
            }
        }

        private void btnFolder_Click(object sender, EventArgs e)
        {
            var currentUser = Storages.CurrentUser;
            if (currentUser == null)
            {
                var loginForm = new LoginForm(_authService);
                loginForm.ShowDialog();
            }
            else
            {
                var pathConfig = new PathConfig();
                pathConfig.PatchValueToPathConfig();
                pathConfig.ShowDialog();
            }
        }
    }
}
