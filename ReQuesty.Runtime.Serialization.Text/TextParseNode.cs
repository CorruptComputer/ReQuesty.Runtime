using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml;
using ReQuesty.Runtime.Abstractions;
using ReQuesty.Runtime.Abstractions.Helpers;
using ReQuesty.Runtime.Abstractions.Serialization;

namespace ReQuesty.Runtime.Serialization.Text;

/// <summary>
///   The <see cref="IParseNode"/> implementation for the text/plain content type
/// </summary>
/// <param name="text">The text value.</param>
public class TextParseNode(string? text) : IParseNode
{
    internal const string NoStructuredDataMessage = "text does not support structured data";

    private readonly string? Text = text?.Trim('"');

    /// <inheritdoc />
    public Action<IParsable>? OnBeforeAssignFieldValues { get; set; }

    /// <inheritdoc />
    public Action<IParsable>? OnAfterAssignFieldValues { get; set; }

    /// <inheritdoc />
    public bool? GetBoolValue() => bool.TryParse(Text, out bool result) ? result : null;

    /// <inheritdoc />
    public byte[]? GetByteArrayValue() => string.IsNullOrEmpty(Text) ? null : Convert.FromBase64String(Text);

    /// <inheritdoc />
    public byte? GetByteValue() => byte.TryParse(Text, out byte result) ? result : null;

    /// <inheritdoc />
    public IParseNode GetChildNode(string identifier) => throw new InvalidOperationException(NoStructuredDataMessage);

    /// <inheritdoc />
    public IEnumerable<T> GetCollectionOfObjectValues<T>(ParsableFactory<T> factory) where T : IParsable => throw new InvalidOperationException(NoStructuredDataMessage);

    /// <inheritdoc />
    public IEnumerable<T> GetCollectionOfPrimitiveValues<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>() => throw new InvalidOperationException(NoStructuredDataMessage);

    /// <inheritdoc />
    public DateTimeOffset? GetDateTimeOffsetValue() => DateTimeOffset.TryParse(Text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTimeOffset result) ? result : null;

    /// <inheritdoc />
    public Date? GetDateValue() => DateTime.TryParse(Text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime result) ? new Date(result) : null;

    /// <inheritdoc />
    public decimal? GetDecimalValue() => decimal.TryParse(Text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal result) ? result : null;

    /// <inheritdoc />
    public double? GetDoubleValue() => double.TryParse(Text, NumberStyles.Number, CultureInfo.InvariantCulture, out double result) ? result : null;

    /// <inheritdoc />
    public float? GetFloatValue() => float.TryParse(Text, NumberStyles.Number, CultureInfo.InvariantCulture, out float result) ? result : null;

    /// <inheritdoc />
    public Guid? GetGuidValue() => Guid.TryParse(Text, out Guid result) ? result : null;

    /// <inheritdoc />
    public int? GetIntValue() => int.TryParse(Text, NumberStyles.Number, CultureInfo.InvariantCulture, out int result) ? result : null;

    /// <inheritdoc />
    public long? GetLongValue() => long.TryParse(Text, NumberStyles.Number, CultureInfo.InvariantCulture, out long result) ? result : null;

    /// <inheritdoc />
    public T GetObjectValue<T>(ParsableFactory<T> factory) where T : IParsable => throw new InvalidOperationException(NoStructuredDataMessage);

    /// <inheritdoc />
    public sbyte? GetSbyteValue() => sbyte.TryParse(Text, NumberStyles.Number, CultureInfo.InvariantCulture, out sbyte result) ? result : null;

    /// <inheritdoc />
    public string? GetStringValue() => Text;

    /// <inheritdoc />
    public TimeSpan? GetTimeSpanValue() => string.IsNullOrEmpty(Text) ? null : XmlConvert.ToTimeSpan(Text);

    /// <inheritdoc />
    public Time? GetTimeValue() => DateTime.TryParse(Text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime result) ? new Time(result) : null;

    /// <inheritdoc />
    public IEnumerable<T?> GetCollectionOfEnumValues<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>() where T : struct, Enum
        => throw new InvalidOperationException(NoStructuredDataMessage);

    /// <inheritdoc />
    public T? GetEnumValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>() where T : struct, Enum
        => Text is null ? null : EnumHelpers.GetEnumValue<T>(Text);
}
