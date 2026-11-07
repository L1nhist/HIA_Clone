using System.Net.Mime;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using HIA.Core.Languages.DisplayField;

namespace HIA.Core.Base;

public class Constant
{
    #region Authentication
    public const string AUTH_SIGN_IN_FUNC = "SignIn";
    public const string AUTH_SIGN_IN_API_FUNC = "SignIn API";
    public const string AUTH_SIGN_OUT_FUNC = "SignOut";
    public const string AUTH_SIGN_UP_FUNC = "SignUp";
    public const string AUTH_FORGOT_PASS_FUNC = "Forgot Password";
    public const string AUTH_CHANGE_PASS_NEW_USER_FUNC = "Change Password For New User";
    #endregion

    #region Hospital Management
    public const string HOSPITAL_GET_FILTER_LIST = "Get hospital list by filter";
    public const string HOSPITAL_GET_DETAIL = "Get hospital detail";
    public const string HOSPITAL_CREATE_NEW = "Create new hospital";
    public const string HOSPITAL_UPDATE = "Update hospital";
    public const string HOSPITAL_UPDATE_STATUS = "Update hospital status";
    public const string HOSPITAL_SOFT_DELETE = "Soft Delete hospital";
    public const string HOSPITAL_HARD_DELETE = "Hard Delete hospital";
    #endregion
}

public static class RegexValidation
{
    public const string REGEX_EMAIL = "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+.[a-zA-Z]{2,4}$";
    public const string REGEX_PASSWORD = "^.{6,}$";
}

public enum TypeUpdatePass
{
    NEW_USER = 1,
    RESET = 2
}

public static class FileTypeConstant
{
    public const string XLSX_TYPE = "vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public const string XLS_TYPE = "application/vnd.ms-excel;charset=utf-8";
    public const string XML_TYPE = MediaTypeNames.Text.Xml;
}

public static class DataTableSettingDefault
{
    // Giu nguyen ten thuoc tinh (sEmptyTable, oPaginate...): khong dung LowerCaseNamingPolicy
    private static readonly JsonSerializerOptions _json = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    internal static JsonSerializerOptions Json => _json;

    public static string LENGTH_MENU = JsonSerializer.Serialize(new List<List<int>> { new List<int> { 200, 500, 1000 }, new List<int> { 200, 500, 1000 } }, _json);
    public static string O_LANGUAGE = JsonSerializer.Serialize(new
    {
        sEmptyTable = DisplayFieldResource.DataTable_EMPTY_TABLE,
        sLengthMenu = $"{DisplayFieldResource.DataTable_SHOW} _MENU_ {DisplayFieldResource.DataTable_ENTRIES}",
        sSearch = DisplayFieldResource.DataTable_SEARCH,
        sInfo = $"{DisplayFieldResource.DataTable_SHOWING} _START_ {DisplayFieldResource.DataTable_TO} _END_ {DisplayFieldResource.DataTable_OF} _TOTAL_ {DisplayFieldResource.DataTable_ENTRIES}",
        sInfoEmpty = $"{DisplayFieldResource.DataTable_SHOWING}  0 {DisplayFieldResource.DataTable_TO} 0 {DisplayFieldResource.DataTable_OF} 0 {DisplayFieldResource.DataTable_ENTRIES}",
        sLoadingRecords = DisplayFieldResource.DataTable_LOADING,
        sProcessing = DisplayFieldResource.DataTable_PROCESSING,
        oPaginate = new
        {
            sFirst = DisplayFieldResource.DataTable_FIRST,
            sLast = DisplayFieldResource.DataTable_LAST,
            sNext = DisplayFieldResource.DataTable_NEXT,
            sPrevious = DisplayFieldResource.DataTable_PREVIOUS
        }
    }, _json);
}

public class DataTableLanguage
{
    public string SEmptyTable { get; set; } = string.Empty;
}

public static class DropifySettingDefault
{
    public static string MESSAGES = JsonSerializer.Serialize(new DropifyMessage
    {
        Default = DisplayFieldResource.TITLE_UPLOAD_CATEGORY,
        replace = DisplayFieldResource.TITLE_UPLOAD_REPLACE_CATEGORY,
        remove = DisplayFieldResource.DELETE,
        error = DisplayFieldResource.TITLE_UPLOAD_ERROR_CATEGORY
    }, DataTableSettingDefault.Json);
    public static string MESSAGES_XML = JsonSerializer.Serialize(new DropifyMessage
    {
        Default = DisplayFieldResource.TITLE_UPLOAD_XML_CATEGORY,
        replace = DisplayFieldResource.TITLE_UPLOAD_REPLACE_CATEGORY,
        remove = DisplayFieldResource.DELETE,
        error = DisplayFieldResource.TITLE_UPLOAD_ERROR_CATEGORY
    }, DataTableSettingDefault.Json);
    public static string ERROR = JsonSerializer.Serialize(new
    {
        fileSize = DisplayFieldResource.TITLE_UPLOAD_ERROR_FILE_SIZE,
        minWidth = DisplayFieldResource.TITLE_UPLOAD_ERROR_MIN_WIDTH,
        maxWidth = DisplayFieldResource.TITLE_UPLOAD_ERROR_MAX_WIDTH,
        minHeight = DisplayFieldResource.TITLE_UPLOAD_ERROR_MIN_HEIGHT,
        maxHeight = DisplayFieldResource.TITLE_UPLOAD_ERROR_MAX_HEIGHT,
        fileFormat = DisplayFieldResource.TITLE_UPLOAD_ERROR_FORMAT,
    }, DataTableSettingDefault.Json);
}

public static class FileStatusVerification
{
    public const int NOT_VERIFIED = 0;
    public const int VERIFIED = 1;
    public const int EXTERNAL_API_HAS_PROBLEM = 2;
    public const int DUPLICATE_VERIFIED_NAME = 3;
    public const int RECALL_VALIDATE_CHECK = 99;
    public static List<int> NEED_VERIFIED_LIST = new List<int> { NOT_VERIFIED, EXTERNAL_API_HAS_PROBLEM };
    public static List<int> IGNORE_VERIFIED_LIST = new List<int> { VERIFIED, DUPLICATE_VERIFIED_NAME, RECALL_VALIDATE_CHECK };
}

public class DropifyMessage
{
    [JsonPropertyName("default")]
    public string Default { get; set; } = string.Empty;
    public string replace { get; set; } = string.Empty;
    public string remove { get; set; } = string.Empty;
    public string error { get; set; } = string.Empty;
}

//Linhist added 06/07/22
public static class ErrorMessageType
{
    public const int DATA_FORMAT = 1;
    public const int API_VERIFIED = 2;
    public const int HDSD_THUOC = 3;
    public const int HDSD_DICHVU = 4;
    public const int PHACDO_CHIDINH = 5;
}
