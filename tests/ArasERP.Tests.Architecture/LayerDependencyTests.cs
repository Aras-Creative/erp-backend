using System.Reflection;
using ArasERP.BuildingBlocks.Domain.Abstractions;
using ArasERP.BuildingBlocks.Domain.Events;
using ArasERP.Modules.Inventory.Domain.Warehouses;
using FluentAssertions;
using NetArchTest.Rules;

namespace ArasERP.Tests.Architecture;

public class LayerDependencyTests
{
    private static readonly Assembly InventoryAssembly = typeof(Warehouse).Assembly;

    private static string GetFailingTypeNames(TestResult result) =>
        string.Join(Environment.NewLine, result.FailingTypeNames ?? []);

    private static void AssertRule(TestResult result) =>
        result.IsSuccessful.Should().BeTrue(GetFailingTypeNames(result));

    [Fact]
    public void Domain_ShouldNotDependOnApplication()
    {
        var result = Types
            .InAssembly(InventoryAssembly)
            .That()
            .ResideInNamespaceStartingWith("ArasERP.Modules.Inventory.Domain")
            .Should()
            .NotHaveDependencyOn("ArasERP.Modules.Inventory.Application")
            .GetResult();

        AssertRule(result);
    }

    [Fact]
    public void Domain_ShouldNotDependOnInfrastructure()
    {
        var result = Types
            .InAssembly(InventoryAssembly)
            .That()
            .ResideInNamespaceStartingWith("ArasERP.Modules.Inventory.Domain")
            .Should()
            .NotHaveDependencyOn("ArasERP.Modules.Inventory.Infrastructure")
            .GetResult();

        AssertRule(result);
    }

    [Fact]
    public void Domain_ShouldNotDependOnApiOrHost()
    {
        var result = Types
            .InAssembly(InventoryAssembly)
            .That()
            .ResideInNamespaceStartingWith("ArasERP.Modules.Inventory.Domain")
            .Should()
            .NotHaveDependencyOn("ArasERP.Modules.Inventory.Api")
            .And()
            .NotHaveDependencyOn("ArasERP.Host")
            .GetResult();

        AssertRule(result);
    }

    [Fact]
    public void Application_ShouldNotDependOnInfrastructure()
    {
        var result = Types
            .InAssembly(InventoryAssembly)
            .That()
            .ResideInNamespaceStartingWith("ArasERP.Modules.Inventory.Application")
            .Should()
            .NotHaveDependencyOn("ArasERP.Modules.Inventory.Infrastructure")
            .GetResult();

        AssertRule(result);
    }

    [Fact]
    public void Application_ShouldNotDependOnApiOrHost()
    {
        var result = Types
            .InAssembly(InventoryAssembly)
            .That()
            .ResideInNamespaceStartingWith("ArasERP.Modules.Inventory.Application")
            .Should()
            .NotHaveDependencyOn("ArasERP.Modules.Inventory.Api")
            .And()
            .NotHaveDependencyOn("ArasERP.Host")
            .GetResult();

        AssertRule(result);
    }

    [Fact]
    public void Infrastructure_ShouldNotDependOnApiOrHost()
    {
        var result = Types
            .InAssembly(
                typeof(ArasERP.Modules.Inventory.Infrastructure.DependencyInjection).Assembly
            )
            .That()
            .ResideInNamespaceStartingWith("ArasERP.Modules.Inventory.Infrastructure")
            .Should()
            .NotHaveDependencyOn("ArasERP.Modules.Inventory.Api")
            .And()
            .NotHaveDependencyOn("ArasERP.Host")
            .GetResult();

        AssertRule(result);
    }

    [Fact]
    public void Api_ShouldNotDependOnInfrastructureOrDomainOrHost()
    {
        var result = Types
            .InAssembly(
                typeof(ArasERP.Modules.Inventory.Api.Warehouses.Endpoints.WarehousesEndpoints).Assembly
            )
            .That()
            .ResideInNamespaceStartingWith("ArasERP.Modules.Inventory.Api")
            .Should()
            .NotHaveDependencyOn("ArasERP.Modules.Inventory.Infrastructure")
            .And()
            .NotHaveDependencyOn("ArasERP.Modules.Inventory.Domain")
            .And()
            .NotHaveDependencyOn("ArasERP.Host")
            .GetResult();

        AssertRule(result);
    }

    [Fact]
    public void Domain_ShouldNotDependOnBuildingBlocksApplication()
    {
        var result = Types
            .InAssembly(InventoryAssembly)
            .That()
            .ResideInNamespaceStartingWith("ArasERP.Modules.Inventory.Domain")
            .Should()
            .NotHaveDependencyOn("ArasERP.BuildingBlocks.Application")
            .GetResult();

        AssertRule(result);
    }

    [Fact]
    public void ValueObjects_ShouldInheritValueObjectAndBeSealed()
    {
        var result = Types
            .InAssembly(InventoryAssembly)
            .That()
            .ResideInNamespaceStartingWith(
                "ArasERP.Modules.Inventory.Domain.Warehouses.ValueObjects"
            )
            .Should()
            .BeSealed()
            .And()
            .Inherit(typeof(ValueObject))
            .GetResult();

        AssertRule(result);
    }

    [Fact]
    public void DomainEvents_ShouldImplementIDomainEvent()
    {
        var result = Types
            .InAssembly(InventoryAssembly)
            .That()
            .ResideInNamespaceStartingWith("ArasERP.Modules.Inventory.Domain.Warehouses.Events")
            .Should()
            .ImplementInterface(typeof(IDomainEvent))
            .GetResult();

        AssertRule(result);
    }

    [Fact]
    public void Warehouse_ShouldBeAggregateRoot()
    {
        typeof(Warehouse).BaseType.Should().Be(typeof(AggregateRoot<WarehouseId>));
    }

    [Fact]
    public void ValueObjects_ShouldBeImmutable()
    {
        var valueObjects = InventoryAssembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(ValueObject).IsAssignableFrom(t));

        valueObjects.Should().NotBeEmpty();

        foreach (var type in valueObjects)
        {
            foreach (var property in type.GetProperties())
            {
                property
                    .SetMethod.Should()
                    .BeNull($"{type.Name}.{property.Name} should not have a setter");
            }
        }
    }
}
