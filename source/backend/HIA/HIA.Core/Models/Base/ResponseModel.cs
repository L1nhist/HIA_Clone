using System.Net;

namespace HIA.Core.Models.Base;

public class ResponseModel<T>
{
    public HttpStatusCode Code { get; set; }
    public string? Message { get; set; }

    public T? Data { get; set; }

    public ResponseModel()
    {
    }

    public ResponseModel(HttpStatusCode code, string? mes, T? data)
    {
        Code = code;
        Message = mes;
        Data = data;
    }
}
