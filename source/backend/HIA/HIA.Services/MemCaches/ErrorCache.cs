using System.Collections.Concurrent;
using HIA.Data.Rules;

namespace HIA.Services.MemCaches;

public static class ErrorCache
{
    private static readonly object _lock = new();
    private static readonly ConcurrentDictionary<int, DMError130> _errors = new();

    public static void Import(IEnumerable<DMError130> errLsts)
    {
        lock (_lock)
        {
            _errors.Clear();
            foreach (var e in errLsts)
            {
                if (e.ErrorCode == null || e.ErrorCode < 1 || e.Message == null) continue;
                _errors.TryAdd(e.ErrorCode.Value, e);
            }
        }
    }

    public static bool Contains(int err)
        => err > 0 && _errors.ContainsKey(err);

    public static string GetMessage(int err)
    {
        var msg = err > 0 && _errors.TryGetValue(err, out var dm) ? dm.Message : null;
        return msg ?? "Không tìm thấy mã lỗi";
    }

    public static List<DMError130> GetList(params int[] errs)
    {
        return _errors.Values.Where(e => errs.Contains(e.ErrorCode ?? 0)).ToList();
    }
}
