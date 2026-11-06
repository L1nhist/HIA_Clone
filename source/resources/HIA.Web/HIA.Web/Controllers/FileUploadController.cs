//using HIA.Core.Models.FileUploads;
//using HIA.Core.Models.FileUploads130;
//using HIA.DatabaseModel.Entites;
//using HIA.Services.HosoManagement;
//using HIA.Services.HospitalManagement;
//using HIA.Services.RoleManagement;
//using HIA.Services.UserManagement;
//using HIA.Web.Helper;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using HIA.Core.Models.Token;
//using System.Globalization;
//using HIA.Core.Models.Base;
//using System.Xml;
//using Microsoft.AspNetCore.Authorization;
//using HIA.Core.Base;
//using System.Text.Encodings;
//using HIA.Services.Files;

//namespace HIA.Web.Controllers
//{
//    [Authorize]
//    public class FileUploadController : Controller
//    {
//        private readonly IUserManagementService _userManagementService;
//        private readonly IRoleService _roleService;
//        private readonly IHospitalService _hospitalService;
//        private readonly IHoSoManagementService _hosoService;
//        private readonly IHttpContextAccessor _httpContext;
//        private readonly IFilesService _FilesService;
//        private readonly IFilesService130 _FilesService130;


//        //private string fullPath = System.AppDomain.CurrentDomain.BaseDirectory.ToString() + "UploadImages";
//        private string fullPath = "E:/";
//        //haimaster 3/12/2021 GetEncoding
//        public static Encoding GetEncodingHT(byte[] bom)
//        {
//           // Analyze the BOM
            
//            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
//            if (bom[0] == 0xff && bom[1] == 0xfe && bom[2] == 0 && bom[3] == 0) return Encoding.UTF32; //UTF-32LE
//            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
//            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
//            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return new UTF32Encoding(true, true);  //UTF-32BE

//            // We actually have no idea what the encoding is if we reach this point, so
//            // you may wish to return null instead of defaulting to ASCII
//            return Encoding.ASCII;
//        }


//        public FileUploadController(IUserManagementService userManagementService,
//            IRoleService roleService,
//            IHospitalService hospitalService,
//            IHoSoManagementService hosoService,
//            IHttpContextAccessor httpContext,
//            IFilesService FilesService,
//            IFilesService130 filesService130)
//        {
//            _userManagementService = userManagementService;
//            _roleService = roleService;
//            _hospitalService = hospitalService;
//            _hosoService = hosoService;
//            _httpContext = httpContext;
//            _FilesService = FilesService;
//            _FilesService130 = filesService130;
//        }
//        public IActionResult Index()
//        {
//            return View();
//        }

//        [HttpPost]
//        public async Task<IActionResult> Index(Core.Models.FileUploads.FileUpload fileUpload)
//        {
            
//            return Json(await _FilesService.FileUploadAPI(fileUpload));
//            //return Json(await _FilesService130.FileUploadAPI(fileUpload));
            
//        }

//        static DateTime GetDTCTime(ulong nanoseconds, ulong ticksPerNanosecond)
//        {
//            DateTime pointOfReference = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
//            long ticks = (long)(nanoseconds / ticksPerNanosecond);
//            return pointOfReference.AddTicks(ticks);
//        }

//        static DateTime GetDTCTime(ulong nanoseconds)
//        {
//            return GetDTCTime(nanoseconds, 100);
//        }
//        static string GetXMLErrorAtRow(string xml, Exception e)
//        {
//            try
//            {
//                int result;
//                int.TryParse(e.Message.Substring(35, 2), out result);
//                //int possition = int.Parse(e.Message.Substring(37, 2));
//                string error = "Lỗi dòng " + result.ToString();// + "   Vị trí lỗi: " + possition.ToString() + "     ";
//                List<string> foundValues4 = new List<string>();
//                XmlTextReader xmlReader = new XmlTextReader(new System.IO.StringReader(xml.Substring(xml.IndexOf(Environment.NewLine))));
//                while (xmlReader.Read())
//                {
//                    if (xmlReader.LineNumber == result)
//                    {
//                        switch (xmlReader.NodeType)
//                        {

//                            case XmlNodeType.Element:
//                                error += " Name: " + xmlReader.Name;
//                                break;
//                            case XmlNodeType.Text:
//                                error += " Value: " + xmlReader.Value;
//                                break;
//                            case XmlNodeType.EndElement:
//                                foundValues4.Add("");
//                                break;
//                        }
//                    }
//                }
//                // Close the reader.
//                xmlReader.Close();
//                return error;
//            }
//            catch
//            {
//                return "Lỗi đọc file.";
//            }
//        }



//        public static System.Text.Encoding GetType(string FILE_NAME)
//        {
//            FileStream fs = new FileStream(FILE_NAME, FileMode.Open, FileAccess.Read);
//            Encoding r = GetType(fs);
//            fs.Close();
//            return r;
//        }

//        public static System.Text.Encoding GetType(FileStream fs)
//        {
//            byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
//            byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
//            byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; //with BOM
//            Encoding reVal = Encoding.Default;

//            BinaryReader r = new BinaryReader(fs, System.Text.Encoding.Default);
//            int i;
//            int.TryParse(fs.Length.ToString(), out i);
//            byte[] ss = r.ReadBytes(i);
//            if (IsUTF8Bytes(ss) || (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF))
//            {
//                reVal = Encoding.UTF8;
//            }
//            else if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
//            {
//                reVal = Encoding.BigEndianUnicode;
//            }
//            else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41)
//            {
//                reVal = Encoding.Unicode;
//            }
//            r.Close();
//            return reVal;

//        }
//        static void Write(Stream s, Byte[] bytes)
//        {
//            using (var writer = new BinaryWriter(s))
//            {
//                writer.Write(bytes);
//            }
//        }
//        private static bool IsUTF8Bytes(byte[] data)
//        {
//            int charByteCounter = 1;
//            byte curByte;
//            for (int i = 0; i < data.Length; i++)
//            {
//                curByte = data[i];
//                if (charByteCounter == 1)
//                {
//                    if (curByte >= 0x80)
//                    {
//                        while (((curByte <<= 1) & 0x80) != 0)
//                        {
//                            charByteCounter++;
//                        }

//                        if (charByteCounter == 1 || charByteCounter > 6)
//                        {
//                            return false;
//                        }
//                    }
//                }
//                else
//                {
//                    if ((curByte & 0xC0) != 0x80)
//                    {
//                        return false;
//                    }
//                    charByteCounter--;
//                }
//            }
//            if (charByteCounter > 1)
//            {
//                throw new Exception("Error byte format");
//            }
//            return true;
//        }
//    }
//}

