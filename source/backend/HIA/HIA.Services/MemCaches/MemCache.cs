using System.Collections.Concurrent;

namespace HIA.Services.MemCaches;

public class MemCacheEntry<T>
{
    public T Value { get; }

    public DateTime Timestamp { get; set; }

    public MemCacheEntry(T value, DateTime timestamp)
    {
        Value = value;
        Timestamp = timestamp;
    }
}

public class MemCacheDictionary<TKey, TValue> where TKey : notnull
{
    private readonly int _timeout;
    private readonly ConcurrentDictionary<TKey, MemCacheEntry<TValue>> _caches = new();

    public MemCacheDictionary(int timeout = 300) //defaut: 300 seconds = 5 minutes
    {
        _timeout = timeout;
    }

    public bool Append(TKey key, TValue value)
        => _caches.TryAdd(key, new MemCacheEntry<TValue>(value, DateTime.UtcNow));

    public bool Contains(TKey key)
        => _caches.ContainsKey(key);

    public bool Cleanup()
    {
        var now = DateTime.UtcNow;
        var rems = _caches.Where(e => (now - e.Value.Timestamp).TotalSeconds > _timeout).Select(e => e.Key).ToList();
        foreach (var e in rems)
        {
            _caches.TryRemove(e, out _);
        }

        return rems.Count > 0;
    }

    public bool TryGet(TKey key, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out TValue value)
    {
        if (_caches.TryGetValue(key, out var entry))
        {
            value = entry.Value;
            return true;
        }

        value = default;
        return false;
    }

    public void Remove(TKey key)
        => _caches.TryRemove(key, out _);
}
