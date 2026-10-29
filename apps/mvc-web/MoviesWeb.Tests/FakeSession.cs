using System.Text;
using Microsoft.AspNetCore.Http;

namespace MoviesWeb.Tests;

/// <summary>
/// A minimal in-memory session, so the tests do not need a server.
/// </summary>
internal sealed class FakeSession : ISession
{
    private readonly Dictionary<string, byte[]> _values = new();

    public bool IsAvailable => true;

    public string Id => "fake-session";

    public IEnumerable<string> Keys => _values.Keys;

    public void Clear() => _values.Clear();

    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public void Remove(string key) => _values.Remove(key);

    public void Set(string key, byte[] value) => _values[key] = value;

    public bool TryGetValue(string key, out byte[] value) => _values.TryGetValue(key, out value!);

    public string? GetString(string key) =>
        TryGetValue(key, out var value) ? Encoding.UTF8.GetString(value) : null;

    public void SetString(string key, string value) => Set(key, Encoding.UTF8.GetBytes(value));
}
