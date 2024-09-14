using System.Collections.Generic;
using JetBrains.Annotations;

namespace DataIsland.SqlServer;

[PublicAPI]
public sealed record SqlServerSpec : ComponentSpec<SqlServerComponent>
{
    internal string? Collation { get; private set; }

    internal bool? ClrEnabled { get; private set; }

    internal HashSet<string> LinkedServerNames { get; private set; } = new();

    /// <summary>
    /// SQL Server must have a specified collation.
    /// </summary>
    /// <param name="collation">Desired collation name.</param>
    public SqlServerSpec WithCollation(string collation)
    {
        return this with { Collation = collation, LinkedServerNames = [.. LinkedServerNames] };
    }

    /// <summary>
    /// SQL Server must enabled/disabled CLR.
    /// </summary>
    /// <param name="clrEnabled">Desired setting of CLR.</param>
    public SqlServerSpec WithClrEnabled(bool clrEnabled)
    {
        return this with { ClrEnabled = clrEnabled, LinkedServerNames = [.. LinkedServerNames] };
    }

    /// <summary>
    /// SQL Server must have a linked server with a name <paramref name="name"/>. To
    /// require multiple, call this method multiple times.
    /// </summary>
    /// <param name="name">Name of a linked server, i.e. the <c>@server</c> parameter
    ///   of <c>sp_addlinkedserver</c>.</param>
    public SqlServerSpec WithLinkedServerName(string name)
    {
        var linkedServers = new HashSet<string>(LinkedServerNames) { name };
        return this with { LinkedServerNames = linkedServers };
    }
}
