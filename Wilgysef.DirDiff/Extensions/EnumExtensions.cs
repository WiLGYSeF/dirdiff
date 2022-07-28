using System.Collections.Concurrent;
using System.Runtime.Serialization;

namespace Wilgysef.DirDiff.Extensions;

internal static class EnumExtensions
{
    private static readonly ConcurrentDictionary<string, string?> _enumMemberValues = new();

    public static string ToEnumMemberValue(this Enum @enum, bool required = false)
    {
        var stringValue = @enum.ToString();
        var type = @enum.GetType();
        var cacheName = $"{type.FullName}:{stringValue}";

        if (!_enumMemberValues.TryGetValue(cacheName, out var value))
        {
            value = type.GetMember(stringValue)
                .FirstOrDefault()
                ?.GetCustomAttributes(false).OfType<EnumMemberAttribute>()
                .SingleOrDefault()?.Value;
            _enumMemberValues[cacheName] = value;
        }

        if (required && value == null)
        {
            throw new ArgumentException("Enum value does not have an EnumMember value", nameof(@enum));
        }

        return value ?? stringValue;
    }
}
