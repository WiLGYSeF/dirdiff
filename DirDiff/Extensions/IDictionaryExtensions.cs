using System.Diagnostics.CodeAnalysis;

namespace DirDiff.Extensions;

internal static class IDictionaryExtensions
{
    public static bool TryGetValueAs<TKey, TValue, TValueAs>(
        this IDictionary<TKey, TValue> dictionary,
        TKey key,
        [MaybeNull] out TValueAs valueAs) where TValueAs : TValue
    {
        if (!dictionary.TryGetValue(key, out var value))
        {
            valueAs = default;
            return false;
        }

        valueAs = value == null ? default : (TValueAs)value;
        return true;
    }
}
