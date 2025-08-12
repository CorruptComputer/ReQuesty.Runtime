using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;

namespace ReQuesty.Runtime.Abstractions.Helpers;

/// <summary>
///   Helper methods for enums
/// </summary>
public static class EnumHelpers
{
    /// <summary>
    ///   Gets the enum value from the raw value
    /// </summary>
    /// <typeparam name="T">Enum type</typeparam>
    /// <param name="rawValue">Raw value</param>
    /// <returns></returns>
    public static T? GetEnumValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>(string rawValue) where T : struct, Enum
    {
        if (string.IsNullOrEmpty(rawValue))
        {
            return null;
        }

        rawValue = ToEnumRawName<T>(rawValue!);
        if (typeof(T).IsDefined(typeof(FlagsAttribute)))
        {
            ReadOnlySpan<char> valueSpan = rawValue.AsSpan();
            int value = 0;
            while (valueSpan.Length > 0)
            {
                int commaIndex = valueSpan.IndexOf(',');
                ReadOnlySpan<char> valueNameSpan = commaIndex < 0 ? valueSpan : valueSpan.Slice(0, commaIndex);
                valueNameSpan = ToEnumRawName<T>(valueNameSpan);
                if (Enum.TryParse<T>(valueNameSpan, true, out T result))
                {
                    value |= (int)(object)result;
                }

                valueSpan = commaIndex < 0 ? [] : valueSpan.Slice(commaIndex + 1);
            }
            return (T)(object)value;
        }
        else
        {
            return Enum.TryParse<T>(rawValue, true, out T result) ? result : null;
        }
    }

    private static string ToEnumRawName<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>(string value) where T : struct, Enum
    {
        return TryGetFieldValueName(typeof(T), value, out string? val) ? val : value;
    }

    private static ReadOnlySpan<char> ToEnumRawName<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>(ReadOnlySpan<char> span) where T : struct, Enum
    {
        return TryGetFieldValueName(typeof(T), span.ToString(), out string? val) ? val.AsSpan() : span;
    }

    /// <summary>
    ///   Gets the enum value from the raw value for the given type
    /// </summary>
    /// <param name="type">Enum type</param>
    /// <param name="rawValue">Raw value</param>
    /// <returns></returns>
    public static object? GetEnumValue([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] Type? type, string rawValue)
    {
        object? result;
        if (type == null)
        {
            return null;
        }
        Type enumType = (Nullable.GetUnderlyingType(type) is { IsEnum: true } underlyingType) ? underlyingType : type;
        if (enumType.IsDefined(typeof(FlagsAttribute)))
        {
            int intValue = 0;
            while (rawValue.Length > 0)
            {
                int commaIndex = rawValue.IndexOf(',');
                string valueName = commaIndex < 0 ? rawValue : rawValue.Substring(0, commaIndex);
                if (TryGetFieldValueName(enumType, valueName, out string? value))
                {
                    valueName = value;
                }

                if (Enum.TryParse(enumType, valueName, true, out object? enumPartResult))
                {
                    intValue |= (int)enumPartResult!;
                }

                rawValue = commaIndex < 0 ? string.Empty : rawValue.Substring(commaIndex + 1);
            }
            result = intValue > 0 ? Enum.Parse(enumType, intValue.ToString(), true) : null;
        }
        else
        {
            if (TryGetFieldValueName(enumType, rawValue, out string? value))
            {
                rawValue = value;
            }

            Enum.TryParse(enumType, rawValue, true, out object? enumResult);
            result = enumResult;
        }
        return result;
    }

    private static bool TryGetFieldValueName([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] Type type, string rawValue, out string valueName)
    {
        valueName = string.Empty;
        foreach (FieldInfo field in type.GetFields())
        {
            if (field.GetCustomAttribute<EnumMemberAttribute>() is { } attr && rawValue.Equals(attr.Value, StringComparison.Ordinal))
            {
                valueName = field.Name;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    ///   Gets the enum string representation of the given value. Looks up if there is an <see cref="EnumMemberAttribute"/> and returns the value if found, otherwise returns the enum name in camel case.
    /// </summary>
    /// <typeparam name="T">The Enum type</typeparam>
    /// <param name="value">The enum value</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">If value is null</exception>
    public static string? GetEnumStringValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>(T value) where T : struct, Enum
    {
        Type type = typeof(T);

        if (Enum.GetName(type, value) is not { } name)
        {
            throw new ArgumentException($"Invalid Enum value {value} for enum of type {type}");
        }

        if (type.GetField(name)?.GetCustomAttribute<EnumMemberAttribute>() is { } attribute)
        {
            return attribute.Value;
        }

        return name;
    }
}
