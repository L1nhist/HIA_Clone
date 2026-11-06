using HIA.Core.Base;
using HIA.Core.Models.Base;
using Microsoft.Extensions.Logging;

namespace HIA.Services.Categories;

internal static class CategoryResponses
{
    /// <summary>Logs the exception and returns a 500 response with no data.</summary>
    public static ResponseModel<T> InternalError<T>(Exception ex, ILogger<dynamic> logger, string serviceName)
    {
        var error = Uti.HandleInternalServerError(ex, false, logger, serviceName);
        return new ResponseModel<T>(error.Code, error.Message, default);
    }
}
