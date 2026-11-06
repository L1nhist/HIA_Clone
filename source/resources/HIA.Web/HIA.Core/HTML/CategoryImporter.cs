using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Globalization;
using HIA.Core.Base;

namespace HIA.Core.HTML
{
    public class CategoryImporter
    {
        #region Static
        private const string URL_THUOC = @"https://gdbhyt.baohiemxahoi.gov.vn/DanhMuc_ThuocBV_QTM/AdvancedCustomBindingPagingAction";
        private const string URL_DICHVU = @"https://gdbhyt.baohiemxahoi.gov.vn/DanhMuc_DichVuBV_QTM/AdvancedCustomBindingPagingAction";
        private const string URL_VATTU = @"https://gdbhyt.baohiemxahoi.gov.vn/DanhMuc_VatTuBV_QTM/AdvancedCustomBindingPagingAction";
        private const string URL_BACSI = @"https://gdbhyt.baohiemxahoi.gov.vn/DM_NHANVIENYTE_QTM/AdvancedCustomBindingPagingAction";
        private const string URL_KHOA = @"https://gdbhyt.baohiemxahoi.gov.vn/DM_KHOA/DM_KHOAPartial";

        private static string[][] Field_DMThuoc;
        private static string[][] Field_DMDichVu;
        private static string[][] Field_DMVatTu;
        private static string[][] Field_DMBacSi;
        private static string[][] Field_DMKhoa;

        static CategoryImporter()
        {
            Field_DMThuoc = new string[][]
            {
                "Mã HC AX,Hoạt chất AX,Mã đường dùng AX,Đường dùng AX,Hàm lượng,Tên thuốc AX,Số ĐK ánh xạ,Đơn vị tính,Giá bán,Số lượng,Nước SX,Tên nhà thầu,Quyết định,Loại thuốc,Loại thầu,Nhóm thầu,MN 9324,HL,Kết quả giám định,Lý do từ chối,Mã nhóm 5937,Dạng bào chế,Giá bảo hiểm,Quy cách,Nhà SX,TT Thầu,Từ ngày,Đến ngày,Hình thức đấu thầu,Mã DVKT,Tiêu chuẩn chất lượng,Bộ phận sử dụng,Tên khoa học,Nguồn gốc,PP Chế biến,Mã DL Nhập,Mã DL chế biến,Tỉ lệ hư hao chế biến,Tỉ lệ hư hao sử dụng,Ngày đề nghị".Split(','),
                "MA_THUOC,TEN_HOAT_CHAT,MA_DUONG_DUNG,DUONG_DUNG,HAM_LUONG,TEN_THUOC,SO_DANG_KY,DON_VI_TINH,DON_GIA,SO_LUONG,NUOC_SX,NHA_THAU,QUYET_DINH,LOAI_THUOC,LOAI_THAU,NHOM_THAU,MANHOM_9324,HIEULUC,KETQUA,LYDOTUCHOI,MANHOM_5937,DANG_BAO_CHE,DON_GIA_BH,QUY_CACH,NHA_SX,TT_THAU,TU_NGAY,DEN_NGAY,HT_THAU,MA_DVKT,TCCL,BO_PHAN_VT,TEN_KHOA_HOC,NGUON_GOC,PP_CHEBIEN,MA_DL_NHAP,MA_DL_CB,TLHH_CB,TLHH_BQ,NGAY_DE_NGHI".Split(',')
            };
            Field_DMDichVu = new string[][]
            {
                "Mã ánh xạ,Tên AX,Loại giá,Giá,Giá AX,QĐ,MN 9324,HL,Kết quả giám định,Lý do từ chối,Mã nhóm 5937,CS KCB chuyển giao,CS KCB thực hiện CLS,Từ ngày,Đến ngày,Ngày đề nghị".Split(','),
                "MA_DICH_VU,TEN_DICH_VU,MA_GIA,DON_GIA,GIA_AX,QUYET_DINH,MANHOM_9324,HIEULUC,KETQUA,LYDOTUCHOI,MANHOM_5937,CSKCB_CGKT,CSKCB_CLS,TU_NGAY,DEN_NGAY,NGAY_DE_NGHI".Split(','),
            };
            Field_DMVatTu = new string[][]
            {
                "Mã nhóm AX,Tên nhóm AX,Mã Hiệu,Mã VTYT BV,Quy cách,Nước SX,Nhà SX,ĐVT,Giá BHYT,Nhà thầu,Quyết định,Định mức,Số lượng,Loại thầu,MN 9324,HL,Kết quả giám định,Lý do từ chối,Mã nhóm 5937,Tên VTYT BV,Giá,Tỷ lệ,Thông tin thầu,Từ ngày,Đến ngày,Hình thức đấu thầu,Ngày đề nghị".Split(','),
                "MA_VAT_TU,NHOM_VAT_TU,MA_HIEU,MA_VTYT_BV,QUY_CACH,NUOC_SX,NHA_SX,DON_VI_TINH,DON_GIA_TT,NHA_THAU,QUYET_DINH,DINH_MUC,SO_LUONG,LOAI_THAU,MANHOM_9324,HIEULUC,KETQUA,LYDOTUCHOI,MANHOM_5937,TEN_VAT_TU,DON_GIA,TYLE_TT_BH,TT_THAU,TU_NGAY,DEN_NGAY,HT_THAU,NGAY_DE_NGHI".Split(','),
            };
            Field_DMBacSi = new string[][]
            {
                "Mã khoa,Giới tính,Mã CCHN,Ngày cấp CCHN,Nơi cấp CCHN,Từ ngày,Đến ngày,Thời gian đăng ký,Thời gian ngày,Thời gian tuần,Mã loại KCB,Tên khoa,Mã số BHXH,Tên nhân viên,Chức Danh,Vị trí,Phạm vi chuyên môn,Phạm vi chuyên môn bổ sung,Mã DVKT khác,Văn bản phân công,CSKCB khác,CSKCB chuyển giao kĩ thuật,Quyết định chuyển giao kĩ thuật".Split(','),
                "MA_KHOA,GIOI_TINH,MA_CCHN,NGAYCAP_CCHN,NOICAP_CCHN,TU_NGAY,DEN_NGAY,THOIGIAN_DK,THOIGIAN_NGAY,THOIGIAN_TUAN,MA_LOAI_KCB,TEN_KHOA,MA_BHXH,HO_TEN,CHUC_DANH,VI_TRI,PHAMVI_CM,PHAMVI_CMBS,DVKT_KHAC,VB_PHANCONG,CSKCB_KHAC,CSKCB_CGKT,QD_CGKT".Split(',')
            };
            Field_DMKhoa = new string[][]
            {
                "Mã,Tên,Từ ngày,Đến ngày,HL".Split(','),
                "MA_KHOA,TEN_KHOA,TU_NGAY,DEN_NGAY,HIEULUC".Split (','),
            };
        }
        #endregion

        private readonly string _username;
        private readonly string _password;
        private CookieCollection _authCookies = null;

        public CategoryImporter(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public async Task<bool> Login()
        {
            try
            {
                _authCookies = null;
                var cookies = new CookieContainer();
                var handler = new HttpClientHandler
                {
                    CookieContainer = cookies
                };

                var client = new HttpClient(handler);
                var uri = new Uri(@"https://gdbhyt.baohiemxahoi.gov.vn/Account/login");
                var prms = new List<KeyValuePair<string, string>>()
                {
                    new("username", _username),
                    new("password", _password),
                    new("captcha", ""),
                };

                var req = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = uri,
                    Content = new FormUrlEncodedContent(prms),
                };

                var resp = await client.SendAsync(req);
                var content = await resp.Content.ReadAsStringAsync();
                if (content.Replace(" ", "") == "{\"data\":1}")
                {
                    _authCookies = cookies.GetCookies(uri);
                    return true;
                }
            }
            catch
            { }

            return false;
        }

        private string nextPage = "";
        private string nextState = "";
        private DateTime DateInRange = new(2017, 1, 1);

        public async Task<string> GetPage(string path, List<KeyValuePair<string, string>> query)
        {
            if (_authCookies == null && !(await Login() || _authCookies == null)) return null;

            var cookies = new CookieContainer();
            cookies.Add(_authCookies);
            var handler = new HttpClientHandler
            {
                CookieContainer = cookies
            };

            var client = new HttpClient(handler);
            var uri = new Uri(path);
            var req = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = uri,
                Content = new FormUrlEncodedContent(query),
            };

            var resp = await client.SendAsync(req);
            var cont = await resp.Content.ReadAsStringAsync();
            return cont;
        }

        private void SetNext(string cont)
        {
            string s1 = "/*DX*/({'id':0,'result':{'stateObject':";
            string s2 = ",'html':'<%html%>'}})/*DXHTML*/";
            var idx1 = cont.IndexOf(s1);
            if (idx1 != -1)
            {
                var idx2 = cont.IndexOf(s2, idx1 + s1.Length);
                if (idx2 != -1)
                {
                    nextState = cont.Substring(idx1 + s1.Length, idx2 - idx1 - s1.Length).Replace("'", "\"");

                    int idx3 = nextState.IndexOf("[");
                    nextPage = nextState.Substring(idx3, nextState.IndexOf("]") - idx3 + 1);
                    nextState = nextState.Replace("\"", "&quot;");
                }
            }
        }

        private DataTable SetCap(DataTable dt, DOMCollection cells, string[][] fields)
        {
            foreach (var cel in cells)
            {
                var td = cel.Children.QuerySelector("td");
                if (td != null && td.Count > 0)
                {
                    string capt = Uti.ReplaceValuePairs(td[0].InnerText(), fields);
                    dt.Columns.Add(new DataColumn(capt));
                }

            }
            return dt;
        }

        private string ParseGroup(string group)
        {
            if (string.IsNullOrEmpty(group)) return "";

            int i = group.IndexOf("-");
            if (i < 1) return "";

            return int.TryParse(group[0..i], out i) ? i.ToString() : "";
        }

        #region DMThuoc
        public async Task<string> DMThuoc_GetFirstPage()
        {
            var prms = new List<KeyValuePair<string, string>>()
            {
                new("DXCallbackName", "gvDanhmuc_ThuocBV"),
                new("DXCallbackArgument", "c0:KV|166;[\"72242102\",\"72231954\",\"72181363\",\"72181362\",\"72181361\",\"72143484\",\"72143363\",\"72124965\",\"72124964\",\"72106324\",\"72097101\",\"72097100\",\"72078634\",\"72078633\",\"72073919\"];CR|2;{};GB|23;12|PAGERONCLICK6|PSP500;"),
                new("gvDanhmuc_ThuocBV", "{&quot;keys&quot;:[&quot;72242102&quot;,&quot;72231954&quot;,&quot;72181363&quot;,&quot;72181362&quot;,&quot;72181361&quot;,&quot;72143484&quot;,&quot;72143363&quot;,&quot;72124965&quot;,&quot;72124964&quot;,&quot;72106324&quot;,&quot;72097101&quot;,&quot;72097100&quot;,&quot;72078634&quot;,&quot;72078633&quot;,&quot;72073919&quot;],&quot;resizingState&quot;:&quot;{}&quot;,&quot;callbackState&quot;:&quot;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&quot;,&quot;scrollState&quot;:&quot;-1;0&quot;,&quot;selection&quot;:&quot;&quot;,&quot;customOperationState&quot;:&quot;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YmxpY0tleVRva2VuPWI3N2E1YzU2MTkzNGUwODkHAAcABwMHAgIHSFRfVEhBVQcABv//Bv//ByUAWlN5c3RlbS5PYmplY3QsIG1zY29ybGliLCBWZXJzaW9uPTQuMC4wLjAsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49Yjc3YTVjNTYxOTM0ZTA4OQcABwAHAwcCAgdNQV9EVktUBwAG//8G//8HJgBaU3lzdGVtLk9iamVjdCwgbXNjb3JsaWIsIFZlcnNpb249NC4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1iNzdhNWM1NjE5MzRlMDg5BwAHAAcDBwICB0RWX1RIQVUHAAb//wb//wcnAFpTeXN0ZW0uT2JqZWN0LCBtc2NvcmxpYiwgVmVyc2lvbj00LjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWI3N2E1YzU2MTkzNGUwODkHAAcABwMHAgIKQk9fUEhBTl9WVAcABv//Bv//BygAWlN5c3RlbS5PYmplY3QsIG1zY29ybGliLCBWZXJzaW9uPTQuMC4wLjAsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49Yjc3YTVjNTYxOTM0ZTA4OQcABwAHAwcCAgxURU5fS0hPQV9IT0MHAAb//wb//wcpAFpTeXN0ZW0uT2JqZWN0LCBtc2NvcmxpYiwgVmVyc2lvbj00LjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWI3N2E1YzU2MTkzNGUwODkHAAcABwMHAgIJTkdVT05fR09DBwAG//8G//8HKgBaU3lzdGVtLk9iamVjdCwgbXNjb3JsaWIsIFZlcnNpb249NC4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1iNzdhNWM1NjE5MzRlMDg5BwAHAAcDBwICB0RMX05IQVAHAAb//wb//wcrAFpTeXN0ZW0uT2JqZWN0LCBtc2NvcmxpYiwgVmVyc2lvbj00LjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWI3N2E1YzU2MTkzNGUwODkHAAcABwMHAgIHVExISF9DQgcABv//Bv//BywAWlN5c3RlbS5PYmplY3QsIG1zY29ybGliLCBWZXJzaW9uPTQuMC4wLjAsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49Yjc3YTVjNTYxOTM0ZTA4OQcABwAHBQcCAgdUTEhIX0JRBwAG//8G//8HLQBaU3lzdGVtLk9iamVjdCwgbXNjb3JsaWIsIFZlcnNpb249NC4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1iNzdhNWM1NjE5MzRlMDg5BwAHAAcFBwICBFRDQ0wHAAb//wb//wcuAFpTeXN0ZW0uT2JqZWN0LCBtc2NvcmxpYiwgVmVyc2lvbj00LjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWI3N2E1YzU2MTkzNGUwODkHAAcABwAHAgIKUFBfQ0hFQklFTgcABv//Bv//By8AWlN5c3RlbS5PYmplY3QsIG1zY29ybGliLCBWZXJzaW9uPTQuMC4wLjAsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49Yjc3YTVjNTYxOTM0ZTA4OQcABwAHAAcCAgpNQV9ETF9OSEFQBwAG//8G//8HMABaU3lzdGVtLk9iamVjdCwgbXNjb3JsaWIsIFZlcnNpb249NC4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1iNzdhNWM1NjE5MzRlMDg5BwAHAAcABwICCE1BX0RMX0NCBwAG//8G//8HMQBaU3lzdGVtLk9iamVjdCwgbXNjb3JsaWIsIFZlcnNpb249NC4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1iNzdhNWM1NjE5MzRlMDg5BwAHAAcABwICB1RUX1RIQVUHAAb//wb//wcyAFpTeXN0ZW0uT2JqZWN0LCBtc2NvcmxpYiwgVmVyc2lvbj00LjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWI3N2E1YzU2MTkzNGUwODkHAAcABwAHAgIHSElFVUxVQwcABv//Bv//BzMKBwAHAwcDBwICCE5HQVlOSEFOBwAG//8G//8HNAgHAAcBBwAHAgIHTkdBWUdVSQcABv//Bv//BzUEBwAHAQcABwICDFRJTkhUUkFOR0dVSQcABv//Bv//BzYDBwAHAgcABwICDktFVFFVQUdJQU1ESU5IBwAG//8G//8HNwBaU3lzdGVtLk9iamVjdCwgbXNjb3JsaWIsIFZlcnNpb249NC4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1iNzdhNWM1NjE5MzRlMDg5BwAHAgcABwICD1NVQV9NQU5IT01fOTMyNAcABv//Bv//BzgAWlN5c3RlbS5PYmplY3QsIG1zY29ybGliLCBWZXJzaW9uPTQuMC4wLjAsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49Yjc3YTVjNTYxOTM0ZTA4OQcABwAHAAcCAgNTVUEHAAb//wb//wc5AFpTeXN0ZW0uT2JqZWN0LCBtc2NvcmxpYiwgVmVyc2lvbj00LjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWI3N2E1YzU2MTkzNGUwODkHAAcABwAHAgIDWE9BBwAG//8G//8HOgBaU3lzdGVtLk9iamVjdCwgbXNjb3JsaWIsIFZlcnNpb249NC4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1iNzdhNWM1NjE5MzRlMDg5BwAHAAcABwICBktFVFFVQQcABv//Bv//BzsDBwAHAAcABwICBlJFU1VMVAcABv//Bv//BzwHBwAHAAcABwICCkRBR0lBTURJTkgHAAb//wb//wc9CgcABwAHAAcCBwAHDwIBBhMBAgECAAIABwUHFAcyB2QHyAb0AQcABwACAklEAgRCd0E9AgACAQACASoHAA==&quot;}"),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol5", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol6", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol7", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol8", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol9", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol10", ""),
                //new("gvDanhmuc_ThuocBV_DXFREditorcol11_VI", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol11", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol11$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol11$DDD$L$State", "{&quot;CustomCallback&quot;:&quot;&quot;}"),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol11$DDD$L", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol12", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol13", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol14", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol15", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol16", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol17", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol18", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol19", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol20", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol21", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol22", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol23", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol24", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol25", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol26", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol27", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol28", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol29", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol30$State", "{&quot;rawValue&quot;:&quot;N&quot;}"),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol30", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol30$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol30$DDD$C", "{&quot;visibleDate&quot;:&quot;12/25/2023&quot;}"),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol30$DDD$C$FNPState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol31$State", "{&quot;rawValue&quot;:&quot;N&quot;}"),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol31", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol31$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol31$DDD$C", "{&quot;visibleDate&quot;:&quot;12/25/2023&quot;}"),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol31$DDD$C$FNPState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvDanhmuc_ThuocBV_DXFREditorcol32_VI", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol32", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol32$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol32$DDD$L$State", "{&quot;CustomCallback&quot;:&quot;&quot;}"),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol32$DDD$L", ""),
                //new("gvDanhmuc_ThuocBV_DXFREditorcol33_VI", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol33", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol33$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol33$DDD$L$State", "{&quot;CustomCallback&quot;:&quot;&quot;}"),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol33$DDD$L", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol34", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol35", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol36", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol37", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol38", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol39", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol40", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol41", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol42", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol43", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol44", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol45", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol46", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol47", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol48", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol49", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol50", ""),
                //new("gvDanhmuc_ThuocBV_DXFREditorcol51_VI", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol51", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol51$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol51$DDD$L$State", "{&quot;CustomCallback&quot;:&quot;&quot;}"),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol51$DDD$L", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol52$State", "{&quot;rawValue&quot;:&quot;N&quot;}"),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol52", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol52$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol52$DDD$C", "{&quot;visibleDate&quot;:&quot;12/25/2023&quot;}"),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol52$DDD$C$FNPState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol53$State", "{&quot;rawValue&quot;:&quot;N&quot;}"),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol53", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol53$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol53$DDD$C", "{&quot;visibleDate&quot;:&quot;12/25/2023&quot;}"),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol53$DDD$C$FNPState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvDanhmuc_ThuocBV_DXFREditorcol54_VI", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol54", ""),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol54$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol54$DDD$L$State", "{&quot;CustomCallback&quot;:&quot;&quot;}"),
                //new("gvDanhmuc_ThuocBV$DXFREditorcol54$DDD$L", ""),
                //new("gvDanhmuc_ThuocBV$custwindowState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvDanhmuc_ThuocBV$DXFilterRowMenu", "{&quot;selectedItemIndexPath&quot;:&quot;&quot;,&quot;checkedState&quot;:&quot;&quot;}"),
                //new("gvDanhmuc_ThuocBV$DXPagerBottom$PSP", "{&quot;selectedItemIndexPath&quot;:&quot;4&quot;,&quot;checkedState&quot;:&quot;&quot;}"),
                //new("DXMVCEditorsValues", "{\"gvDanhmuc_ThuocBV_DXFREditorcol5\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol6\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol7\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol8\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol9\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol10\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol11_DDD_L\":[],\"gvDanhmuc_ThuocBV_DXFREditorcol11\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol12\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol13\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol14\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol15\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol16\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol17\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol18\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol19\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol20\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol21\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol22\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol23\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol24\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol25\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol26\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol27\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol28\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol29\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol30_DDD_C\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol30\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol31_DDD_C\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol31\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol32_DDD_L\":[],\"gvDanhmuc_ThuocBV_DXFREditorcol32\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol33_DDD_L\":[],\"gvDanhmuc_ThuocBV_DXFREditorcol33\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol34\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol35\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol36\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol37\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol38\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol39\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol40\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol41\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol42\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol43\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol44\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol45\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol46\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol47\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol48\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol49\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol50\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol51_DDD_L\":[],\"gvDanhmuc_ThuocBV_DXFREditorcol51\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol52_DDD_C\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol52\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol53_DDD_C\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol53\":null,\"gvDanhmuc_ThuocBV_DXFREditorcol54_DDD_L\":[],\"gvDanhmuc_ThuocBV_DXFREditorcol54\":null}"),
                //new("strTimKiem", ""),
                //new("loaithuoc", ""),
                //new("loaithau", ""),
                //new("trangthaihs", ""),
                //new("trangthaiax", ""),
                //new("trangthaigui", "2"),
                //new("ketqua", "1"),
                //new("tungay", "Sun+Jan+01+2017+00:00:00+GMT+0700+(Indochina+Time)"),
                //new("denngay", $"{Field_MonDate[0][(int)DateTime.Now.DayOfWeek]}+{Field_MonDate[1][DateTime.Now.Month - 1]}+{DateTime.Now:dd+yyyy}+00:00:00+GMT+0700+(Indochina+Time)"), //"Mon+Dec+25+2023+00:00:00+GMT+0700+(Indochina+Time)"),
            };

            try
            {
                var cont = await GetPage(URL_THUOC, prms);
                SetNext(cont);
                return cont;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> DMThuoc_GetNextPage(int page)
        {
            var prms = new List<KeyValuePair<string, string>>()
            {
                new("DXCallbackName", "gvDanhmuc_ThuocBV"),
                new("DXCallbackArgument", $"c0:KV|5501;{nextPage};CR|2;{{}};GB|2{(page > 9 ? 1 : 0)};12|PAGERONCLICK{(page > 9 ? 4 : 3)}|PN{page};"),
                new("gvDanhmuc_ThuocBV", nextState),
            };

            try
            {
                var cont = await GetPage(URL_THUOC, prms);
                SetNext(cont);
                return cont;
            }
            catch { }

            return null;
        }

        public DataTable DMThuoc_PeakTableCaption(string content)
        {
            int idx1 = content.IndexOf("<table id=\"gvDanhmuc_ThuocBV_DXHeaderTable\"");
            int idx2 = content.IndexOf("</table><script", idx1);
            if (idx1 < 0 || idx2 < 0) return null;

            string html = content.Substring(idx1, idx2 - idx1 + 8);
            HTMLDocument doc = HTMLDocument.Parse(html);

            DOMCollection all_head_cells = doc.Body.QuerySelector(".dxgvHeader_EIS");
            return (all_head_cells == null || all_head_cells.Count < 1) ? null : SetCap(new("DMThuoc"), all_head_cells, Field_DMThuoc);
        }

        public bool DMThuoc_PeakContent(string content, DataTable dt)
        {
            if (dt == null) return false;

            int idx1 = content.IndexOf("<table id=\"gvDanhmuc_ThuocBV_DXMainTable\"");
            int idx2 = content.IndexOf("</table>", idx1);
            if (idx1 < 0 || idx2 < 0) return false;

            string html = content.Substring(idx1, idx2 - idx1 + 8);
            HTMLDocument doc = HTMLDocument.Parse(html);

            var all_item_rows = doc.Body.QuerySelector(".dxgvDataRow_EIS");
            if (all_item_rows?.Count < 1) return false;

            foreach (var row in all_item_rows)
            {
                var cells = row.QuerySelector("td");
                if (cells?.Count < 1) continue;

                //Bo qua chua phe duyet
                string s = cells[54].InnerText().Trim();
                if (!s.Contains("Đã phê duyệt")) continue;

                //Bo qua ngay truoc 01/01/2027
                string dte = cells[51].InnerText().Trim();
                if (DateTime.TryParseExact(dte, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dd))
                {
                    if (dd < DateInRange) continue;
                }
                else continue;

                idx1 = -1;
                List<string> values = new();
                foreach (var cel in cells)
                {
                    ++idx1;
                    if (idx1 == 32)
                    {
                        values.Add(ParseGroup(cel.InnerText().Trim()));
                    }
                    else if (idx1 == 50)
                    {
                        values.Add(cel.QuerySelector(".dxWeb_edtCheckBoxChecked_EIS") != null ? "Có" : "");
                    }
                    else if (idx1 == 51)
                    {
                        values.Add(dte);
                    }
                    else if (idx1 == 54)
                    {
                        values.Add("1");
                    }
                    else if (idx1 < 55)
                    {
                        values.Add(cel.InnerText().Trim());
                    }
                }
                dt.Rows.Add(values.ToArray());
            }

            return true;
        }

        public int DMThuoc_PeakPageCount(string content)
        {
            int idx1 = content.IndexOf("<div class=\"dxpLite_EIS\" id=\"gvDanhmuc_ThuocBV_DXPagerBottom\">");
            if (idx1 < 0) return -1;

            idx1 = content.IndexOf("<b class=\"dxp-lead dxp-summary\">Page 1 of ", idx1);
            if (idx1 < 0) return -1;

            idx1 = content.IndexOf(" (", idx1);
            int idx2 = content.IndexOf(" items)", idx1);
            if (idx2 < 0) return -1;

            string s = content.Substring(idx1 + 2, idx2 - idx1 - 2).Trim();
            if (int.TryParse(s, out int res))
            {
                return res / 500 + (res % 500 > 0 ? 1 : 0);
            }
            return -1;
        }
        #endregion

        #region DMDichVu
        public async Task<string> DMDichVu_GetFirstPage()
        {
            var prms = new List<KeyValuePair<string, string>>()
            {
                new("DXCallbackName", "gvEditing"),
                new("DXCallbackArgument", "c0:KV|221;[\"72359714\",\"72357887\",\"72355623\",\"72354318\",\"72352464\",\"72351355\",\"72350278\",\"72349104\",\"72348273\",\"72343807\",\"72343300\",\"72342463\",\"72340815\",\"72338333\",\"72337644\",\"72336713\",\"72335798\",\"72335087\",\"72333894\",\"72332918\"];CR|2;{};GB|23;12|PAGERONCLICK6|PSP500;"),
                new("gvEditing", "{&quot;keys&quot;:[&quot;72359714&quot;,&quot;72357887&quot;,&quot;72355623&quot;,&quot;72354318&quot;,&quot;72352464&quot;,&quot;72351355&quot;,&quot;72350278&quot;,&quot;72349104&quot;,&quot;72348273&quot;,&quot;72343807&quot;,&quot;72343300&quot;,&quot;72342463&quot;,&quot;72340815&quot;,&quot;72338333&quot;,&quot;72337644&quot;,&quot;72336713&quot;,&quot;72335798&quot;,&quot;72335087&quot;,&quot;72333894&quot;,&quot;72332918&quot;],&quot;resizingState&quot;:&quot;{}&quot;,&quot;callbackState&quot;:&quot;BwUHAQIFU3RhdGUGcgMHJQcAAgEHAQIBBv//AgAHAgIBBwMCAQcEAgEG//8CAAcFAgEHBgIBBv//AgAHBwIBBwgCAQcJAgEHCgIBBwsCAQcMAgEHDQIBBw4CAQcPAgEHEAIBBxECAQcSAgEHEwIBBxQCAQcVAgEHFgIBBxcCAQcYAgEHGQIBBxoCAQcbAgEHHAIBBx0CAQb//wIABv//AgAG//8CAAceAgEHAQcCBwIHAAcABx8HAAcBAgAAAAAAAERABwEHAQIAAAAAAABOQAcDBwECAAAAAAAASUAHBAcBAgAAAAAAAElABwUHAQIAAAAAAABZQAcHBwECAAAAAAAAWUAHCAcBAgAAAAAA4HVABwoHAQIAAAAAAOB1QAcLBwECAAAAAACAVkAHDAcBAgAAAAAAgFZABw0HAQIAAAAAAABpQAcOBwECAAAAAAAAWUAHDwcBAgAAAAAAAFlABxAHAQIAAAAAAABZQAcRBwECAAAAAAAAWUAHEgcBAgAAAAAAAE5ABxMHAQIAAAAAAMBiQAcUBwECAAAAAAAAWUAHFQcBAgAAAAAAwGJABxYHAQIAAAAAAMBiQAcXBwECAAAAAADAYkAHGAcBAgAAAAAAAElABxkHAQIAAAAAAABJQAcaBwECAAAAAADAbEAHGwcBAgAAAAAAwGJABxwHAQIAAAAAAMBiQAcdBwECAAAAAADAYkAHHgcBAgAAAAAAwGJABx8HAQIAAAAAAIBLQAcgBwECAAAAAACAS0AHJAcBAgAAAAAAgFZAByQHAQcABwIHAAcDBwAHBAcABwUHAAcGBwAHBwcABwgHAAcJBwAHCgcABwsHBQcMBwUHDQcABw4HAAcPBwAHEAcABxEHAAcSBwAHEwcABxQHAAcVBwAHFgcABxcHAAcYBwAHGQcABxoHAAcbBwAHHAcABx0HAAceBwAHHwcAByAHAAchBwAHIgcAByMHAAckBwACAAUAAACACQICSUQHAQICSUQECQIAAgADBwACAQaqIAcAAgEAc0RldkV4cHJlc3MuV2ViLkNoZWNrU3RhdGUsIERldkV4cHJlc3MuV2ViLnYxNS4xLCBWZXJzaW9uPTE1LjEuOC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWI4OGQxNzU0ZDcwMGU0OWEHAQIBBwAHAAcABwACDVNob3dGaWx0ZXJSb3cKAgECEVNob3dGaWx0ZXJSb3dNZW51CgIBAghQYWdlU2l6ZQMHFAIRU2VhcmNoUGFuZWxGaWx0ZXIJ&quot;,&quot;scrollState&quot;:&quot;-1;0&quot;,&quot;selection&quot;:&quot;&quot;,&quot;customOperationState&quot;:&quot;ByQCAAcABv//Bv//BwEAWlN5c3RlbS5PYmplY3QsIG1zY29ybGliLCBWZXJzaW9uPTQuMC4wLjAsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49Yjc3YTVjNTYxOTM0ZTA4OQcABwAHAAcCAgJJRAcCBwAG//8HAgcHAAcABwAHAgICVFQHAAb//wb//wcDAFpTeXN0ZW0uT2JqZWN0LCBtc2NvcmxpYiwgVmVyc2lvbj00LjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWI3N2E1YzU2MTkzNGUwODkHAAcCBwAHAgICQVgHAAb//wb//wcEAFpTeXN0ZW0uT2JqZWN0LCBtc2NvcmxpYiwgVmVyc2lvbj00LjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWI3N2E1YzU2MTkzNGUwODkHAAcCBwAHAgICTUEHAAb//wb//wcFBwcABwAHAAcCAgVNQV9HWQcABv//Bv//BwYAWlN5c3RlbS5PYmplY3QsIG1zY29ybGliLCBWZXJzaW9uPTQuMC4wLjAsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49Yjc3YTVjNTYxOTM0ZTA4OQcABwAHAAcCAgVNQV9BWAcABv//Bv//BwcHBwAHAAcABwICA1RFTgcABv//Bv//BwgHBwAHAAcABwICBlRFTl9HWQcABv//Bv//BwkAWlN5c3RlbS5PYmplY3QsIG1zY29ybGliLCBWZXJzaW9uPTQuMC4wLjAsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49Yjc3YTVjNTYxOTM0ZTA4OQcABwAHAAcCAgZURU5fQVgHAAb//wb//wcKBwcABwAHAAcCAgNHSUEHAAb//wb//wcLBQcABwAHBQcCAgZHSUFfQVgHAAb//wb//wcMBQcABwAHBQcCAg9CQU5HR0lBX0RWS1RfSUQHAAb//wb//wcNBAcABwAHAAcCAgtTT1FVWUVURElOSAcABv//Bv//Bw4HBwAHAAcABwICCk5HQVlCQVREQVUHAAb//wb//wcPCAcABwEHAAcCAgZUVU5HQVkHAAb//wb//wcQCAcABwEHAAcCAgdERU5OR0FZBwAG//8G//8HEQgHAAcBBwAHAgIGTUFUSU5IBwAG//8G//8HEgcHAAcABwAHAgILU1RUUEhFRFVZRVQHAAb//wb//wcTBwcABwAHAAcCAgtNQU5IT01fOTMyNAcABv//Bv//BxQHBwAHAgcABwICC01BTkhPTV81OTM3BwAG//8G//8HFQBaU3lzdGVtLk9iamVjdCwgbXNjb3JsaWIsIFZlcnNpb249NC4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1iNzdhNWM1NjE5MzRlMDg5BwAHAAcABwICCkNTS0NCX0NHS1QHAAb//wb//wcWAFpTeXN0ZW0uT2JqZWN0LCBtc2NvcmxpYiwgVmVyc2lvbj00LjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWI3N2E1YzU2MTkzNGUwODkHAAcABwAHAgIJQ1NLQ0JfQ0xTBwAG//8G//8HFwBaU3lzdGVtLk9iamVjdCwgbXNjb3JsaWIsIFZlcnNpb249NC4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1iNzdhNWM1NjE5MzRlMDg5BwAHAAcABwICDU5HT0FJRElOSFNVQVQHAAb//wb//wcYAwcABwMHAAcCAgdISUVVTFVDBwAG//8G//8HGQoHAAcDBwAHAgIGTUlFVVRBBwAG//8G//8HGgcHAAcABwAHAgIITkdBWU5IQU4HAAb//wb//wcbCAcABwEHAAcCAgdOR0FZR1VJBwAG//8G//8HHAQHAAcBBwAHAgIMVElOSFRSQU5HR1VJBwAG//8G//8HHQMHAAcCBwAHAgIOS0VUUVVBR0lBTURJTkgHAAb//wb//wceAFpTeXN0ZW0uT2JqZWN0LCBtc2NvcmxpYiwgVmVyc2lvbj00LjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWI3N2E1YzU2MTkzNGUwODkHAAcCBwAHAgIDU1VBBwAG//8G//8HHwBaU3lzdGVtLk9iamVjdCwgbXNjb3JsaWIsIFZlcnNpb249NC4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1iNzdhNWM1NjE5MzRlMDg5BwAHAAcABwICA1hPQQcABv//Bv//ByAAWlN5c3RlbS5PYmplY3QsIG1zY29ybGliLCBWZXJzaW9uPTQuMC4wLjAsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49Yjc3YTVjNTYxOTM0ZTA4OQcABwAHAAcCAgZLRVRRVUEHAAb//wb//wchAwcABwAHAAcCAgZSRVNVTFQHAAb//wb//wciBwcABwAHAAcCAgpEQUdJQU1ESU5IBwAG//8G//8HIwoHAAcABwAHAgIPU1VBX01BTkhPTV81OTM3BwAG//8G//8HJABaU3lzdGVtLk9iamVjdCwgbXNjb3JsaWIsIFZlcnNpb249NC4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1iNzdhNWM1NjE5MzRlMDg5BwAHAAcABwIHAAcUAgEGowECAQIAAgAHBQcUBzIHZAfIBvQBBwAHAAICSUQCBEJ3QT0CAAIBAAIBKgcA&quot;}"),
                //new("gvEditing$DXFREditorcol5", ""),
                //new("gvEditing$DXFREditorcol8", ""),
                //new("gvEditing$DXFREditorcol11", ""),
                //new("gvEditing$DXFREditorcol12", ""),
                //new("gvEditing$DXFREditorcol13", ""),
                //new("gvEditing$DXFREditorcol14", ""),
                //new("gvEditing$DXFREditorcol15$State", "{&quot;rawValue&quot;:&quot;N&quot;}"),
                //new("gvEditing$DXFREditorcol15", ""),
                //new("gvEditing$DXFREditorcol15$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFREditorcol15$DDD$C", "{&quot;visibleDate&quot;:&quot;02/17/2024&quot;}"),
                //new("gvEditing$DXFREditorcol15$DDD$C$FNPState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFREditorcol16$State", "{&quot;rawValue&quot;:&quot;N&quot;}"),
                //new("gvEditing$DXFREditorcol16", ""),
                //new("gvEditing$DXFREditorcol16$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFREditorcol16$DDD$C", "{&quot;visibleDate&quot;:&quot;02/17/2024&quot;}"),
                //new("gvEditing$DXFREditorcol16$DDD$C$FNPState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFREditorcol17$State", "{&quot;rawValue&quot;:&quot;N&quot;}"),
                //new("gvEditing$DXFREditorcol17", ""),
                //new("gvEditing$DXFREditorcol17$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFREditorcol17$DDD$C", "{&quot;visibleDate&quot;:&quot;02/17/2024&quot;}"),
                //new("gvEditing$DXFREditorcol17$DDD$C$FNPState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFREditorcol18", ""),
                //new("gvEditing$DXFREditorcol19", ""),
                //new("gvEditing_DXFREditorcol20_VI", ""),
                //new("gvEditing$DXFREditorcol20", ""),
                //new("gvEditing$DXFREditorcol20$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFREditorcol20$DDD$L$State", "{&quot;CustomCallback&quot;:&quot;&quot;}"),
                //new("gvEditing$DXFREditorcol20$DDD$L", ""),
                //new("gvEditing$DXFREditorcol21", ""),
                //new("gvEditing$DXFREditorcol22", ""),
                //new("gvEditing$DXFREditorcol23", ""),
                //new("gvEditing_DXFREditorcol24_VI", ""),
                //new("gvEditing$DXFREditorcol24", ""),
                //new("gvEditing$DXFREditorcol24$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFREditorcol24$DDD$L$State", "{&quot;CustomCallback&quot;:&quot;&quot;}"),
                //new("gvEditing$DXFREditorcol24$DDD$L", ""),
                //new("gvEditing_DXFREditorcol25_VI", ""),
                //new("gvEditing$DXFREditorcol25", ""),
                //new("gvEditing$DXFREditorcol25$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFREditorcol25$DDD$L$State", "{&quot;CustomCallback&quot;:&quot;&quot;}"),
                //new("gvEditing$DXFREditorcol25$DDD$L", ""),
                //new("gvEditing$DXFREditorcol26", ""),
                //new("gvEditing$DXFREditorcol27$State", "{&quot;rawValue&quot;:&quot;N&quot;}"),
                //new("gvEditing$DXFREditorcol27", ""),
                //new("gvEditing$DXFREditorcol27$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFREditorcol27$DDD$C", "{&quot;visibleDate&quot;:&quot;02/17/2024&quot;}"),
                //new("gvEditing$DXFREditorcol27$DDD$C$FNPState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFREditorcol28$State", "{&quot;rawValue&quot;:&quot;N&quot;}"),
                //new("gvEditing$DXFREditorcol28", ""),
                //new("gvEditing$DXFREditorcol28$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFREditorcol28$DDD$C", "{&quot;visibleDate&quot;:&quot;02/17/2024&quot;}"),
                //new("gvEditing$DXFREditorcol28$DDD$C$FNPState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing_DXFREditorcol29_VI", ""),
                //new("gvEditing$DXFREditorcol29", ""),
                //new("gvEditing$DXFREditorcol29$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFREditorcol29$DDD$L$State", "{&quot;CustomCallback&quot;:&quot;&quot;}"),
                //new("gvEditing$DXFREditorcol29$DDD$L", ""),
                //new("gvEditing$custwindowState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFilterRowMenu", "{&quot;selectedItemIndexPath&quot;:&quot;&quot;,&quot;checkedState&quot;:&quot;&quot;}"),
                //new("gvEditing$DXPagerBottom$PSP", "{&quot;selectedItemIndexPath&quot;:&quot;4&quot;,&quot;checkedState&quot;:&quot;&quot;}"),
                //new("DXMVCEditorsValues", "{\"gvEditing_DXFREditorcol5\":null,\"gvEditing_DXFREditorcol8\":null,\"gvEditing_DXFREditorcol11\":null,\"gvEditing_DXFREditorcol12\":null,\"gvEditing_DXFREditorcol13\":null,\"gvEditing_DXFREditorcol14\":null,\"gvEditing_DXFREditorcol15_DDD_C\":null,\"gvEditing_DXFREditorcol15\":null,\"gvEditing_DXFREditorcol16_DDD_C\":null,\"gvEditing_DXFREditorcol16\":null,\"gvEditing_DXFREditorcol17_DDD_C\":null,\"gvEditing_DXFREditorcol17\":null,\"gvEditing_DXFREditorcol18\":null,\"gvEditing_DXFREditorcol19\":null,\"gvEditing_DXFREditorcol20_DDD_L\":[],\"gvEditing_DXFREditorcol20\":null,\"gvEditing_DXFREditorcol21\":null,\"gvEditing_DXFREditorcol22\":null,\"gvEditing_DXFREditorcol23\":null,\"gvEditing_DXFREditorcol24_DDD_L\":[],\"gvEditing_DXFREditorcol24\":null,\"gvEditing_DXFREditorcol25_DDD_L\":[],\"gvEditing_DXFREditorcol25\":null,\"gvEditing_DXFREditorcol26\":null,\"gvEditing_DXFREditorcol27_DDD_C\":null,\"gvEditing_DXFREditorcol27\":null,\"gvEditing_DXFREditorcol28_DDD_C\":null,\"gvEditing_DXFREditorcol28\":null,\"gvEditing_DXFREditorcol29_DDD_L\":[],\"gvEditing_DXFREditorcol29\":null}"),
            };

            try
            {
                var cont = await GetPage(URL_DICHVU, prms);
                SetNext(cont);
                return cont;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> DMDichVu_GetNextPage(int page)
        {
            var prms = new List<KeyValuePair<string, string>>()
            {
                new("DXCallbackName", "gvEditing"),
                new("DXCallbackArgument", $"c0:KV|5501;{nextPage};CR|2;{{}};GB|2{(page > 9 ? 1 : 0)};12|PAGERONCLICK{(page > 9 ? 4 : 3)}|PN{page};"),
                new("gvEditing", nextState),
            };

            try
            {
                var cont = await GetPage(URL_DICHVU, prms);
                SetNext(cont);
                return cont;
            }
            catch { }

            return null;
        }

        public DataTable DMDichVu_PeakTableCaption(string content)
        {
            int idx1 = content.IndexOf("<table id=\"gvEditing_DXHeaderTable\"");
            int idx2 = content.IndexOf("</table><script", idx1);
            if (idx1 < 0 || idx2 < 0) return null;

            string html = content.Substring(idx1, idx2 - idx1 + 8);
            HTMLDocument doc = HTMLDocument.Parse(html);

            DOMCollection all_head_cells = doc.Body.QuerySelector(".dxgvHeader_EIS");
            return (all_head_cells == null || all_head_cells.Count < 1) ? null : SetCap(new("DMDichVu"), all_head_cells, Field_DMDichVu);
        }

        public bool DMDichVu_PeakContent(string content, DataTable dt)
        {
            if (dt == null) return false;
            if (content == null) return true;

            int idx1 = content.IndexOf("<table id=\"gvEditing_DXMainTable\"");
            int idx2 = content.IndexOf("</table>", idx1);
            if (idx1 < 0 || idx2 < 0) return false;

            string html = content.Substring(idx1, idx2 - idx1 + 8);
            HTMLDocument doc = HTMLDocument.Parse(html);

            var all_item_rows = doc.Body.QuerySelector(".dxgvDataRow_EIS");
            if (all_item_rows?.Count < 1) return false;

            foreach (var row in all_item_rows)
            {
                var cells = row.QuerySelector("td");
                if (cells?.Count < 1) continue;

                //Bo qua chua phe duyet
                string s = cells[27].InnerText().Trim();
                if (!s.Contains("Đã phê duyệt")) continue;

                //Bo qua ngay truoc 01/01/2027
                string dte = cells[24].InnerText().Trim();
                if (DateTime.TryParseExact(dte, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dd))
                {
                    if (dd < DateInRange) continue;
                }
                else continue;

                idx1 = -1;
                List<string> values = new();
                foreach (var cel in cells)
                {
                    ++idx1;
                    if (idx1 == 22)
                    {
                        values.Add(cel.QuerySelector(".dxWeb_edtCheckBoxChecked_EIS") != null ? "1" : "");
                    }
                    else if (idx1 == 24)
                    {
                        values.Add(dte);
                    }
                    else if (idx1 == 27)
                    {
                        values.Add("1");
                    }
                    else if (idx1 < 28)
                    {
                        values.Add(cel.InnerText().Trim());
                    }
                }
                dt.Rows.Add(values.ToArray());
            }

            return true;
        }

        public int DMDichVu_PeakPageCount(string content)
        {
            int idx1 = content.IndexOf("<div class=\"dxpLite_EIS\" id=\"gvEditing_DXPagerBottom\">");
            if (idx1 < 0) return -1;

            idx1 = content.IndexOf("<b class=\"dxp-lead dxp-summary\">Page 1 of ", idx1);
            if (idx1 < 0) return -1;

            idx1 = content.IndexOf(" (", idx1);
            int idx2 = content.IndexOf(" items)", idx1);
            if (idx2 < 0) return -1;

            string s = content.Substring(idx1 + 2, idx2 - idx1 - 2).Trim();
            if (int.TryParse(s, out int res))
            {
                return res / 500 + (res % 500 > 0 ? 1 : 0);
            }
            return -1;
        }
        #endregion

        #region DMVatTu
        public async Task<string> DMVatTu_GetFirstPage()
        {
            var prms = new List<KeyValuePair<string, string>>()
            {
                new("DXCallbackName", "gvEditing"),
                new("DXCallbackArgument", "c0:KV|201;[\"5814118\",\"5814117\",\"5814116\",\"5814115\",\"5814114\",\"5814113\",\"5814112\",\"5814111\",\"5814110\",\"5814109\",\"5814108\",\"5814107\",\"5814106\",\"5814105\",\"5814104\",\"5814103\",\"5814102\",\"5814101\",\"5814040\",\"5777652\"];CR|2;{};GB|23;12|PAGERONCLICK6|PSP500;"),
                new("gvEditing", "{&quot;keys&quot;:[&quot;5777652&quot;,&quot;5777651&quot;,&quot;5777650&quot;,&quot;5777649&quot;,&quot;5777648&quot;,&quot;5709974&quot;,&quot;5709973&quot;,&quot;5709972&quot;,&quot;5709971&quot;,&quot;5709970&quot;,&quot;5709969&quot;,&quot;5709968&quot;,&quot;5654123&quot;,&quot;5503316&quot;,&quot;5503315&quot;,&quot;5503314&quot;,&quot;5503313&quot;,&quot;5492873&quot;,&quot;5492872&quot;,&quot;5472453&quot;],&quot;resizingState&quot;:&quot;{}&quot;,&quot;callbackState&quot;:&quot;BwUHAQIFU3RhdGUG+wMHKwcAAgEHAQIBBv//AgAHAgIBBwMCAQcEAgEHBQIBBwYCAQcHAgEHCAIBBwkCAQcKAgEHCwIBBwwCAQcNAgEHDgIBBw8CAQcQAgEHEQIBBxICAQcTAgEHFAIBBxUCAQcWAgEHFwIBBxgCAQcZAgEHGgIBBxsCAQccAgEHHQIBBx4CAQcfAgEHIAIBByECAQciAgEHIwIBByQCAQclAgEHJgIBBv//AgAG//8CAAb//wIABwEHAgcCBwAHAAcmBwAHAQIAAAAAAABEQAcBBwECAAAAAAAATkAHAwcBAgAAAAAAAElABwQHAQIAAAAAAABJQAcFBwECAAAAAAAAWUAHBgcBAgAAAAAAAF5ABwcHAQIAAAAAAOB1QAcIBwECAAAAAADgdUAHCQcBAgAAAAAAAGlABwoHAQIAAAAAAABeQAcLBwECAAAAAAAAaUAHDAcBAgAAAAAAAFRABw0HAQIAAAAAAABZQAcOBwECAAAAAAAAWUAHEAcBAgAAAAAAwGJABxEHAQIAAAAAAABUQAcSBwECAAAAAAAAWUAHEwcBAgAAAAAAAFRABxQHAQIAAAAAAABUQAcVBwECAAAAAADAYkAHFgcBAgAAAAAAwGJABxcHAQIAAAAAAABZQAcYBwECAAAAAADAYkAHGQcBAgAAAAAAgGZABxoHAQIAAAAAAMBiQAcbBwECAAAAAADAYkAHHAcBAgAAAAAAwGJABx0HAQIAAAAAAABUQAceBwECAAAAAAAAVEAHHwcBAgAAAAAAAE5AByAHAQIAAAAAAMBsQAchBwECAAAAAADAbEAHIgcBAgAAAAAAwGJAByMHAQIAAAAAAMBiQAckBwECAAAAAADAYkAHJQcBAgAAAAAAwGJAByYHAQIAAAAAAIBLQAcnBwECAAAAAACAS0AHKgcBBwAHAgcABwMHAAcEBwAHBQcDBwYHAwcHBwMHCAcDBwkHAwcKBwMHCwcDBwwHAwcNBwAHDgcDBw8HAAcQBwUHEQcABxIHAAcTBwMHFAcDBxUHAwcWBwMHFwcABxgHAAcZBwMHGgcABxsHAAccBwAHHQcABx4HAAcfBwAHIAcDByEHAwciBwAHIwcAByQHAAclBwAHJgcABycHAAcoBwAHKQcAByoHAAIABQAAAIAJAgJJRAcBAgJJRAQJAgACAAMHAAIBBuEFBwACAQBzRGV2RXhwcmVzcy5XZWIuQ2hlY2tTdGF0ZSwgRGV2RXhwcmVzcy5XZWIudjE1LjEsIFZlcnNpb249MTUuMS44LjAsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49Yjg4ZDE3NTRkNzAwZTQ5YQcBAgEHAAcABwAHAAINU2hvd0ZpbHRlclJvdwoCAQIRU2hvd0ZpbHRlclJvd01lbnUKAgECCFBhZ2VTaXplAwcUAhFTZWFyY2hQYW5lbEZpbHRlcgk=&quot;,&quot;scrollState&quot;:&quot;-1;0&quot;,&quot;selection&quot;:&quot;&quot;,&quot;customOperationState&quot;:&quot;ByoCAAcABv//Bv//BwEAWlN5c3RlbS5PYmplY3QsIG1zY29ybGliLCBWZXJzaW9uPTQuMC4wLjAsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49Yjc3YTVjNTYxOTM0ZTA4OQcABwAHAAcCAgJJRAcCBwAG//8HAgcHAAcABwAHAgICVFQHAAb//wb//wcDAFpTeXN0ZW0uT2JqZWN0LCBtc2NvcmxpYiwgVmVyc2lvbj00LjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWI3N2E1YzU2MTkzNGUwODkHAAcCBwAHAgICQVgHAAb//wb//wcEAFpTeXN0ZW0uT2JqZWN0LCBtc2NvcmxpYiwgVmVyc2lvbj00LjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWI3N2E1YzU2MTkzNGUwODkHAAcCBwAHAgIGTUFfQllUBwAG//8G//8HBQcHAAcABwMHAgIFTUFfQVgHAAb//wb//wcGBwcABwAHAwcCAgNURU4HAAb//wb//wcHBwcABwAHAwcCAgZURU5fQVgHAAb//wb//wcIBwcABwAHAwcCAgdRVVlDQUNIBwAG//8G//8HCQcHAAcABwMHAgIGTlVPQ1NYBwAG//8G//8HCgcHAAcABwMHAgIFTkhBU1gHAAb//wb//wcLBwcABwAHAwcCAglET05WSVRJTkgHAAb//wb//wcMBwcABwAHAwcCAgNHSUEHAAb//wb//wcNBQcABwAHAAcCAgdHSUFCSFlUBwAG//8G//8HDgUHAAcABwMHAgIMU09DQVRIVUNISUVOBwAG//8G//8HDwQHAAcABwAHAgIHRElOSE1VQwcABv//Bv//BxAFBwAHAAcFBwICB1NPTFVPTkcHAAb//wb//wcRBQcABwAHAAcCAghMT0FJVEhBVQcABv//Bv//BxIDBwAHAgcABwICBk1BSElFVQcABv//Bv//BxMHBwAHAAcDBwICBFRZTEUHAAb//wb//wcUBQcABwAHAwcCAglRVVlFVERJTkgHAAb//wb//wcVBwcABwAHAwcCAgdOSEFUSEFVBwAG//8G//8HFgcHAAcABwMHAgILTUFOSE9NXzkzMjQHAAb//wb//wcXBwcABwIHAAcCAgtNQU5IT01fNTkzNwcABv//Bv//BxgAWlN5c3RlbS5PYmplY3QsIG1zY29ybGliLCBWZXJzaW9uPTQuMC4wLjAsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49Yjc3YTVjNTYxOTM0ZTA4OQcABwAHAAcCAgdIVF9USEFVBwAG//8G//8HGQBaU3lzdGVtLk9iamVjdCwgbXNjb3JsaWIsIFZlcnNpb249NC4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1iNzdhNWM1NjE5MzRlMDg5BwAHAAcDBwICB0RWX1RIQVUHAAb//wb//wcaAFpTeXN0ZW0uT2JqZWN0LCBtc2NvcmxpYiwgVmVyc2lvbj00LjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWI3N2E1YzU2MTkzNGUwODkHAAcABwAHAgIFTUFfQlYHAAb//wb//wcbBwcABwAHAAcCAgZURU5fQlYHAAb//wb//wccBwcABwAHAAcCAgZUVU5HQVkHAAb//wb//wcdCAcABwEHAAcCAgdERU5OR0FZBwAG//8G//8HHggHAAcBBwAHAgIHSElFVUxVQwcABv//Bv//Bx8KBwAHAwcABwICBk1JRVVUQQcABv//Bv//ByAHBwAHAAcDBwICB1RUX1RIQVUHAAb//wb//wchAFpTeXN0ZW0uT2JqZWN0LCBtc2NvcmxpYiwgVmVyc2lvbj00LjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWI3N2E1YzU2MTkzNGUwODkHAAcABwMHAgIITkdBWU5IQU4HAAb//wb//wciCAcABwEHAAcCAgdOR0FZR1VJBwAG//8G//8HIwQHAAcBBwAHAgIMVElOSFRSQU5HR1VJBwAG//8G//8HJAMHAAcCBwAHAgIOS0VUUVVBR0lBTURJTkgHAAb//wb//wclAFpTeXN0ZW0uT2JqZWN0LCBtc2NvcmxpYiwgVmVyc2lvbj00LjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWI3N2E1YzU2MTkzNGUwODkHAAcCBwAHAgIDU1VBBwAG//8G//8HJgBaU3lzdGVtLk9iamVjdCwgbXNjb3JsaWIsIFZlcnNpb249NC4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1iNzdhNWM1NjE5MzRlMDg5BwAHAAcABwICA1hPQQcABv//Bv//BycAWlN5c3RlbS5PYmplY3QsIG1zY29ybGliLCBWZXJzaW9uPTQuMC4wLjAsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49Yjc3YTVjNTYxOTM0ZTA4OQcABwAHAAcCAgZLRVRRVUEHAAb//wb//wcoAwcABwAHAAcCAgZSRVNVTFQHAAb//wb//wcpBwcABwAHAAcCAgpEQUdJQU1ESU5IBwAG//8G//8HKgoHAAcABwAHAgcABxQCAQdMAgECAAIABwUHFAcyB2QHyAb0AQcABwACAklEAgRCd0E9AgACAQACASoHAA==&quot;}"),
                //new("gvEditing$DXFREditorcol5", ""),
                //new("gvEditing$DXFREditorcol6", ""),
                //new("gvEditing$DXFREditorcol7", ""),
                //new("gvEditing$DXFREditorcol8", ""),
                //new("gvEditing$DXFREditorcol9", ""),
                //new("gvEditing$DXFREditorcol10", ""),
                //new("gvEditing$DXFREditorcol11", ""),
                //new("gvEditing$DXFREditorcol12", ""),
                //new("gvEditing$DXFREditorcol13", ""),
                //new("gvEditing$DXFREditorcol14", ""),
                //new("gvEditing$DXFREditorcol15", ""),
                //new("gvEditing$DXFREditorcol16", ""),
                //new("gvEditing$DXFREditorcol17", ""),
                //new("gvEditing_DXFREditorcol18_VI", ""),
                //new("gvEditing$DXFREditorcol18", ""),
                //new("gvEditing$DXFREditorcol18$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFREditorcol18$DDD$L$State", "{&quot;CustomCallback&quot;:&quot;&quot;}"),
                //new("gvEditing$DXFREditorcol18$DDD$L", ""),
                //new("gvEditing$DXFREditorcol19", ""),
                //new("gvEditing$DXFREditorcol20", ""),
                //new("gvEditing$DXFREditorcol21", ""),
                //new("gvEditing$DXFREditorcol22", ""),
                //new("gvEditing_DXFREditorcol23_VI", ""),
                //new("gvEditing$DXFREditorcol23", ""),
                //new("gvEditing$DXFREditorcol23$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFREditorcol23$DDD$L$State", "{&quot;CustomCallback&quot;:&quot;&quot;}"),
                //new("gvEditing$DXFREditorcol23$DDD$L", ""),
                //new("gvEditing$DXFREditorcol24", ""),
                //new("gvEditing$DXFREditorcol25", ""),
                //new("gvEditing$DXFREditorcol26", ""),
                //new("gvEditing$DXFREditorcol27", ""),
                //new("gvEditing$DXFREditorcol28", ""),
                //new("gvEditing$DXFREditorcol29$State", "{&quot;rawValue&quot;:&quot;N&quot;}"),
                //new("gvEditing$DXFREditorcol29", ""),
                //new("gvEditing$DXFREditorcol29$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFREditorcol29$DDD$C", "{&quot;visibleDate&quot;:&quot;02/17/2024&quot;}"),
                //new("gvEditing$DXFREditorcol29$DDD$C$FNPState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFREditorcol30$State", "{&quot;rawValue&quot;:&quot;N&quot;}"),
                //new("gvEditing$DXFREditorcol30", ""),
                //new("gvEditing$DXFREditorcol30$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFREditorcol30$DDD$C", "{&quot;visibleDate&quot;:&quot;02/17/2024&quot;}"),
                //new("gvEditing$DXFREditorcol30$DDD$C$FNPState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing_DXFREditorcol31_VI", ""),
                //new("gvEditing$DXFREditorcol31", ""),
                //new("gvEditing$DXFREditorcol31$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFREditorcol31$DDD$L$State", "{&quot;CustomCallback&quot;:&quot;&quot;}"),
                //new("gvEditing$DXFREditorcol31$DDD$L", ""),
                //new("gvEditing$DXFREditorcol32", ""),
                //new("gvEditing$DXFREditorcol33", ""),
                //new("gvEditing$DXFREditorcol34$State", "{&quot;rawValue&quot;:&quot;N&quot;}"),
                //new("gvEditing$DXFREditorcol34", ""),
                //new("gvEditing$DXFREditorcol34$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFREditorcol34$DDD$C", "{&quot;visibleDate&quot;:&quot;02/17/2024&quot;}"),
                //new("gvEditing$DXFREditorcol34$DDD$C$FNPState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFREditorcol35$State", "{&quot;rawValue&quot;:&quot;N&quot;}"),
                //new("gvEditing$DXFREditorcol35", ""),
                //new("gvEditing$DXFREditorcol35$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFREditorcol35$DDD$C", "{&quot;visibleDate&quot;:&quot;02/17/2024&quot;}"),
                //new("gvEditing$DXFREditorcol35$DDD$C$FNPState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing_DXFREditorcol36_VI", ""),
                //new("gvEditing$DXFREditorcol36", ""),
                //new("gvEditing$DXFREditorcol36$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFREditorcol36$DDD$L$State", "{&quot;CustomCallback&quot;:&quot;&quot;}"),
                //new("gvEditing$DXFREditorcol36$DDD$L", ""),
                //new("gvEditing$custwindowState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFilterRowMenu", "{&quot;selectedItemIndexPath&quot;:&quot;&quot;,&quot;checkedState&quot;:&quot;&quot;}"),
                //new("gvEditing$DXPagerBottom$PSP", "{&quot;selectedItemIndexPath&quot;:&quot;4&quot;,&quot;checkedState&quot;:&quot;&quot;}"),
                //new("DXMVCEditorsValues", "{\"gvEditing_DXFREditorcol5\":null,\"gvEditing_DXFREditorcol6\":null,\"gvEditing_DXFREditorcol7\":null,\"gvEditing_DXFREditorcol8\":null,\"gvEditing_DXFREditorcol9\":null,\"gvEditing_DXFREditorcol10\":null,\"gvEditing_DXFREditorcol11\":null,\"gvEditing_DXFREditorcol12\":null,\"gvEditing_DXFREditorcol13\":null,\"gvEditing_DXFREditorcol14\":null,\"gvEditing_DXFREditorcol15\":null,\"gvEditing_DXFREditorcol16\":null,\"gvEditing_DXFREditorcol17\":null,\"gvEditing_DXFREditorcol18_DDD_L\":[],\"gvEditing_DXFREditorcol18\":null,\"gvEditing_DXFREditorcol19\":null,\"gvEditing_DXFREditorcol20\":null,\"gvEditing_DXFREditorcol21\":null,\"gvEditing_DXFREditorcol22\":null,\"gvEditing_DXFREditorcol23_DDD_L\":[],\"gvEditing_DXFREditorcol23\":null,\"gvEditing_DXFREditorcol24\":null,\"gvEditing_DXFREditorcol25\":null,\"gvEditing_DXFREditorcol26\":null,\"gvEditing_DXFREditorcol27\":null,\"gvEditing_DXFREditorcol28\":null,\"gvEditing_DXFREditorcol29_DDD_C\":null,\"gvEditing_DXFREditorcol29\":null,\"gvEditing_DXFREditorcol30_DDD_C\":null,\"gvEditing_DXFREditorcol30\":null,\"gvEditing_DXFREditorcol31_DDD_L\":[],\"gvEditing_DXFREditorcol31\":null,\"gvEditing_DXFREditorcol32\":null,\"gvEditing_DXFREditorcol33\":null,\"gvEditing_DXFREditorcol34_DDD_C\":null,\"gvEditing_DXFREditorcol34\":null,\"gvEditing_DXFREditorcol35_DDD_C\":null,\"gvEditing_DXFREditorcol35\":null,\"gvEditing_DXFREditorcol36_DDD_L\":[],\"gvEditing_DXFREditorcol36\":null}"),
            };

            try
            {
                var cont = await GetPage(URL_VATTU, prms);
                SetNext(cont);
                return cont;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> DMVatTu_GetNextPage(int page)
        {
            var prms = new List<KeyValuePair<string, string>>()
            {
                new("DXCallbackName", "gvEditing"),
                new("DXCallbackArgument", $"c0:KV|5001;{nextPage};CR|2;{{}};GB|2{(page > 9 ? 1 : 0)};12|PAGERONCLICK{(page > 9 ? 4 : 3)}|PN{page};"),
                new("gvEditing", nextState),
            };

            try
            {
                var cont = await GetPage(URL_VATTU, prms);
                SetNext(cont);
                return cont;
            }
            catch { }

            return null;
        }

        public DataTable DMVatTu_PeakTableCaption(string content)
        {
            int idx1 = content.IndexOf("<table id=\"gvEditing_DXHeaderTable\"");
            int idx2 = content.IndexOf("</table><script", idx1);
            if (idx1 < 0 || idx2 < 0) return null;

            string html = content.Substring(idx1, idx2 - idx1 + 8);
            HTMLDocument doc = HTMLDocument.Parse(html);

            DOMCollection all_head_cells = doc.Body.QuerySelector(".dxgvHeader_EIS");
            return (all_head_cells == null || all_head_cells.Count < 1) ? null : SetCap(new("DMVatTu"), all_head_cells, Field_DMVatTu);
        }

        public bool DMVatTu_PeakContent(string content, DataTable dt)
        {
            if (dt == null) return false;

            int idx1 = content.IndexOf("<table id=\"gvEditing_DXMainTable\"");
            int idx2 = content.IndexOf("</table>", idx1);
            if (idx1 < 0 || idx2 < 0) return false;

            string html = content.Substring(idx1, idx2 - idx1 + 8);
            HTMLDocument doc = HTMLDocument.Parse(html);

            var all_item_rows = doc.Body.QuerySelector(".dxgvDataRow_EIS");
            if (all_item_rows?.Count < 1) return false;

            foreach (var row in all_item_rows)
            {
                var cells = row.QuerySelector("td");
                if (cells?.Count < 1) continue;

                //Bo qua chua phe duyet
                string s = cells[36].InnerText().Trim();
                if (!s.Contains("Đã phê duyệt")) continue;

                //Bo qua ngay truoc 01/01/2027
                string dte = cells[33].InnerText().Trim();
                if (DateTime.TryParseExact(dte, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dd))
                {
                    if (dd < DateInRange) continue;
                }
                else continue;

                idx1 = -1;
                List<string> values = new();
                foreach (var cel in cells)
                {
                    ++idx1;
                    if (idx1 == 30)
                    {
                        values.Add(cel.QuerySelector(".dxWeb_edtCheckBoxChecked_EIS") != null ? "1" : "");
                    }
                    else if (idx1 == 33)
                    {
                        values.Add(dte);
                    }
                    else if (idx1 == 36)
                    {
                        values.Add("1");
                    }
                    else if (idx1 < 37)
                    {
                        values.Add(cel.InnerText().Trim());
                    }
                }
                dt.Rows.Add(values.ToArray());
            }

            return true;
        }

        public int DMVatTu_PeakPageCount(string content)
        {
            int idx1 = content.IndexOf("<div class=\"dxpLite_EIS\" id=\"gvEditing_DXPagerBottom\">");
            if (idx1 < 0) return -1;

            idx1 = content.IndexOf("<b class=\"dxp-lead dxp-summary\">Page 1 of ", idx1);
            if (idx1 < 0) return -1;

            idx1 = content.IndexOf(" (", idx1);
            int idx2 = content.IndexOf(" items)", idx1);
            if (idx2 < 0) return -1;

            string s = content.Substring(idx1 + 2, idx2 - idx1 - 2).Trim();
            if (int.TryParse(s, out int res))
            {
                return res / 500 + (res % 500 > 0 ? 1 : 0);
            }
            return -1;
        }
        #endregion

        #region DMBacSi
        public async Task<string> DMBacSi_GetFirstPage()
        {
            var prms = new List<KeyValuePair<string, string>>()
            {
                new("DXCallbackName", "gvEditing"),
                new("DXCallbackArgument", "c0:KV|151;[\"2697389\",\"2697388\",\"2656309\",\"2656308\",\"2656307\",\"2656306\",\"2656095\",\"2655799\",\"2655798\",\"2655797\",\"2655796\",\"2655795\",\"2655794\",\"2655793\",\"2655777\"];GB|23;12|PAGERONCLICK6|PSP100;"),
                new("gvEditing", "{&quot;customOperationState&quot;:&quot;Bx0CAAcABv//Bv//BwEAWlN5c3RlbS5PYmplY3QsIG1zY29ybGliLCBWZXJzaW9uPTQuMC4wLjAsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49Yjc3YTVjNTYxOTM0ZTA4OQcABwAHAAcCAgVUYWNWdQcABv//Bv//BwIHBwAHAAcABwICB01BX0JIWEgHAAb//wb//wcDAFpTeXN0ZW0uT2JqZWN0LCBtc2NvcmxpYiwgVmVyc2lvbj00LjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWI3N2E1YzU2MTkzNGUwODkHAAcABwMHAgIMVEVOX05IQU5WSUVOBwAG//8G//8HBAcHAAcABwMHAgIJR0lPSV9USU5IBwAG//8G//8HBQMHAAcCBwAHAgILTUFfTE9BSV9LQ0IHAAb//wb//wcGAFpTeXN0ZW0uT2JqZWN0LCBtc2NvcmxpYiwgVmVyc2lvbj00LjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWI3N2E1YzU2MTkzNGUwODkHAAcCBwMHAgIHTUFfS0hPQQcABv//Bv//BwcHBwAHAAcDBwICCFRFTl9LSE9BBwAG//8G//8HCAcHAAcABwMHAgIJVkFOQkFOR0NNBwAG//8G//8HCQcHAAcCBwAHAgIGVklfVFJJBwAG//8G//8HCgcHAAcCBwMHAgIGTUFDQ0hOBwAG//8G//8HCwcHAAcABwMHAgIMTkdBWUNBUF9DQ0hOBwAG//8G//8HDAgHAAcABwAHAgIHTk9JX0NBUAcABv//Bv//Bw0HBwAHAAcABwICDk1BX0NIVVlFTk5HQU5IBwAG//8G//8HDgcHAAcABwAHAgILUEhBTVZJX0NNQlMHAAb//wb//wcPBwcABwAHAAcCAglEVktUX0tIQUMHAAb//wb//wcQBwcABwAHAAcCAgtWQl9QSEFOQ09ORwcABv//Bv//BxEHBwAHAAcABwICC1RIT0lHSUFOX0RLBwAG//8G//8HEgcHAAcABwMHAgINVEhPSUdJQU5fTkdBWQcABv//Bv//BxMHBwAHAAcDBwICDVRIT0lHSUFOX1RVQU4HAAb//wb//wcUBwcABwAHAwcCAgpDU0tDQl9LSEFDBwAG//8G//8HFQcHAAcABwMHAgIKQ1NLQ0JfQ0dLVAcABv//Bv//BxYHBwAHAAcDBwICB1FEX0NHS1QHAAb//wb//wcXBwcABwAHAwcCAgdUVV9OR0FZBwAG//8G//8HGAgHAAcABwAHAgIIREVOX05HQVkHAAb//wb//wcZCAcABwAHAAcCAgdMWURPX1RDBwAG//8G//8HGgcHAAcABwAHAgIMVElOSFRSQU5HR1VJBwAG//8G//8HGwMHAAcCBwMHAgIOVElOSFRSQU5HRFVZRVQHAAb//wb//wccAwcABwIHAwcCAgdISUVVTFVDBwAG//8G//8HHQoHAAcDBwAHAgcABw8CAQcnAgECAAIABwYHCgcPBxQHHgcyB2QHAAcAAgJJRAIEQndBPQIAAgEAAgEqBwA=&quot;,&quot;keys&quot;:[&quot;2697389&quot;,&quot;2697388&quot;,&quot;2656309&quot;,&quot;2656308&quot;,&quot;2656307&quot;,&quot;2656306&quot;,&quot;2656095&quot;,&quot;2655799&quot;,&quot;2655798&quot;,&quot;2655797&quot;,&quot;2655796&quot;,&quot;2655795&quot;,&quot;2655794&quot;,&quot;2655793&quot;,&quot;2655777&quot;],&quot;callbackState&quot;:&quot;BwUHAQIFU3RhdGUGmwEHHgcAAgEHAQIBBwICAQcDAgEHBAIBBwUCAQcGAgEHBwIBBwgCAQcJAgEHCgIBBwsCAQcMAgEHDQIBBw4CAQcPAgEHEAIBBxECAQcSAgEHEwIBBxQCAQcVAgEHFgIBBxcCAQcYAgEHGQIBBxoCAQcbAgEHHAIBBx0CAQcABwAHAAcdBwEHAAcCBwAHAwcDBwQHAwcFBwAHBgcDBwcHAwcIBwMHCQcABwoHAwcLBwMHDAcABw0HAAcOBwAHDwcABxAHAAcRBwAHEgcDBxMHAwcUBwMHFQcDBxYHAwcXBwMHGAcABxkHAAcaBwAHGwcDBxwHAwcdBwACAAUAAACACQICSUQHAQICSUQECQIAAgADBwACAQZCAgcAAgEAc0RldkV4cHJlc3MuV2ViLkNoZWNrU3RhdGUsIERldkV4cHJlc3MuV2ViLnYxNS4xLCBWZXJzaW9uPTE1LjEuOC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWI4OGQxNzU0ZDcwMGU0OWEHAQIBBwAHAAcABwACDVNob3dGaWx0ZXJSb3cKAgECEVNob3dGaWx0ZXJSb3dNZW51CgIBAghQYWdlU2l6ZQMHDwIRU2VhcmNoUGFuZWxGaWx0ZXIJ&quot;,&quot;scrollState&quot;:&quot;-1;0&quot;,&quot;selection&quot;:&quot;&quot;}"),
                //new("gvEditing$DXFREditorcol3", ""),
                //new("gvEditing$DXFREditorcol4", ""),
                //new("gvEditing$DXFREditorcol7", ""),
                //new("gvEditing$DXFREditorcol8", ""),
                //new("gvEditing_DXFREditorcol5_VI", ""),
                //new("gvEditing$DXFREditorcol5", ""),
                //new("gvEditing$DXFREditorcol5$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFREditorcol5$DDD$L$State", "{&quot;CustomCallback&quot;:&quot;&quot;}"),
                //new("gvEditing$DXFREditorcol5$DDD$L", ""),
                //new("gvEditing$DXFREditorcol11", ""),
                //new("gvEditing$DXFREditorcol18", ""),
                //new("gvEditing$DXFREditorcol19", ""),
                //new("gvEditing$DXFREditorcol20", ""),
                //new("gvEditing$DXFREditorcol21", ""),
                //new("gvEditing$DXFREditorcol22", ""),
                //new("gvEditing$DXFREditorcol23", ""),
                //new("gvEditing_DXFREditorcol29_VI", ""),
                //new("gvEditing$DXFREditorcol29", ""),
                //new("gvEditing$DXFREditorcol29$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFREditorcol29$DDD$L$State", "{&quot;CustomCallback&quot;:&quot;&quot;}"),
                //new("gvEditing$DXFREditorcol29$DDD$L", ""),
                //new("btnEditPVCM_DVKT_2697389", "submit"),
                //new("btnDelete_2697389", "submit"),
                //new("btnEditPVCM_DVKT_2697388", "submit"),
                //new("btnDelete_2697388", "submit"),
                //new("btnEditPVCM_DVKT_2656309", "submit"),
                //new("btnDelete_2656309", "submit"),
                //new("btnEditPVCM_DVKT_2656308", "submit"),
                //new("btnDelete_2656308", "submit"),
                //new("btnEditPVCM_DVKT_2656307", "submit"),
                //new("btnDelete_2656307", "submit"),
                //new("btnEditPVCM_DVKT_2656306", "submit"),
                //new("btnDelete_2656306", "submit"),
                //new("btnEditPVCM_DVKT_2656095", "submit"),
                //new("btnDelete_2656095", "submit"),
                //new("btnEditPVCM_DVKT_2655799", "submit"),
                //new("btnDelete_2655799", "submit"),
                //new("btnEditPVCM_DVKT_2655798", "submit"),
                //new("btnDelete_2655798", "submit"),
                //new("btnEditPVCM_DVKT_2655797", "submit"),
                //new("btnDelete_2655797", "submit"),
                //new("btnEditPVCM_DVKT_2655796", "submit"),
                //new("btnDelete_2655796", "submit"),
                //new("btnEditPVCM_DVKT_2655795", "submit"),
                //new("btnDelete_2655795", "submit"),
                //new("btnEditPVCM_DVKT_2655794", "submit"),
                //new("btnDelete_2655794", "submit"),
                //new("btnEditPVCM_DVKT_2655793", "submit"),
                //new("btnDelete_2655793", "submit"),
                //new("btnEditPVCM_DVKT_2655777", "submit"),
                //new("btnDelete_2655777", "submit"),
                //new("gvEditing$custwindowState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFilterRowMenu", "{&quot;selectedItemIndexPath&quot;:&quot;&quot;,&quot;checkedState&quot;:&quot;&quot;}"),
                //new("gvEditing$DXPagerBottom$PSP", "{&quot;selectedItemIndexPath&quot;:&quot;5&quot;,&quot;checkedState&quot;:&quot;&quot;}"),
                //new("DXMVCEditorsValues", "{\"gvEditing_DXFREditorcol3\":null,\"gvEditing_DXFREditorcol4\":null,\"gvEditing_DXFREditorcol7\":null,\"gvEditing_DXFREditorcol8\":null,\"gvEditing_DXFREditorcol5_DDD_L\":[],\"gvEditing_DXFREditorcol5\":null,\"gvEditing_DXFREditorcol11\":null,\"gvEditing_DXFREditorcol18\":null,\"gvEditing_DXFREditorcol19\":null,\"gvEditing_DXFREditorcol20\":null,\"gvEditing_DXFREditorcol21\":null,\"gvEditing_DXFREditorcol22\":null,\"gvEditing_DXFREditorcol23\":null,\"gvEditing_DXFREditorcol29_DDD_L\":[],\"gvEditing_DXFREditorcol29\":null}"),
                new("str_TimKiem", ""),
                new("loaiNhap", "1"),
                new("trangThai", "-1"),
                new("tinhTrangGui", "2"),
                new("tinhTrangDuyet", "1"),
            };

            try
            {
                var cont = await GetPage(URL_BACSI, prms);
                SetNext(cont);
                return cont;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> DMBacSi_GetNextPage(int page)
        {
            var prms = new List<KeyValuePair<string, string>>()
            {
                new("DXCallbackName", "gvEditing"),
                new("DXCallbackArgument", $"c0:KV|1001;{nextPage};GB|2{(page > 9 ? 1 : 0)};12|PAGERONCLICK{(page > 9 ? 4 : 3)}|PN{page};"),
                new("gvEditing", nextState),

                new("str_TimKiem", ""),
                new("loaiNhap", "1"),
                new("trangThai", "-1"),
                new("tinhTrangGui", "2"),
                new("tinhTrangDuyet", "1"),
            };

            try
            {
                var cont = await GetPage(URL_BACSI, prms);
                SetNext(cont);
                return cont;
            }
            catch { }

            return null;
        }

        public DataTable DMBacSi_PeakTableCaption(string content)
        {
            int idx1 = content.IndexOf("<table id=\"gvEditing_DXHeaderTable\"");
            int idx2 = content.IndexOf("</table><script", idx1);
            if (idx1 < 0 || idx2 < 0) return null;

            string html = content.Substring(idx1, idx2 - idx1 + 8);
            HTMLDocument doc = HTMLDocument.Parse(html);

            DOMCollection all_head_cells = doc.Body.QuerySelector(".dxgvHeader_EIS");
            return (all_head_cells == null || all_head_cells.Count < 1) ? null : SetCap(new("DMBacSi"), all_head_cells, Field_DMBacSi);
        }

        public bool DMBacSi_PeakContent(string content, DataTable dt)
        {
            if (dt == null) return false;

            int idx1 = content.IndexOf("<table id=\"gvEditing_DXMainTable\"");
            int idx2 = content.IndexOf("</div><div id=\"gvEditing_DXFixedColumnsDiv\" class=\"dxgvFCSD\"", idx1);
            if (idx1 < 0 || idx2 < 0) return false;

            string html = content.Substring(idx1, idx2 - idx1);
            HTMLDocument doc = HTMLDocument.Parse(html);

            var all_item_rows = doc.Body.QuerySelector(".dxgvDataRow_EIS");
            if (all_item_rows?.Count < 1) return false;

            foreach (var row in all_item_rows)
            {
                var cells = row.Children[2].Children[0].QuerySelector("td");
                if (cells?.Count < 1) continue;

                //Bo qua chua phe duyet
                string s = cells[27].InnerText().Trim();
                if (!s.Contains("GD đã duyệt")) continue;

                idx1 = -1;
                List<string> values = new() { "" };
                foreach (var cel in cells)
                {
                    ++idx1;
                    if (idx1 == 28)
                    {
                        values.Add(cel.QuerySelector(".dxWeb_edtCheckBoxChecked_EIS") != null ? "1" : "");
                    }
                    else if (idx1 < 29)
                    {
                        values.Add(cel.InnerText().Trim());
                    }
                }
                dt.Rows.Add(values.ToArray());
            }

            return true;
        }

        public int DMBacSi_PeakPageCount(string content)
        {
            int idx1 = content.IndexOf("<div class=\"dxpLite_EIS\" id=\"gvEditing_DXPagerBottom\">");
            if (idx1 < 0) return -1;

            idx1 = content.IndexOf("<b class=\"dxp-lead dxp-summary\">Page 1 of ", idx1);
            if (idx1 < 0) return -1;

            idx1 = content.IndexOf(" (", idx1);
            int idx2 = content.IndexOf(" items)", idx1);
            if (idx2 < 0) return -1;

            string s = content.Substring(idx1 + 2, idx2 - idx1 - 2).Trim();
            if (int.TryParse(s, out int res))
            {
                return res / 100 + (res % 100 > 0 ? 1 : 0);
            }
            return -1;
        }
        #endregion

        #region DMKhoa
        public async Task<string> DMKhoa_GetFirstPage()
        {
            var prms = new List<KeyValuePair<string, string>>()
            {
                new("DXCallbackName", "gvEditing"),
                new("DXCallbackArgument", "c0:KV|121;[\"55530\",\"55529\",\"55532\",\"55534\",\"55535\",\"54483\",\"55533\",\"55557\",\"55556\",\"55544\",\"55547\",\"55512\",\"55515\",\"55545\",\"55550\"];CR|2;{};GB|22;12|PAGERONCLICK5|PSP30;"),
                new("gvEditing", "{&quot;keys&quot;:[&quot;54483&quot;,&quot;55511&quot;,&quot;55512&quot;,&quot;91430&quot;,&quot;55514&quot;,&quot;55515&quot;,&quot;55536&quot;,&quot;55537&quot;,&quot;55538&quot;,&quot;55529&quot;,&quot;55530&quot;,&quot;56294&quot;,&quot;55532&quot;,&quot;55533&quot;,&quot;55534&quot;,&quot;55535&quot;,&quot;55556&quot;,&quot;55557&quot;,&quot;55542&quot;,&quot;55544&quot;,&quot;55545&quot;,&quot;91473&quot;,&quot;98305&quot;,&quot;55547&quot;,&quot;55548&quot;,&quot;55549&quot;,&quot;55550&quot;,&quot;55551&quot;,&quot;55552&quot;,&quot;55553&quot;,&quot;55554&quot;,&quot;98322&quot;,&quot;55555&quot;,&quot;93961&quot;],&quot;lastMultiSelectIndex&quot;:&quot;&quot;,&quot;resizingState&quot;:&quot;{}&quot;,&quot;callbackState&quot;:&quot;BwQHAQIFU3RhdGUGDgEHCgcAAgEHAQIBBwICAQb//wIABwMCAQcEAgEHBQIBBwYCAQcHAgEHCAIBBwAHAAcKBwAHAQIAAAAAAABJQAcBBwECAAAAAADgdUAHAgcBAgAAAAAAQG9ABwMHAQIAAAAAAOB1QAcEBwECAAAAAADghUAHBQcBAgAAAAAAAFlABwYHAQIAAAAAAABZQAcHBwECAAAAAAAASUAHCAcBAgAAAAAAgFFABwkHAQIAAAAAAIBRQAcKBwAHAAcBBwMHAgcDBwMHAwcEBwMHBQcABwYHAAcHBwAHCAcABwkHAAIABQAAAIAJAgJJRAcBAgJJRAQJAgACAAMHBAIABwACAQciBwACAAIBBwAHAAcABwACDVNob3dGaWx0ZXJSb3cKAgECEVNob3dGaWx0ZXJSb3dNZW51CgIBAghQYWdlU2l6ZQMHPA==&quot;,&quot;scrollState&quot;:&quot;0;0&quot;,&quot;selection&quot;:&quot;&quot;,&quot;customOperationState&quot;:&quot;BwoCAAcABv//Bv//BwAAWlN5c3RlbS5PYmplY3QsIG1zY29ybGliLCBWZXJzaW9uPTQuMC4wLjAsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49Yjc3YTVjNTYxOTM0ZTA4OQcABwAHAAcCAgJNQQcABv//Bv//BwEHBwAHAgcDBwICA1RFTgcABv//Bv//BwIHBwAHAAcDBwICCE1BX0NTS0NCBwAG//8G//8HAwcHAAcCBwMHAgIGTUlFVVRBBwAG//8G//8HBAcHAAcABwMHAgIHVFVfTkdBWQcABv//Bv//BwUIBwAHAQcABwICCERFTl9OR0FZBwAG//8G//8HBggHAAcBBwAHAgIHSElFVUxVQwcABv//Bv//BwcKBwAHAwcABwICAAcABv//Bv//BwgAWlN5c3RlbS5PYmplY3QsIG1zY29ybGliLCBWZXJzaW9uPTQuMC4wLjAsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49Yjc3YTVjNTYxOTM0ZTA4OQcABwAHAAcCAgAHAAb//wb//wcJAFpTeXN0ZW0uT2JqZWN0LCBtc2NvcmxpYiwgVmVyc2lvbj00LjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWI3N2E1YzU2MTkzNGUwODkHAAcABwAHAgcABzwCAQcBAgECAAIABwcHCgcPBxQHHgcoBzIHPAcABwACAklEAgRCd0E9AgACAQIAAgEqBwA=&quot;}"),
                //new("gvEditing_DXFREditorcol1_VI", ""),
                //new("gvEditing$DXFREditorcol1", ""),
                //new("gvEditing$DXFREditorcol1$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFREditorcol1$DDD$L$State", "{&quot;CustomCallback&quot;:&quot;&quot;}"),
                //new("gvEditing$DXFREditorcol1$DDD$L", ""),
                //new("gvEditing$DXFREditorcol2", ""),
                //new("gvEditing$DXFREditorcol4", ""),
                //new("gvEditing$DXFREditorcol5$State", "{&quot;rawValue&quot;:&quot;N&quot;}"),
                //new("gvEditing$DXFREditorcol5", ""),
                //new("gvEditing$DXFREditorcol5$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFREditorcol5$DDD$C", "{&quot;visibleDate&quot;:&quot;02/18/2024&quot;}"),
                //new("gvEditing$DXFREditorcol5$DDD$C$FNPState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFREditorcol6$State", "{&quot;rawValue&quot;:&quot;N&quot;}"),
                //new("gvEditing$DXFREditorcol6", ""),
                //new("gvEditing$DXFREditorcol6$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFREditorcol6$DDD$C", "{&quot;visibleDate&quot;:&quot;02/18/2024&quot;}"),
                //new("gvEditing$DXFREditorcol6$DDD$C$FNPState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing_DXFREditorcol7_VI", ""),
                //new("gvEditing$DXFREditorcol7", ""),
                //new("gvEditing$DXFREditorcol7$DDDState", "{&quot;windowsState&quot;:&quot;0:0:-1:0:0:0:-10000:-10000:1:0:0:0&quot;}"),
                //new("gvEditing$DXFREditorcol7$DDD$L$State", "{&quot;CustomCallback&quot;:&quot;&quot;}"),
                //new("gvEditing$DXFREditorcol7$DDD$L", ""),
                //new("gvEditing$DXFREditorcol8", ""),
                //new("gvEditing$DXFREditorcol9", ""),
                //new("ButtonEdit54483", "+"),
                //new("ButtonXoa54483", "+"),
                //new("ButtonEdit55511", "+"),
                //new("ButtonXoa55511", "+"),
                //new("ButtonEdit55512", "+"),
                //new("ButtonXoa55512", "+"),
                //new("ButtonEdit91430", "+"),
                //new("ButtonXoa91430", "+"),
                //new("ButtonEdit55514", "+"),
                //new("ButtonXoa55514", "+"),
                //new("ButtonEdit55515", "+"),
                //new("ButtonXoa55515", "+"),
                //new("ButtonEdit55536", "+"),
                //new("ButtonXoa55536", "+"),
                //new("ButtonEdit55537", "+"),
                //new("ButtonXoa55537", "+"),
                //new("ButtonEdit55538", "+"),
                //new("ButtonXoa55538", "+"),
                //new("ButtonEdit55529", "+"),
                //new("ButtonXoa55529", "+"),
                //new("ButtonEdit55530", "+"),
                //new("ButtonXoa55530", "+"),
                //new("ButtonEdit56294", "+"),
                //new("ButtonXoa56294", "+"),
                //new("ButtonEdit55532", "+"),
                //new("ButtonXoa55532", "+"),
                //new("ButtonEdit55533", "+"),
                //new("ButtonXoa55533", "+"),
                //new("ButtonEdit55534", "+"),
                //new("ButtonXoa55534", "+"),
                //new("ButtonEdit55535", "+"),
                //new("ButtonXoa55535", "+"),
                //new("ButtonEdit55556", "+"),
                //new("ButtonXoa55556", "+"),
                //new("ButtonEdit55557", "+"),
                //new("ButtonXoa55557", "+"),
                //new("ButtonEdit55542", "+"),
                //new("ButtonXoa55542", "+"),
                //new("ButtonEdit55544", "+"),
                //new("ButtonXoa55544", "+"),
                //new("ButtonEdit55545", "+"),
                //new("ButtonXoa55545", "+"),
                //new("ButtonEdit91473", "+"),
                //new("ButtonXoa91473", "+"),
                //new("ButtonEdit98305", "+"),
                //new("ButtonXoa98305", "+"),
                //new("ButtonEdit55547", "+"),
                //new("ButtonXoa55547", "+"),
                //new("ButtonEdit55548", "+"),
                //new("ButtonXoa55548", "+"),
                //new("ButtonEdit55549", "+"),
                //new("ButtonXoa55549", "+"),
                //new("ButtonEdit55550", "+"),
                //new("ButtonXoa55550", "+"),
                //new("ButtonEdit55551", "+"),
                //new("ButtonXoa55551", "+"),
                //new("ButtonEdit55552", "+"),
                //new("ButtonXoa55552", "+"),
                //new("ButtonEdit55553", "+"),
                //new("ButtonXoa55553", "+"),
                //new("ButtonEdit55554", "+"),
                //new("ButtonXoa55554", "+"),
                //new("ButtonEdit98322", "+"),
                //new("ButtonXoa98322", "+"),
                //new("ButtonEdit55555", "+"),
                //new("ButtonXoa55555", "+"),
                //new("ButtonEdit93961", "+"),
                //new("ButtonXoa93961", "+"),
                //new("gvEditing$DXFilterRowMenu", "{&quot;selectedItemIndexPath&quot;:&quot;&quot;,&quot;checkedState&quot;:&quot;&quot;}"),
                //new("gvEditing$DXPagerBottom$PSP", "{&quot;selectedItemIndexPath&quot;:&quot;0&quot;,&quot;checkedState&quot;:&quot;&quot;}"),
                //new("DXMVCEditorsValues", "{\"gvEditing_DXFREditorcol1_DDD_L\":[],\"gvEditing_DXFREditorcol1\":null,\"gvEditing_DXFREditorcol2\":null,\"gvEditing_DXFREditorcol4\":null,\"gvEditing_DXFREditorcol5_DDD_C\":null,\"gvEditing_DXFREditorcol5\":null,\"gvEditing_DXFREditorcol6_DDD_C\":null,\"gvEditing_DXFREditorcol6\":null,\"gvEditing_DXFREditorcol7_DDD_L\":[],\"gvEditing_DXFREditorcol7\":null,\"gvEditing_DXFREditorcol8\":null,\"gvEditing_DXFREditorcol9\":null}"
            };

            try
            {
                var cont = await GetPage(URL_KHOA, prms);
                SetNext(cont);
                return cont;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> DMKhoa_GetNextPage(int page)
        {
            var prms = new List<KeyValuePair<string, string>>()
            {
                new("DXCallbackName", "gvEditing"),
                new("DXCallbackArgument", $"c0:KV|241;{nextPage};CR|2;{{}};GB|20;12|PAGERONCLICK3|PN{page};"),
                new("gvEditing", nextState),
            };

            try
            {
                var cont = await GetPage(URL_KHOA, prms);
                SetNext(cont);
                return cont;
            }
            catch { }

            return null;
        }

        public DataTable DMKhoa_PeakTableCaption(string content)
        {
            int idx1 = content.IndexOf("<table id=\"gvEditing_DXHeaderTable\"");
            int idx2 = content.IndexOf("</table><script", idx1);
            if (idx1 < 0 || idx2 < 0) return null;

            string html = content.Substring(idx1, idx2 - idx1 + 8);
            HTMLDocument doc = HTMLDocument.Parse(html);

            DOMCollection all_head_cells = doc.Body.QuerySelector(".dxgvHeader_EIS");
            return (all_head_cells == null || all_head_cells.Count < 1) ? null : SetCap(new("DMKhoa"), all_head_cells, Field_DMKhoa);
        }

        public bool DMKhoa_PeakContent(string content, DataTable dt)
        {
            if (dt == null) return false;

            int idx1 = content.IndexOf("<table id=\"gvEditing_DXMainTable\"");
            int idx2 = content.IndexOf("</table>", idx1);
            if (idx1 < 0 || idx2 < 0) return false;

            string html = content.Substring(idx1, idx2 - idx1 + 8);
            HTMLDocument doc = HTMLDocument.Parse(html);

            var all_item_rows = doc.Body.QuerySelector(".dxgvDataRow_EIS");
            if (all_item_rows?.Count < 1) return false;

            foreach (var row in all_item_rows)
            {
                var cells = row.QuerySelector("td");
                if (cells?.Count < 1) continue;

                idx1 = -1;
                List<string> values = new();
                foreach (var cel in cells)
                {
                    ++idx1;
                    if (idx1 == 6)
                    {
                        values.Add(cel.QuerySelector(".dxWeb_edtCheckBoxChecked_EIS") != null ? "1" : "");
                    }
                    else if (idx1 < 7)
                    {
                        values.Add(cel.InnerText().Trim());
                    }
                }
                dt.Rows.Add(values.ToArray());
            }

            return true;
        }

        public int DMKhoa_PeakPageCount(string content)
        {
            int idx1 = content.IndexOf("<div class=\"dxpLite_EIS\" id=\"gvEditing_DXPagerBottom\">");
            if (idx1 < 0) return -1;

            idx1 = content.IndexOf("<b class=\"dxp-lead dxp-summary\">Page 1 of ", idx1);
            if (idx1 < 0) return -1;

            idx1 = content.IndexOf(" (", idx1);
            int idx2 = content.IndexOf(" items)", idx1);
            if (idx2 < 0) return -1;

            string s = content.Substring(idx1 + 2, idx2 - idx1 - 2).Trim();
            if (int.TryParse(s, out int res))
            {
                return res / 30 + (res % 30 > 0 ? 1 : 0);
            }
            return -1;
        }
        #endregion
    }
}