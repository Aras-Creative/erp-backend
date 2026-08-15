using System.Reflection;
using ArasERP.Modules.Inventory.Contracts.Warehouses;
using FluentAssertions;
using NetArchTest.Rules;

namespace ArasERP.Tests.Architecture;

public class ContractDependencyTests
{
    private static readonly Assembly ContractsAssembly = typeof(CreateWarehouseRequest).Assembly;

    private static string GetFailingTypeNames(TestResult result) =>
        string.Join(Environment.NewLine, result.FailingTypeNames ?? []);

    [Fact]
    public void Contracts_ShouldNotDependOnOtherLayers()
    {
        var result = Types
            .InAssembly(ContractsAssembly)
            .Should()
            .NotHaveDependencyOn("ArasERP.Modules.Inventory.Domain")
            .And()
            .NotHaveDependencyOn("ArasERP.Modules.Inventory.Application")
            .And()
            .NotHaveDependencyOn("ArasERP.Modules.Inventory.Infrastructure")
            .And()
            .NotHaveDependencyOn("ArasERP.Modules.Inventory.Api")
            .And()
            .NotHaveDependencyOn("ArasERP.BuildingBlocks")
            .And()
            .NotHaveDependencyOn("ArasERP.Host")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(GetFailingTypeNames(result));
    }
}
