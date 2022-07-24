using System.Text;

namespace DirDiff.Cli.Logging;

internal class ConsoleLogFormatter
{
    public string Format<TState>(TState state)
    {
        // this is stupid, I just want a simple logger

        if (state is IEnumerable<KeyValuePair<string, object>> enumerable)
        {
            var pairs = enumerable.ToList();

            if (pairs[^1].Key != "{OriginalFormat}")
            {
                throw new InvalidOperationException();
            }

            var format = (string)pairs[^1].Value;

            var builder = new StringBuilder();
            var pairIndex = 0;

            for (var i = 0; i < format.Length; i++)
            {
                if (format[i] == '{')
                {
                    var start = i;
                    if (i + 1 < format.Length && format[i + 1] == '{')
                    {
                        builder.Append('{');
                        i++;
                        continue;
                    }

                    for (; i < format.Length && format[i] != '}'; i++) ;

                    if (i == format.Length)
                    {
                        builder.Append(format[start..]);
                        break;
                    }

                    builder.Append(pairs[pairIndex++].Value);
                }
                else if (format[i] == '}')
                {
                    if (i + 1 < format.Length && format[i + 1] == '}')
                    {
                        builder.Append('}');
                        i++;
                        continue;
                    }

                    builder.Append(format[i]);
                }
                else
                {
                    builder.Append(format[i]);
                }
            }

            return builder.ToString();
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}
