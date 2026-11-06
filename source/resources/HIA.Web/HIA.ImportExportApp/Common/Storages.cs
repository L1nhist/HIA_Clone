using HIA.Core.Models.Token;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIA.ImportExportApp.Common
{
    internal static class Storages
    {
        public static TokenRequestModel? CurrentUser { get; set; }
        public static string? UploadPath { get; set; }
        public static string? DownloadPath { get; set; }

        public static FileInfo[]? FilesNameHandleProccessing { get; set; }
    }
}
