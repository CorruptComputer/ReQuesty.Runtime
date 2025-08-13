using System.Collections;

namespace ReQuesty.Runtime.Abstractions;

/// <summary>Represents a collection of request headers.</summary>
public class RequestHeaders : IDictionary<string, IEnumerable<string>>
{
    private readonly Dictionary<string, HashSet<string>> _headers = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _singleValueHeaders = new(StringComparer.OrdinalIgnoreCase) {
        "Content-Type",
        "Content-Encoding",
        "Content-Length"
    };
    /// <summary>
    /// Adds values to the header with the specified name.
    /// </summary>
    /// <param name="headerName">The name of the header to add values to.</param>
    /// <param name="headerValues">The values to add to the header.</param>
    public void Add(string headerName, params string[] headerValues)
    {
        if (string.IsNullOrEmpty(headerName))
        {
            throw new ArgumentNullException(nameof(headerName));
        }

        ArgumentNullException.ThrowIfNull(headerValues);
        if (headerValues.Length == 0)
        {
            return;
        }

        if (_singleValueHeaders.Contains(headerName))
        {
            _headers[headerName] = [headerValues[0]];
        }
        else if (_headers.TryGetValue(headerName, out HashSet<string>? values))
        {
            foreach (string headerValue in headerValues)
            {
                values.Add(headerValue);
            }
        }
        else
        {
            _headers.Add(headerName, new HashSet<string>(headerValues));
        }
    }

    /// <summary>
    /// Adds values to the header with the specified name if it's not already present
    /// </summary>
    /// <param name="headerName">The name of the header to add values to.</param>
    /// <param name="headerValue">The values to add to the header.</param>
    /// <returns>If the headerValue have been added to the Dictionary.</returns>
    public bool TryAdd(string headerName, string headerValue)
    {
        if (string.IsNullOrEmpty(headerName))
        {
            throw new ArgumentNullException(nameof(headerName));
        }

        ArgumentNullException.ThrowIfNull(headerValue);
        if (!_headers.ContainsKey(headerName))
        {
            _headers.Add(headerName, [headerValue]);
            return true;
        }
        return false;
    }

    /// <inheritdoc/>
    public ICollection<string> Keys => _headers.Keys;

    /// <inheritdoc/>
    public ICollection<IEnumerable<string>> Values
    {
        get
        {
            List<IEnumerable<string>> values = [.. _headers.Values];
            return values;
        }
    }

    /// <inheritdoc/>
    public int Count => _headers.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    public IEnumerable<string> this[string key] { get => TryGetValue(key, out IEnumerable<string>? result) ? result : throw new KeyNotFoundException($"Key not found : {key}"); set => Add(key, value); }

    /// <summary>
    /// Removes the specified value from the header with the specified name.
    /// </summary>
    /// <param name="headerName">The name of the header to remove the value from.</param>
    /// <param name="headerValue">The value to remove from the header.</param>
    public bool Remove(string headerName, string headerValue)
    {
        if (string.IsNullOrEmpty(headerName))
        {
            throw new ArgumentNullException(nameof(headerName));
        }

        ArgumentNullException.ThrowIfNull(headerValue);
        if (_headers.TryGetValue(headerName, out HashSet<string>? values))
        {
            bool result = values.Remove(headerValue);
            if (values.Count == 0)
            {
                _headers.Remove(headerName);
            }

            return result;
        }
        return false;
    }

    /// <summary>
    /// Adds all the headers values from the specified headers collection.
    /// </summary>
    /// <param name="headers">The headers to update the current headers with.</param>
    public void AddAll(RequestHeaders headers)
    {
        ArgumentNullException.ThrowIfNull(headers);
        foreach (KeyValuePair<string, IEnumerable<string>> header in headers)
        {
            foreach (string value in header.Value)
            {
                Add(header.Key, value);
            }
        }
    }

    /// <summary>
    /// Removes all headers.
    /// </summary>
    public void Clear()
    {
        _headers.Clear();
    }

    /// <inheritdoc/>
    public bool ContainsKey(string key) => !string.IsNullOrEmpty(key) && _headers.ContainsKey(key);

    /// <inheritdoc/>
    public void Add(string key, IEnumerable<string> value)
    {
        if (value == null)
        {
            Add(key, null!);
        }
        else
        {
            List<string> valueArray = [.. value];
            Add(key, valueArray.ToArray());
        }
    }

    /// <inheritdoc/>
    public bool Remove(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentNullException(nameof(key));
        }

        return _headers.Remove(key);
    }
    /// <inheritdoc/>
    public bool TryGetValue(string key, out IEnumerable<string> value)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (_headers.TryGetValue(key, out HashSet<string>? values))
        {
            value = values;
            return true;
        }
        value = [];
        return false;
    }

    /// <inheritdoc/>
    public void Add(KeyValuePair<string, IEnumerable<string>> item) => Add(item.Key, item.Value);

    /// <inheritdoc/>
    public bool Contains(KeyValuePair<string, IEnumerable<string>> item)
    {
        if (!TryGetValue(item.Key, out IEnumerable<string>? values))
        {
            return false;
        }

        List<string> itemValueList = new(item.Value);

        int valuesCount = 0;
        foreach (string value in values)
        {
            valuesCount++;
            if (!itemValueList.Contains(value))
            {
                return false;
            }
        }

        return valuesCount == itemValueList.Count;
    }

    /// <inheritdoc/>
    public void CopyTo(KeyValuePair<string, IEnumerable<string>>[] array, int arrayIndex) => throw new NotImplementedException();

    /// <inheritdoc/>
    public bool Remove(KeyValuePair<string, IEnumerable<string>> item)
    {
        bool result = false;
        foreach (string value in item.Value)
        {
            result |= Remove(item.Key, value);
        }

        return result;
    }
    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, IEnumerable<string>>> GetEnumerator() => new RequestHeadersEnumerator(_headers.GetEnumerator());
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    private sealed class RequestHeadersEnumerator(IEnumerator enumerator) : IEnumerator<KeyValuePair<string, IEnumerable<string>>>
    {
        private readonly IEnumerator _enumerator = enumerator;

        public KeyValuePair<string, IEnumerable<string>> Current => _enumerator.Current is KeyValuePair<string, HashSet<string>> current ? new(current.Key, current.Value) : throw new InvalidOperationException();

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            (_enumerator as IDisposable)?.Dispose();
            GC.SuppressFinalize(this);
        }
        public bool MoveNext() => _enumerator.MoveNext();
        public void Reset() => _enumerator.Reset();
    }
}
