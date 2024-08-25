namespace Sanctuary;

internal class DataAccessConfig<TDataAccess> : DataAccessConfig, IDataAccessBuilder<TDataAccess>
    where TDataAccess : class
{
    internal object? DataSource { get; set; }

    public IDataAccessBuilder<TDataAccess> WithDataSource<TDataSource>(TDataSource dataSource)
    {
        DataSource = dataSource;
        return this;
    }
}