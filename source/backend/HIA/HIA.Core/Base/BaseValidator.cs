using System.Linq.Expressions;
using System.Reflection;
using HIA.Data.Base;
using HIA.Data.Records;
using Microsoft.Extensions.Logging;

namespace HIA.Core.Base;

public class BaseValidator<T>
    where T : class, IRecord
{
    private readonly Dictionary<string, object> _evals = new();
    private readonly Dictionary<int, string> _errors;
    private readonly ILogger<dynamic>? _logger;

    public T File { get; private set; } = null!;

    public string MsgPrefix { get; set; } = string.Empty;

    public BaseValidator(ILogger<dynamic>? logger)
    {
        _errors = new();
        _logger = logger;
    }

    public BaseValidator(Dictionary<int, string> errors, ILogger<dynamic>? logger)
    {
        _errors = errors;
        _logger = logger;
    }

    private static string GetPropName<TField>(Expression<Func<T, TField>> predicate)
    {
        try
        {
            if (predicate.Body is MemberExpression member)
            {
                if (member.Member is PropertyInfo prop)
                {
                    Type type = typeof(TField);
                    if (prop.ReflectedType != null && (type == prop.PropertyType || type.IsSubclassOf(prop.PropertyType))) return prop.Name;
                }
            }
        }
        catch { }

        return "";
    }

    public bool HasError(int error)
        => _errors?.ContainsKey(error) == true;

    public void AddNewLog(string message)
        => Uti.HandleCronJobLog(_logger!, message, $"[VALIDATE130] {DateTime.Now}");

    public void RaiseError(int error, string? desc = null)
    {
        if (string.IsNullOrEmpty(MsgPrefix) && (_errors == null || _errors.Count == 0))
        {
            File.RaiseError(error, desc);
            return;
        }

        desc = string.IsNullOrEmpty(desc) ? string.Empty : $"Chi tiết: {desc}";
        if (_errors!.TryGetValue(error, out string? message))
        {
            File.RaiseError(error, $"{MsgPrefix}Nội dung lỗi:{error}|{message}\n{desc}");
        }
        else
        {
            File.RaiseError(error, $"{MsgPrefix}Nội dung lỗi:{error}|Không tìm thấy nội dung lỗi\n{desc}");
        }
    }

    public bool RaiseError(bool eval, int error, string? desc = null)
    {
        if (eval) RaiseError(error, desc);
        return eval;
    }

    public bool RaiseError<TXml>(TXml xml, bool eval, int error, string? desc = null)
        where TXml : IRecord<XML01>
    {
        if (eval) xml.RaiseError(error, desc);
        return eval;
    }

    public void RaiseError<TXml>(TXml xml, int error, string? desc = null)
        where TXml : IRecord<XML01>
    {
        if (string.IsNullOrEmpty(MsgPrefix) && (_errors == null || _errors.Count == 0))
        {
            xml.RaiseError(error, desc);
            return;
        }

        desc = string.IsNullOrEmpty(desc) ? string.Empty : $"Chi tiết: {desc}";
        if (_errors!.TryGetValue(error, out string? message))
        {
            xml.RaiseError(error, $"{MsgPrefix}Nội dung lỗi:{error}|{message}\n{desc}");
        }
        else
        {
            xml.RaiseError(error, $"{MsgPrefix}Nội dung lỗi:{error}|Không tìm thấy nội dung lỗi\n{desc}");
        }
    }

    public Evaluation<T, TField> Field<TField>(Expression<Func<T, TField>> predicate)
    {
        var prop = GetPropName(predicate);
        Evaluation<T, TField>? eval;
        if (!string.IsNullOrEmpty(prop) && _evals.TryGetValue(prop, out object? value))
        {
            eval = value as Evaluation<T, TField>;
            if (eval != null)
            {
                eval.Result();
                return eval;
            }
        }

        eval = new Evaluation<T, TField>(File, predicate);
        _evals[prop] = eval;
        return eval;
    }

    public virtual Task<bool> Validate(T xml)
    {
        _evals.Clear();
        File = xml;
        return Task.FromResult(File != null);
    }

    public virtual void WriteLog(string message, string cronJobName)
    {
        if (_logger != null) Uti.HandleCronJobLog(_logger, message, cronJobName);
    }
}
