using Autofac;
using LibSqlite3Orm.IntegrationTests.TestDataModel;

namespace LibSqlite3Orm.IntegrationTests;

public static class ContainerRegistration
{
    public static IContainer RegisterDependencies()
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule<ContainerModule>();
        builder.RegisterType<TestDbContext>().InstancePerDependency();
        return builder.Build();
    }
}