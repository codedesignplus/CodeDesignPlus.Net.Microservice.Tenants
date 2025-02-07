using System;
using FluentValidation.TestHelper;
using CodeDesignPlus.Net.Microservice.Tenants.Application.Tenant.Queries.GetTenantById;
using Xunit;

namespace CodeDesignPlus.Net.Microservice.Tenants.Application.Test.Tenant.Queries.GetTenantById;

public class GetTenantByIdQueryTest
{
    private readonly Validator validator;

    public GetTenantByIdQueryTest()
    {
        validator = new Validator();
    }

    [Fact]
    public void Should_Have_Error_When_Id_Is_Empty()
    {
        // Arrange
        var query = new GetTenantByIdQuery(Guid.Empty);

        // Act
        var result = validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Id_Is_Not_Empty()
    {
        // Arrange
        var query = new GetTenantByIdQuery(Guid.NewGuid());

        // Act
        var result = validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }
}
