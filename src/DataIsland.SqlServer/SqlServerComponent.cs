namespace DataIsland.SqlServer
{
    /// <summary>
    /// A component for ADO.NET SQL server.
    /// </summary>
    /// <param name="ConnectionString">A connection string to SQL server that will be used to manipulate the component (add/remove databases, load data).</param>
    public record SqlServerComponent(string ConnectionString);
}
