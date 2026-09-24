using System;
using System.Collections.Generic;
using System.Text;

namespace MarkdownPreview.Handler;

/// <summary>
/// 读写与主程序 history.json 相同的字符串数组，避免再引入 JSON 库。
/// </summary>
internal static class JsonStringList
{
    public static bool TryParse(string json, out List<string> items)
    {
        items = new List<string>();
        if (string.IsNullOrWhiteSpace(json))
        {
            return true;
        }

        var i = 0;
        Skip(json, ref i);
        if (i >= json.Length || json[i] != '[')
        {
            return false;
        }

        i++;
        while (true)
        {
            Skip(json, ref i);
            if (i >= json.Length)
            {
                return false;
            }

            if (json[i] == ']')
            {
                return true;
            }

            if (json[i] == ',')
            {
                i++;
                continue;
            }

            if (json[i] != '"')
            {
                return false;
            }

            if (!TryReadString(json, ref i, out var value))
            {
                return false;
            }

            items.Add(value);
        }
    }

    public static string Format(IList<string> items)
    {
        var sb = new StringBuilder();
        sb.Append('[').AppendLine();
        for (var i = 0; i < items.Count; i++)
        {
            sb.Append("  \"").Append(Escape(items[i])).Append('"');
            if (i < items.Count - 1)
            {
                sb.Append(',');
            }

            sb.AppendLine();
        }

        sb.Append(']');
        sb.AppendLine();
        return sb.ToString();
    }

    private static void Skip(string json, ref int i)
    {
        while (i < json.Length && char.IsWhiteSpace(json[i]))
        {
            i++;
        }
    }

    private static bool TryReadString(string json, ref int i, out string value)
    {
        value = null;
        if (i >= json.Length || json[i] != '"')
        {
            return false;
        }

        i++;
        var sb = new StringBuilder();
        while (i < json.Length)
        {
            var ch = json[i++];
            if (ch == '"')
            {
                value = sb.ToString();
                return true;
            }

            if (ch != '\\' || i >= json.Length)
            {
                sb.Append(ch);
                continue;
            }

            var esc = json[i++];
            switch (esc)
            {
                case '"':
                case '\\':
                case '/':
                    sb.Append(esc);
                    break;
                case 'b':
                    sb.Append('\b');
                    break;
                case 'f':
                    sb.Append('\f');
                    break;
                case 'n':
                    sb.Append('\n');
                    break;
                case 'r':
                    sb.Append('\r');
                    break;
                case 't':
                    sb.Append('\t');
                    break;
                case 'u':
                    if (i + 4 > json.Length)
                    {
                        return false;
                    }

                    var hex = json.Substring(i, 4);
                    i += 4;
                    if (!ushort.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out var code))
                    {
                        return false;
                    }

                    sb.Append((char)code);
                    break;
                default:
                    return false;
            }
        }

        return false;
    }

    private static string Escape(string value)
    {
        var sb = new StringBuilder(value.Length + 8);
        foreach (var ch in value)
        {
            switch (ch)
            {
                case '\\':
                    sb.Append("\\\\");
                    break;
                case '"':
                    sb.Append("\\\"");
                    break;
                case '\b':
                    sb.Append("\\b");
                    break;
                case '\f':
                    sb.Append("\\f");
                    break;
                case '\n':
                    sb.Append("\\n");
                    break;
                case '\r':
                    sb.Append("\\r");
                    break;
                case '\t':
                    sb.Append("\\t");
                    break;
                default:
                    if (ch < 0x20)
                    {
                        sb.Append("\\u").Append(((int)ch).ToString("x4"));
                    }
                    else
                    {
                        sb.Append(ch);
                    }

                    break;
            }
        }

        return sb.ToString();
    }
}
