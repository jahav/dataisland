# Sancturary

Sanctuary is a project to enable parallel execution of integration tests.

Integration tests need external components (e.g. database). External components are hard and slow to instantiate and prepare.
The integration tests therefore have to balance two competing requirements:

* Test speed
* Test isolation

There are three strategies:
* Use fakes in place of real dependencies. E.g. [Integration tests in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)
  recommend to use SQLite instead of real database backend. That is fine for simple cases, but different feature-set of databases causes problems (stored
  procedures, transactions, available functions).
* Use real external dependency and execute tests sequentially. This approach works, but is really slow and will get slower with growing number of tests.
  This also necessites a need to ensure clean the state of dependency after each test. There are libraries that help with that, e.g. [Respawn](https://github.com/jbogard/Respawn).
* Instantiate a separate dependency for each test. This is seriously problematic because of resources and slow startup time and is least practical one.
  E.g. SQL Server docker requires 1.1 GB and takes tens of seconds to start.

Sanctuary tries to solve this problem by taking advantage of multi-tenancy available to most data stores. It's expensive to spin-up a new SQL Server, but
very fast to create empty database.

The test project has to define what kind of data sources it requires and dataset it shoudl use to initialize them and after that, just run your tests. Sanctuary
hooks up into DI to create a new tenants in data sources and adjusts the connections for data access frameworks (e.g. `EFCore`).

|  Component    | Tenant       | Data access              |
|---------------|--------------|--------------------------|
|  SQL Server   | Database     | EfCore, Dapper, ADO.NET  |
|  RabbitMQ     | Virtual Host | MassTransit, NServiceBus |
|  Blob Storage | Container    | TwentyTwenty.Storage     |

