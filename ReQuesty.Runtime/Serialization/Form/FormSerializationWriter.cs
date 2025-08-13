using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Xml;
using ReQuesty.Runtime.Abstractions;
using ReQuesty.Runtime.Helpers;
using ReQuesty.Runtime.Abstractions.Serialization;

namespace ReQuesty.Runtime.Serialization.Form;

/// <summary>
///   Represents a serialization writer that can be used to write a form url encoded string.
/// </summary>
public class FormSerializationWriter : ISerializationWriter
{
    private int depth;

    private readonly StringBuilder _builder = new();

    /// <inheritdoc/>
    public Action<IParsable>? OnBeforeObjectSerialization { get; set; }

    /// <inheritdoc/>
    public Action<IParsable>? OnAfterObjectSerialization { get; set; }

    /// <inheritdoc/>
    public Action<IParsable, ISerializationWriter>? OnStartObjectSerialization { get; set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public Stream GetSerializedContent() => new MemoryStream(Encoding.UTF8.GetBytes(_builder.ToString()));

    /// <inheritdoc/>
    public void WriteAdditionalData(IDictionary<string, object> value)
    {
        if (value == null)
        {
            return;
        }

        foreach (KeyValuePair<string, object> dataValue in value)
        {
            WriteAnyValue(dataValue.Key, dataValue.Value);
        }
    }

    private void WriteAnyValue(string? key, object value)
    {
        switch (value)
        {
            case null:
                WriteNullValue(key);
                break;
            case decimal d:
                WriteDecimalValue(key, d);
                break;
            case bool b:
                WriteBoolValue(key, b);
                break;
            case byte b:
                WriteByteValue(key, b);
                break;
            case sbyte b:
                WriteSbyteValue(key, b);
                break;
            case int i:
                WriteIntValue(key, i);
                break;
            case float f:
                WriteFloatValue(key, f);
                break;
            case long l:
                WriteLongValue(key, l);
                break;
            case double d:
                WriteDoubleValue(key, d);
                break;
            case Guid g:
                WriteGuidValue(key, g);
                break;
            case DateTimeOffset dto:
                WriteDateTimeOffsetValue(key, dto);
                break;
            case TimeSpan timeSpan:
                WriteTimeSpanValue(key, timeSpan);
                break;
            case Time time:
                WriteTimeValue(key, time);
                break;
            case IEnumerable<object> coll:
                WriteCollectionOfPrimitiveValues(key, coll);
                break;
            case byte[] coll:
                WriteByteArrayValue(key, coll);
                break;
            case IParsable:
                throw new InvalidOperationException("Form serialization does not support nested objects.");
            default:
                WriteStringValue(key, value.ToString());// works for Date and String types
                break;
        }
    }

    /// <inheritdoc/>
    public void WriteBoolValue(string? key, bool? value)
    {
        if (value.HasValue)
        {
            WriteStringValue(key, value.Value.ToString().ToLowerInvariant());
        }
    }

    /// <inheritdoc/>
    public void WriteByteArrayValue(string? key, byte[]? value)
    {
        if (value != null)//empty array is meaningful
        {
            WriteStringValue(key, value.Length > 0 ? Convert.ToBase64String(value) : string.Empty);
        }
    }

    /// <inheritdoc/>
    public void WriteByteValue(string? key, byte? value)
    {
        if (value.HasValue)
        {
            WriteIntValue(key, Convert.ToInt32(value.Value));
        }
    }

    /// <inheritdoc/>
    public void WriteCollectionOfObjectValues<T>(string? key, IEnumerable<T>? values) where T : IParsable => throw new InvalidOperationException("Form serialization does not support collections.");

    /// <inheritdoc/>
    public void WriteCollectionOfPrimitiveValues<T>(string? key, IEnumerable<T>? values)
    {
        if (values == null)
        {
            return;
        }

        foreach (T? value in values)
        {
            if (value != null)
            {
                WriteAnyValue(key, value);
            }
        }
    }

    /// <inheritdoc/>
    public void WriteDateTimeOffsetValue(string? key, DateTimeOffset? value)
    {
        if (value.HasValue)
        {
            WriteStringValue(key, value.Value.ToString("o", CultureInfo.InvariantCulture));
        }
    }

    /// <inheritdoc/>
    public void WriteDateValue(string? key, Date? value)
    {
        if (value.HasValue)
        {
            WriteStringValue(key, value.Value.ToString());
        }
    }

    /// <inheritdoc/>
    public void WriteDecimalValue(string? key, decimal? value)
    {
        if (value.HasValue)
        {
            WriteStringValue(key, value.Value.ToString(CultureInfo.InvariantCulture));
        }
    }

    /// <inheritdoc/>
    public void WriteDoubleValue(string? key, double? value)
    {
        if (value.HasValue)
        {
            WriteStringValue(key, value.Value.ToString(CultureInfo.InvariantCulture));
        }
    }

    /// <inheritdoc/>
    public void WriteFloatValue(string? key, float? value)
    {
        if (value.HasValue)
        {
            WriteStringValue(key, value.Value.ToString(CultureInfo.InvariantCulture));
        }
    }

    /// <inheritdoc/>
    public void WriteGuidValue(string? key, Guid? value)
    {
        if (value.HasValue)
        {
            WriteStringValue(key, value.Value.ToString("D"));
        }
    }

    /// <inheritdoc/>
    public void WriteIntValue(string? key, int? value)
    {
        if (value.HasValue)
        {
            WriteStringValue(key, value.Value.ToString(CultureInfo.InvariantCulture));
        }
    }

    /// <inheritdoc/>
    public void WriteLongValue(string? key, long? value)
    {
        if (value.HasValue)
        {
            WriteStringValue(key, value.Value.ToString(CultureInfo.InvariantCulture));
        }
    }

    /// <inheritdoc/>
    public void WriteNullValue(string? key)
    {
        WriteStringValue(key, "null");
    }

    /// <inheritdoc/>
    public void WriteObjectValue<T>(string? key, T? value, params IParsable?[] additionalValuesToMerge) where T : IParsable
    {
        if (depth > 0)
        {
            throw new InvalidOperationException("Form serialization does not support nested objects.");
        }

        depth++;
        if (value == null && !Array.Exists(additionalValuesToMerge, static x => x is not null))
        {
            return;
        }

        if (value != null)
        {
            OnBeforeObjectSerialization?.Invoke(value);
            OnStartObjectSerialization?.Invoke(value, this);
            value.Serialize(this);
        }
        foreach (IParsable? additionalValueToMerge in additionalValuesToMerge)
        {
            if (additionalValueToMerge is null)
            {
                continue;
            }

            OnBeforeObjectSerialization?.Invoke(additionalValueToMerge);
            OnStartObjectSerialization?.Invoke(additionalValueToMerge, this);
            additionalValueToMerge.Serialize(this);
            OnAfterObjectSerialization?.Invoke(additionalValueToMerge);
        }
        if (value != null)
        {
            OnAfterObjectSerialization?.Invoke(value);
        }
    }

    /// <inheritdoc/>
    public void WriteSbyteValue(string? key, sbyte? value)
    {
        if (value.HasValue)
        {
            WriteIntValue(key, Convert.ToInt32(value.Value));
        }
    }

    /// <inheritdoc/>
    public void WriteStringValue(string? key, string? value)
    {
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
        {
            return;
        }

        if (_builder.Length > 0)
        {
            _builder.Append('&');
        }

        _builder.Append(Uri.EscapeDataString(key)).Append('=').Append(Uri.EscapeDataString(value));
    }

    /// <inheritdoc/>
    public void WriteTimeSpanValue(string? key, TimeSpan? value)
    {
        if (value.HasValue)
        {
            WriteStringValue(key, XmlConvert.ToString(value.Value));
        }
    }

    /// <inheritdoc/>
    public void WriteTimeValue(string? key, Time? value)
    {
        if (value.HasValue)
        {
            WriteStringValue(key, value.Value.ToString());
        }
    }

    /// <inheritdoc/>
    public void WriteCollectionOfEnumValues<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>(string? key, IEnumerable<T?>? values) where T : struct, Enum
    {
        if (values == null)
        {
            return;
        }

        StringBuilder? valueNames = null;
        foreach (T? x in values)
        {
            if (x.HasValue && EnumHelpers.GetEnumStringValue(x.Value) is string valueName)
            {
                if (valueNames == null)
                {
                    valueNames = new StringBuilder();
                }
                else
                {
                    valueNames.Append(',');
                }

                valueNames.Append(valueName);
            }
        }

        if (valueNames is not null)
        {
            WriteStringValue(key, valueNames.ToString());
        }
    }

    /// <inheritdoc/>
    public void WriteEnumValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>(string? key, T? value) where T : struct, Enum
    {
        if (value.HasValue)
        {
            if (typeof(T).IsDefined(typeof(FlagsAttribute)))
            {
                T[] values = Enum.GetValues<T>();

                StringBuilder valueNames = new();
                foreach (T x in values)
                {
                    if (value.Value.HasFlag(x) && EnumHelpers.GetEnumStringValue(x) is string valueName)
                    {
                        if (valueNames.Length > 0)
                        {
                            valueNames.Append(',');
                        }

                        valueNames.Append(valueName);
                    }
                }
                WriteStringValue(key, valueNames.ToString());
            }
            else
            {
                WriteStringValue(key, EnumHelpers.GetEnumStringValue(value.Value));
            }
        }
    }
}
