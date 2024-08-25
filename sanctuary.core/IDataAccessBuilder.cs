namespace Sanctuary;

public interface IDataAccessBuilder<TDataAccess>
{
    public IDataAccessBuilder<TDataAccess> WithDataSource<TDataSource>(TDataSource dataSource);
}