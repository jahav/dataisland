using System.Collections.Generic;
using System.Linq;

namespace DataIsland.SqlServer;

internal static class SysnameExtensions
{
    private const string StartMaker = "[";
    private const string EndMarker = "]";
    private const string Separator = "], [";

    public static string ToSysnameList(this IEnumerable<string> list)
    {
        return StartMaker + StartMaker + string.Join(Separator, list.Select(EscapeSysname)) + EndMarker;
    }

    public static string EscapeSysname(this string sysname)
    {
        // Sysname can't be null, it has automatic NOT NULL constraint
        return sysname.Replace(StartMaker, "[[").Replace(EndMarker, "]]");
    }
}
