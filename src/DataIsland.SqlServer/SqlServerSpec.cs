using JetBrains.Annotations;

namespace DataIsland.SqlServer;

[PublicAPI]
public sealed record SqlServerSpec : ComponentSpec<SqlServerComponent>
{
    internal string? Collation { get; private set; }

    internal bool? ClrEnabled { get; private set; }

    /// <summary>
    /// SQL Server must have a specified collation.
    /// </summary>
    /// <param name="collation">Desired collation name.</param>
    public SqlServerSpec WithCollation(string collation)
    {
        return this with { Collation = collation };
    }

    /// <summary>
    /// SQL Server must enabled/disabled CLR.
    /// </summary>
    /// <param name="clrEnabled">Desired setting of CLR.</param>
    public SqlServerSpec WithClrEnabled(bool clrEnabled)
    {
        return this with { ClrEnabled = clrEnabled };
    }
}
