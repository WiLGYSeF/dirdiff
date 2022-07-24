using System.Collections.Concurrent;
using System.Runtime.Serialization;

namespace DirDiff.Utilities;

public static class EnumUtils
{
    private static readonly ConcurrentDictionary<Type, Dictionary<string, string>> _enumMemberValuesMap = new();

    /// <summary>
    /// Parses string to enum value.
    /// </summary>
    /// <typeparam name="T">Enum type.</typeparam>
    /// <param name="value">Value to parse to enum.</param>
    /// <param name="ignoreCase">Whether to ignore case when parsing.</param>
    /// <returns>Enum.</returns>
    /// <exception cref="ArgumentException">Value is not a valid enum value.</exception>
    public static T Parse<T>(string value, bool ignoreCase = true) where T : struct, IConvertible
    {
        var result = Enum.Parse<T>(value, ignoreCase);
        if (!Enum.IsDefined(typeof(T), result))
        {
            throw new ArgumentException("Value is not a valid enum value.", nameof(value));
        }
        return result;
    }

    /// <summary>
    /// Tries to parse string to enum value.
    /// </summary>
    /// <typeparam name="T">Enum type.</typeparam>
    /// <param name="value">Value to parse to enum.</param>
    /// <param name="result">Parse result.</param>
    /// <param name="ignoreCase">Whether to ignore case when parsing.</param>
    /// <returns><c>true</c> if parsing was successful, <c>false</c> otherwise.</returns>
    public static bool TryParse<T>(string value, out T result, bool ignoreCase = true) where T : struct, IConvertible
    {
        try
        {
            result = Parse<T>(value, ignoreCase);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <summary>
    /// Parses enum member string to enum value.
    /// </summary>
    /// <typeparam name="T">Enum type.</typeparam>
    /// <param name="value">Enum member value to parse to enum.</param>
    /// <param name="ignoreCase">Whether to ignore case when parsing.</param>
    /// <param name="enumMemberValueRequired">
    /// Whether to throw if no matching enum member value is found.
    /// Otherwise try to parse with <see cref="Parse{T}(string, bool)"/>.</param>
    /// <returns>Enum.</returns>
    /// <exception cref="ArgumentException">Value does not match any <see cref="EnumMemberAttribute"/> value</exception>
    public static T ParseEnumMemberValue<T>(
        string value,
        bool ignoreCase = true,
        bool enumMemberValueRequired = false) where T : struct, IConvertible
    {
        var type = typeof(T);
        if (!_enumMemberValuesMap.TryGetValue(type, out var enumMemberValues))
        {
            enumMemberValues = type.GetFields()
                .Select(f => (f.Name, f.GetCustomAttributes(false).OfType<EnumMemberAttribute>().SingleOrDefault()?.Value))
                .Where(p => p.Value != null)
                .ToDictionary(e => e.Value!, e => e.Name);
            _enumMemberValuesMap[type] = enumMemberValues;
        }

        string? enumMemberValue = null;

        if (ignoreCase)
        {
            foreach (var pair in enumMemberValues)
            {
                if (pair.Key.Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    enumMemberValue = pair.Value;
                    break;
                }
            }
        }
        else
        {
            enumMemberValues.TryGetValue(value, out enumMemberValue);
        }

        if (enumMemberValue == null && enumMemberValueRequired)
        {
            throw new ArgumentException("Value does not match any EnumMember value", nameof(value));
        }

        return Parse<T>(enumMemberValue ?? value, ignoreCase);
    }

    /// <summary>
    /// Tries to parse enum member string to enum value.
    /// </summary>
    /// <typeparam name="T">Enum type.</typeparam>
    /// <param name="value">Enum member value to parse to enum.</param>
    /// <param name="result">Parse result.</param>
    /// <param name="ignoreCase">Whether to ignore case when parsing.</param>
    /// <param name="enumMemberValueRequired">
    /// Whether to throw if no matching enum member value is found.
    /// Otherwise try to parse with <see cref="Parse{T}(string, bool)"/>.</param>
    /// <returns><c>true</c> if parsing was successful, <c>false</c> otherwise.</returns>
    public static bool TryParseEnumMemberValue<T>(
        string value,
        out T result,
        bool ignoreCase = true,
        bool enumMemberValueRequired = false) where T : struct, IConvertible
    {
        try
        {
            result = ParseEnumMemberValue<T>(value, ignoreCase, enumMemberValueRequired);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }
}
