How does it work?
=================

Principle
---------

DataIsland is designed to manage test environments efficiently by creating isolated tenants for each test case. Tenants could include SQL databases, RabbitMQ virtual hosts, or blob containers. Creating these tenants is quick and resource-light, typically taking only a few hundred milliseconds and a few megabytes of storage.

However, setting up the components that host these tenants—such as SQL Server, RabbitMQ, or Azure Storage emulators—is more resource-intensive. This setup usually requires hundreds of megabytes or even gigabytes of memory and tens of seconds for initialization.

Given the high cost of component setup, it's impractical to instantiate a new component for each test. Instead, DataIsland has a pool of instance of each type of component and reuses them across tests.

Here’s how it works:

#. **Developer Specification:** Developers provide DataIsland with details on the tenants required for their application and how these tenants are accessed.
#. **Tenant Management:** Data Island uses this information to allocate a unique tenants for each test. It ensures that tenant data is isolated and does not overlap with other tests.
#. **Component Reuse:** While components are not recreated, Data Island manages their usage efficiently. It ensures that each test operates in isolation by handling dependency injection and tenant assignment.

.. mermaid:: xunit-execution.mmd
