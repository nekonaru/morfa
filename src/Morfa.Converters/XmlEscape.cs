// XML-escapes text and removes characters XML 1.0 does not allow.

using System.Security;
using System.Text;

namespace Morfa.Converters;

internal static class XmlEscape
{
    /// <summary>XML-escapes text and drops characters disallowed by XML 1.0.</summary>
    public static string Escape(string text)
    {
        var clean = new StringBuilder(text.Length);
        foreach (var ch in text)
        {
            if (ch is '\t' or '\n' or '\r' || (ch >= 0x20 && ch != 0xFFFE && ch != 0xFFFF))
                clean.Append(ch);
        }
        return SecurityElement.Escape(clean.ToString()) ?? "";
    }
}
