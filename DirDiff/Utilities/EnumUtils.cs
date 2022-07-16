using System.Collections.Concurrent;
using System.Runtime.Serialization;

namespace DirDiff.Utilities;

internal static class EnumUtils
{
    private static readonly ConcurrentDictionary<Type, Dictionary<string, string>> _enumMemberValuesMap = new();

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

        return Enum.Parse<T>(enumMemberValue ?? value, ignoreCase);
    }
}
